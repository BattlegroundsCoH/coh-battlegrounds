using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Battlegrounds.Lua.Debugging;
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
            Dot
        }

        private record LuaToken(LuaTokenType Type, string Val);

        private static readonly Regex LuaRegex 
            = new Regex(@"(?<n>\d*\.\d+)|(?<i>\d+)|(?<b>true|false)|(?<nil>nil)|(?<op>=|,|\+|-|;|\.)|(?<t>\{|\}|\[|\])|(?<id>\w(\d|\w)*)|(?<str>\""(\s|\S)*\"")");

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
                    _ => throw new Exception(),
                });
            }
            
            // Apply groups
            ApplyGroup(expressions, 0, LuaTokenType.TableOpen, LuaTokenType.TableClose, x => new LuaTableExpr(x));
            ApplyGroup(expressions, 0, LuaTokenType.IndexOpen, LuaTokenType.IndexClose, CreateIndexer);

            // Define grammar method
            static void ApplyGrammar(List<LuaExpr> luaExprs, int i) {
                while (i < luaExprs.Count) {
                    bool hasNext = i + 1 < luaExprs.Count;
                    bool hasPrev = i - 1 >= 0;
                    if (hasNext && hasPrev && luaExprs[i] is LuaOpExpr op && (LuaTokenType)op.Type == LuaTokenType.Equals) {
                        if (luaExprs[i + 1] is LuaTableExpr tableExpr) {
                            ApplyGrammar(tableExpr.SubExpressions, 0);
                        }
                        luaExprs[i - 1] = new LuaBinaryExpr(luaExprs[i - 1], luaExprs[i + 1], "=");
                        luaExprs.RemoveRange(i, 2);
                    } else if (hasNext && hasPrev && luaExprs[i] is LuaOpExpr dotOp && (LuaTokenType)dotOp.Type == LuaTokenType.Dot) {
                        if (luaExprs[i + 1] is LuaTableExpr tableExpr) {
                            ApplyGrammar(tableExpr.SubExpressions, 0);
                        }
                        luaExprs[i - 1] = new LuaLookupExpr(luaExprs[i - 1], luaExprs[i + 1] as LuaLookupIdExpr);
                        luaExprs.RemoveRange(i, 2);
                    } else if (hasPrev && luaExprs[i] is LuaIndexExpr ixp) {
                        luaExprs[i - 1] = new LuaLookupExpr(luaExprs[i - 1], luaExprs[i] as LuaLookupIdExpr);
                        luaExprs.RemoveAt(i);

                    } else {
                        i++;
                    }
                }
            }

            // Apply grammar
            ApplyGrammar(expressions, 0);

            // Return expressions
            return expressions;

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
                            if (!(expressions[j] is LuaOpExpr syntaxComma && syntaxComma.Type is LuaTokenType commaType && commaType == LuaTokenType.Comma)) {
                                sub.Add(expressions[j]);
                            }
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
                        _ => throw new Exception()
                    }, match.Value),
                    "id" => new LuaToken(LuaTokenType.Identifier, match.Value),
                    "nil" => new LuaToken(LuaTokenType.Nil, match.Value),
                    "str" => new LuaToken(LuaTokenType.String, match.Value.Trim('\"')),
                    _ => throw new Exception()
                });

            }

            return tokens;

        }

    }

}
