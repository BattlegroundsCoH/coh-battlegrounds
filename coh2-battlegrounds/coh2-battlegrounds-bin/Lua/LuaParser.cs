using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Operators;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Lua {

    public static class LuaParser {

        private enum LuaTokenType {
            Undefined = 0,
            Number,
            Integer,
            Bool,
            Nil,
            String,
            Identifier,
            TableOpen,
            TableClose,
            IndexOpen,
            IndexClose,
            Comma,
            Equals,
            Semicolon,
            Dot,
            Comment,
            StdOperator,
            Keyword,
        }

        private record LuaToken(LuaTokenType Type, string Val);

        private static readonly Regex LuaRegex 
            = new Regex(@"(?<c>--.*\n)|(?<n>\d*\.\d+)|(?<i>\d+)|(?<b>true|false)|(?<nil>nil)|(?<op>=|,|\+|-|\*|/|;|\.)|(?<t>\{|\}|\[|\])|(?<id>\w(\d|\w)*)|(?<str>\"".*\""{1}?)");

        public static List<LuaExpr> ParseLuaSource(string sourceText) {
            
            // Tokenize
            var tokens = Tokenize(sourceText);
            
            // Convert into epressions
            List<LuaExpr> expressions = new List<LuaExpr>();
            for (int i = 0; i < tokens.Count; i++) {
                expressions.Add(tokens[i].Type switch {
                    LuaTokenType.Bool => new LuaValueExpr(new LuaBool(bool.Parse(tokens[i].Val))),
                    LuaTokenType.Identifier => new LuaIdentifierExpr(tokens[i].Val),
                    LuaTokenType.Integer => new LuaValueExpr(new LuaNumber(int.Parse(tokens[i].Val))),
                    LuaTokenType.Nil => new LuaValueExpr(new LuaNil()),
                    LuaTokenType.String => new LuaValueExpr(new LuaString(tokens[i].Val)),
                    LuaTokenType.Number => new LuaValueExpr(new LuaNumber(double.Parse(tokens[i].Val))),
                    LuaTokenType.Comma or LuaTokenType.Equals or LuaTokenType.IndexClose or 
                    LuaTokenType.IndexOpen or LuaTokenType.Semicolon or LuaTokenType.TableClose or
                    LuaTokenType.TableOpen or LuaTokenType.Dot => new LuaOpExpr(tokens[i].Type),
                    LuaTokenType.Comment => new LuaComment(tokens[i].Val),
                    LuaTokenType.StdOperator => new LuaOpExpr(tokens[i].Val),
                    _ => throw new Exception(),
                });
                if (expressions[^1] is LuaComment) {
                    expressions.RemoveAt(expressions.Count - 1);
                }
            }
            
            // Apply groups
            ApplyGroup(expressions, 0, LuaTokenType.TableOpen, LuaTokenType.TableClose, x => new LuaTableExpr(x));
            ApplyGroup(expressions, 0, LuaTokenType.IndexOpen, LuaTokenType.IndexClose, CreateIndexer);

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

        static void ApplyOrderOfOperations(List<LuaExpr> luaExprs) {

            ILuaOperatorSyntax[][] order = new ILuaOperatorSyntax[][] {
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("*"), new LuaBinaryOperatorSyntax("/") },
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("+"), new LuaBinaryOperatorSyntax("-") },
                new ILuaOperatorSyntax[] {
                    new LuaBinaryOperatorSyntax("<"), new LuaBinaryOperatorSyntax("<="), new LuaBinaryOperatorSyntax(">"), new LuaBinaryOperatorSyntax(">="),
                    new LuaBinaryOperatorSyntax("=="), new LuaBinaryOperatorSyntax("!=")
                },
                new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("or"), new LuaBinaryOperatorSyntax("and") },
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
                } else {
                    if (luaExprs[i] is LuaTableExpr singleTable) {
                        ApplyGrammar(singleTable.SubExpressions, 0);
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
                    if (luaTable.SubExpressions[j] is LuaValueExpr or LuaTableExpr) {
                        luaTable.SubExpressions[j] = new LuaBinaryExpr(new LuaIndexExpr(new LuaValueExpr(new LuaNumber(k))), luaTable.SubExpressions[j], "=");
                        k++;
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
                    "op" => new LuaToken(match.Value switch {
                        "," => LuaTokenType.Comma,
                        "=" => LuaTokenType.Equals,
                        ";" => LuaTokenType.Semicolon,
                        "." => LuaTokenType.Dot,
                        "+" => LuaTokenType.StdOperator,
                        "-" => LuaTokenType.StdOperator,
                        "*" => LuaTokenType.StdOperator,
                        "/" => LuaTokenType.StdOperator,
                        _ => throw new Exception()
                    }, match.Value),
                    "id" => match.Value switch {
                        "and" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "or" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "if" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "for" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "while" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "do" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        _ => new LuaToken(LuaTokenType.Identifier, match.Value)
                    },
                    "nil" => new LuaToken(LuaTokenType.Nil, match.Value),
                    "str" => new LuaToken(LuaTokenType.String, match.Value.Trim('\"')),
                    _ => throw new Exception()
                });

            }

            return tokens;

        }

    }

}
