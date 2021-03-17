using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Operators;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Lua.Parsing {

    // https://www.lua.org/manual/5.1/manual.html

    public static class LuaParser {

        private enum LuaTokenType {
            Undefined = 0,
            Number,
            Integer,
            Bool,
            Nil,
            String,
            Quote,
            Whitespace,
            Identifier,
            TableOpen,
            TableClose,
            IndexOpen,
            IndexClose,
            ExprOpen,
            ExprClose,
            Comma,
            Equals,
            Semicolon,
            Dot,
            Comment,
            StdOperator,
            RelOperator,
            Or,
            And,
            Not,
            Concat,
            VarArgs,
            Keyword,
        }

        private record LuaToken(LuaTokenType Type, string Val);

        private static readonly Regex LuaRegex 
            = new Regex(@"(?<c>--.*\n)|(?<n>\d*\.\d+)|(?<i>\d+)|(?<b>true|false)|(?<nil>nil)|(?<op>(=|,|\+|-|\*|/|;|\.|#|<|>|~|\^|:|%)+)|(?<e>\(|\))|(?<t>\{|\}|\[|\])|(?<id>(_|\w)(_|\d|\w)*)|(?<s>\"".*?\"")");

        public static List<LuaExpr> ParseLuaSource(string sourceText) {
            
            // Tokenize
            var tokens = Tokenize(sourceText);
            
            // Convert into epressions
            List<LuaExpr> expressions = new List<LuaExpr>();
            for (int i = 0; i < tokens.Count; i++) {
                expressions.Add(tokens[i].Type switch {
                    LuaTokenType.Bool => new LuaConstValueExpr(new LuaBool(bool.Parse(tokens[i].Val))),
                    LuaTokenType.Identifier => new LuaIdentifierExpr(tokens[i].Val),
                    LuaTokenType.Integer => new LuaConstValueExpr(new LuaNumber(int.Parse(tokens[i].Val))),
                    LuaTokenType.Nil => new LuaConstValueExpr(new LuaNil()),
                    LuaTokenType.String => new LuaConstValueExpr(new LuaString(tokens[i].Val)),
                    LuaTokenType.Number => new LuaConstValueExpr(new LuaNumber(double.Parse(tokens[i].Val))),
                    LuaTokenType.Comma or LuaTokenType.Equals or LuaTokenType.IndexClose or 
                    LuaTokenType.IndexOpen or LuaTokenType.Semicolon or LuaTokenType.TableClose or
                    LuaTokenType.TableOpen or LuaTokenType.Dot or LuaTokenType.ExprOpen or
                    LuaTokenType.ExprClose => new LuaOpExpr(tokens[i].Type),
                    LuaTokenType.Comment => new LuaComment(tokens[i].Val),
                    LuaTokenType.StdOperator or LuaTokenType.RelOperator => new LuaOpExpr(tokens[i].Val),
                    LuaTokenType.Keyword => new LuaKeyword(tokens[i].Val),
                    _ => throw new Exception(),
                });
                if (expressions[^1] is LuaComment) {
                    expressions.RemoveAt(expressions.Count - 1);
                }
            }

            // Apply groups
            ApplyGroup(expressions, 0, LuaTokenType.ExprOpen, LuaTokenType.ExprClose, CreateExprGroup);
            ApplyGroup(expressions, 0, LuaTokenType.TableOpen, LuaTokenType.TableClose, x => new LuaTableExpr(x));
            ApplyGroup(expressions, 0, LuaTokenType.IndexOpen, LuaTokenType.IndexClose, CreateIndexer);

            // Apply scope groups
            _ = ApplyScopeGroups(expressions, 0);

            // Apply OOP
            ApplyOrderOfOperations(expressions);

            // Apply grammar
            ApplyGrammar(expressions, 0);

            // Apply implicit behaviour (like implict indexing)
            ApplyImplicitBehaviour(expressions, 0);

            // Apply late-syntax check
            ApplyLateSyntaxCheck(expressions, 0);

            // Return expressions
            return expressions;

        }

        static List<LuaExpr> ApplyScopeGroups(List<LuaExpr> luaExprs, int i) {
            List<LuaExpr> result = new List<LuaExpr>();
            while (i < luaExprs.Count) {
                if (luaExprs[i] is LuaKeyword { Keyword: "end" }) {
                    return result;
                } else if (luaExprs[i] is LuaKeyword { Keyword: "function" }) {
                    // Handle functions
                    if (i + 1 < luaExprs.Count) {
                        if (luaExprs[i + 1] is LuaArguments lfargs) {
                            
                            // Get body
                            var body = ApplyScopeGroups(luaExprs, i + 2);

                            // Form func
                            luaExprs[i] = new LuaFuncExpr(lfargs, new LuaScope(body));

                            // Remove body + args
                            luaExprs.RemoveRange(i + 1, body.Count+2);

                        } else if (luaExprs[i + 1] is LuaIdentifierExpr) {
                            
                            // Re-order and handle later
                            var tmp = luaExprs[i];
                            luaExprs[i] = luaExprs[i + 1];
                            luaExprs[i + 1] = new LuaOpExpr(LuaTokenType.Equals);
                            luaExprs.Insert(i + 2, tmp);

                        } else {
                            throw new LuaSyntaxError(); // TODO: Error message
                        }
                    } else {
                        throw new LuaSyntaxError("Identifier expected but found EOF");
                    }

                } else {
                    result.Add(luaExprs[i]);
                }
                i++;
            }
            return result;
        }

        static void ApplyOrderOfOperations(List<LuaExpr> luaExprs) {

            ILuaOperatorSyntax[][] order = new ILuaOperatorSyntax[][] {
                new ILuaOperatorSyntax[] { new LuaUnaryOperatorSyntax("-", LuaUnaryPosition.Prefix) },
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("*"), new LuaBinaryOperatorSyntax("/") },
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("+"), new LuaBinaryOperatorSyntax("-") },
                new ILuaOperatorSyntax[] {
                    new LuaBinaryOperatorSyntax("<"), new LuaBinaryOperatorSyntax("<="), new LuaBinaryOperatorSyntax(">"), new LuaBinaryOperatorSyntax(">="),
                    new LuaBinaryOperatorSyntax("=="), new LuaBinaryOperatorSyntax("!=")
                },
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("or") },
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("=") },
            };

            for (int i = 0; i < order.Length; i++) {

                var ops = order[i];

                int j = 0;
                while (j < luaExprs.Count) {
                    if (luaExprs[j] is LuaOpExpr opExpr && opExpr.Type is string sym) {
                        if (ops.FirstOrDefault(x => x.OperatorSymbol == sym) is ILuaOperatorSyntax opsyntax) {
                            bool pre = j - 1 >= 0;
                            bool post = j + 1 < luaExprs.Count;
                            if (opsyntax.PrePostCondtion(pre, post)) {
                                if (opsyntax.Apply(luaExprs, j, ApplyOrderOfOperations)) {
                                    continue;
                                }
                            }
                        }
                    }
                    j++;
                }

            }

            // Run recursively
            for (int i = 0; i < luaExprs.Count; i++) {
                if (luaExprs[i] is LuaTableExpr tableNode) {
                    ApplyOrderOfOperations(tableNode.SubExpressions);
                } else if (luaExprs[i] is LuaScope scopeNode) {
                    ApplyOrderOfOperations(scopeNode.ScopeBody);
                } else if (luaExprs[i] is LuaFuncExpr funcExpr) {
                    ApplyOrderOfOperations(funcExpr.Arguments.Arguments);
                    ApplyOrderOfOperations(funcExpr.Body.ScopeBody);
                }
            }

        }

        static void ApplyLateSyntaxCheck(List<LuaExpr> luaExprs, int i) {
            static void ApplyBinop(LuaBinaryExpr binop) {
                if (binop.Left is LuaTableExpr lt) { ApplyTable(lt); }
                if (binop.Right is LuaTableExpr lr) { ApplyTable(lr); }
            }
            static void ApplyTable(LuaTableExpr table) {
                int j = 0;
                while (j < table.SubExpressions.Count) {
                    if (table.SubExpressions[j] is LuaTableExpr sub) {
                        ApplyLateSyntaxCheck(sub.SubExpressions, 0);
                    } else if (table.SubExpressions[j] is LuaBinaryExpr tablebin) {
                        ApplyBinop(tablebin);
                    }
                    if (j + 1 < table.SubExpressions.Count && table.SubExpressions[j + 1] is not LuaOpExpr { Type: LuaTokenType.Comma }) {
                        throw new LuaSyntaxError("',' expected.", $"Insert ',' following '{table.SubExpressions[j]}'");
                    } else {
                        if (j + 1 < table.SubExpressions.Count) {
                            table.SubExpressions.RemoveAt(j + 1);
                        }
                    }
                    j++;
                }
            }
            while (i < luaExprs.Count) {
                if (luaExprs[i] is LuaTableExpr table) {
                    ApplyTable(table);
                } else if (luaExprs[i] is LuaBinaryExpr binop) {
                    ApplyBinop(binop);
                }
                i++;
            }
        }

        static void ApplyGrammar(List<LuaExpr> luaExprs, int i) {
            while (i < luaExprs.Count) {
                bool hasNext = i + 1 < luaExprs.Count;
                bool hasPrev = i - 1 >= 0;
                if (hasNext && hasPrev && luaExprs[i] is LuaOpExpr { Type: LuaTokenType.Equals }) {
                    if (luaExprs[i + 1] is LuaTableExpr tableExpr) {
                        ApplyGrammar(tableExpr.SubExpressions, 0);
                    } else if (luaExprs[i + 1] is LuaFuncExpr lfunc) {
                        ApplyGrammar(lfunc.Arguments.Arguments, 0);
                        ApplyGrammar(lfunc.Body.ScopeBody, 0);
                    }
                    luaExprs[i - 1] = new LuaBinaryExpr(luaExprs[i - 1], luaExprs[i + 1], "=");
                    luaExprs.RemoveRange(i, 2);
                } else if (hasNext && hasPrev && luaExprs[i] is LuaOpExpr op && op.Type is string opStr) {
                    if (luaExprs[i + 1] is LuaTableExpr tableExpr) {
                        ApplyGrammar(tableExpr.SubExpressions, 0);
                    }
                    luaExprs[i - 1] = new LuaBinaryExpr(luaExprs[i - 1], luaExprs[i + 1], opStr);
                    luaExprs.RemoveRange(i, 2);
                } else if (hasNext && hasPrev && luaExprs[i] is LuaOpExpr dotOp && (LuaTokenType)dotOp.Type == LuaTokenType.Dot) {
                    if (luaExprs[i + 1] is LuaTableExpr tableExpr) {
                        ApplyGrammar(tableExpr.SubExpressions, 0);
                    }
                    luaExprs[i - 1] = new LuaLookupExpr(luaExprs[i - 1], luaExprs[i + 1] as LuaLookupIdExpr);
                    luaExprs.RemoveRange(i, 2);
                } else if (hasPrev && luaExprs[i] is LuaIndexExpr && luaExprs[i - 1] is not LuaOpExpr { Type: LuaTokenType.Comma }) {
                    luaExprs[i - 1] = new LuaLookupExpr(luaExprs[i - 1], luaExprs[i] as LuaLookupIdExpr);
                    luaExprs.RemoveAt(i);
                } else if (hasNext && luaExprs[i] is LuaIdentifierExpr or LuaLookupIdExpr && luaExprs[i + 1] is LuaArguments largs) {
                    luaExprs[i] = new LuaCallExpr(luaExprs[i], largs);
                    luaExprs.RemoveAt(i + 1);
                } else {
                    if (luaExprs[i] is LuaTableExpr singleTable) {
                        ApplyGrammar(singleTable.SubExpressions, 0);
                    } else if (luaExprs[i] is LuaScope scope) {
                        ApplyGrammar(scope.ScopeBody, 0);
                    }
                    i++;
                }
            }
        }

        static void ApplyImplicitBehaviour(List<LuaExpr> luaExprs, int i) {
            static void FixIndex(LuaTableExpr luaTable) {
                int j = 0; // using j so we dont confuse with i from outer scope
                int k = 1; // Lua indexing starts from 1
                while (j < luaTable.SubExpressions.Count){
                    if (luaTable.SubExpressions[j] is LuaConstValueExpr or LuaTableExpr or LuaNegateExpr) {
                        luaTable.SubExpressions[j] = new LuaBinaryExpr(new LuaIndexExpr(new LuaConstValueExpr(new LuaNumber(k))), luaTable.SubExpressions[j], "=");
                        k++;
                    } else if (luaTable.SubExpressions[j] is LuaBinaryExpr binaryExpr) {
                        if (binaryExpr.Operator != "=") {
                            luaTable.SubExpressions[j] = new LuaBinaryExpr(new LuaIndexExpr(new LuaConstValueExpr(new LuaNumber(k))), luaTable.SubExpressions[j], "=");
                            k++;
                        }
                    }
                    j++;
                }
            }
            static void TableImplicits(LuaTableExpr table) {
                FixIndex(table);
                ApplyImplicitBehaviour(table.SubExpressions, 0);
            }
            while(i < luaExprs.Count) {
                if (luaExprs[i] is LuaTableExpr t) {
                    TableImplicits(t);
                } else if (luaExprs[i] is LuaBinaryExpr binop) {
                    if (binop.Left is LuaTableExpr tl) {
                        TableImplicits(tl);
                    }
                    if (binop.Right is LuaTableExpr tr) {
                        TableImplicits(tr);
                    }
                }
                i++;
            }
        }

        private static LuaIndexExpr CreateIndexer(List<LuaExpr> exprs) {
            if (exprs.Count == 1) {
                return new LuaIndexExpr(exprs[0]);
            } else {
                throw new LuaSyntaxError();
            }
        }

        private static LuaExpr CreateExprGroup(List<LuaExpr> exprs) {
            if (exprs.Count == 0) {
                return new LuaEmptyParenthesisGroup();
            } else {
                if (exprs.Any(x => x is LuaOpExpr { Type: LuaTokenType.Comma } )){
                    return new LuaArguments(exprs);
                } else {
                    return new LuaSingleElementParenthesisGroup(exprs);
                }
            }
        }

        private static void ApplyGroup(List<LuaExpr> expressions, int i, LuaTokenType open, LuaTokenType close, Func<List<LuaExpr>, LuaExpr> creator) {

            while (i < expressions.Count) {
                if (expressions[i] is LuaOpExpr syntaxOpenExpr && syntaxOpenExpr.Type is LuaTokenType t && t == open) {
                    var sub = new List<LuaExpr>();
                    int j = i + 1;
                    int rem = 0;
                    while (true) {
                        if (expressions[j] is LuaOpExpr syntaxEndExpr && syntaxEndExpr.Type is LuaTokenType e && e == close) {
                            break;
                        } else if (expressions[j] is LuaOpExpr syntaxSubOpenExpr && syntaxSubOpenExpr.Type is LuaTokenType subStart && subStart == open) {
                            int k = j;
                            ApplyGroup(expressions, k, open, close, creator);
                            sub.Add(expressions[j]);
                            j++;
                            rem++;
                        } else {
                            sub.Add(expressions[j]);
                            j++;
                            rem++;
                        }
                    }

                    expressions.RemoveRange(i + 1, rem + 1);
                    expressions[i] = creator(sub);

                } else if (expressions[i] is LuaTableExpr table) {
                    int j = 0;
                    ApplyGroup(table.SubExpressions, j, open, close, creator);
                }

                i++;

            }

        }

        private static List<LuaToken> Tokenize(string source) {

            List<LuaToken> tokens = new List<LuaToken>();

            var matches = LuaRegex.Matches(source);
            foreach (RegexMatch match in matches) {

                string type = "0";
                foreach (Group g in match.Groups) {
                    if (!string.IsNullOrEmpty(g.Value) && g.Name.CompareTo("0") != 0) {
                        type = g.Name;
                    }
                }

                tokens.Add(type switch { 
                    "n" => new LuaToken(LuaTokenType.Number, match.Value),
                    "i" => new LuaToken(LuaTokenType.Integer, match.Value),
                    "b" => new LuaToken(LuaTokenType.Bool, match.Value),
                    "c" => new LuaToken(LuaTokenType.Comment, match.Value),
                    "t" => new LuaToken(match.Value switch {
                        "[" => LuaTokenType.IndexOpen,
                        "]" => LuaTokenType.IndexClose,
                        "{" => LuaTokenType.TableOpen,
                        "}" => LuaTokenType.TableClose,
                        _ => throw new Exception()
                    }, match.Value),
                    "e" => new LuaToken(match.Value switch {
                        "(" => LuaTokenType.ExprOpen,
                        ")" => LuaTokenType.ExprClose,
                        _ => throw new Exception()
                    }, match.Value),
                    "s" => new LuaToken(LuaTokenType.String, match.Value.Trim('\"')),
                    "op" => new LuaToken(match.Value switch {
                        "," => LuaTokenType.Comma,
                        "=" => LuaTokenType.Equals,
                        ";" => LuaTokenType.Semicolon,
                        "." => LuaTokenType.Dot,
                        ".." => LuaTokenType.Concat,
                        "..." => LuaTokenType.VarArgs,
                        "+" => LuaTokenType.StdOperator,
                        "-" => LuaTokenType.StdOperator,
                        "*" => LuaTokenType.StdOperator,
                        "/" => LuaTokenType.StdOperator,
                        "#" => LuaTokenType.StdOperator,
                        "%" => LuaTokenType.StdOperator,
                        "^" => LuaTokenType.StdOperator,
                        "==" => LuaTokenType.RelOperator,
                        "~=" => LuaTokenType.RelOperator,
                        "<=" => LuaTokenType.RelOperator,
                        ">=" => LuaTokenType.RelOperator,
                        "<" => LuaTokenType.RelOperator,
                        ">" => LuaTokenType.RelOperator,
                        "\"" => LuaTokenType.Quote,
                        _ => throw new Exception()
                    }, match.Value),
                    "id" => match.Value switch {
                        "and" => new LuaToken(LuaTokenType.And, match.Value),
                        "not" => new LuaToken(LuaTokenType.Not, match.Value),
                        "or" => new LuaToken(LuaTokenType.Or, match.Value),
                        "if" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "else" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "elseif" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "in" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "for" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "while" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "repeat" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "until" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "do" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "function" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "end" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "break" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "return" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "local" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        _ => new LuaToken(LuaTokenType.Identifier, match.Value)
                    },
                    "nil" => new LuaToken(LuaTokenType.Nil, match.Value),
                    _ => throw new Exception(),
                });

            }

            // Return tokens
            return tokens;

        }

    }

}
