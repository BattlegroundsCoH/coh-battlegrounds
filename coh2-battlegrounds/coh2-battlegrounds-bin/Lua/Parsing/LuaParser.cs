using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Operators;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Lua.Parsing {

    // https://www.lua.org/manual/5.1/manual.html

    /// <summary>
    /// Parser class for parsing Lua source code.
    /// </summary>
    public static class LuaParser {
        // This file explodes really quickly

        /// <summary>
        /// All possible token types a lua source code element can be represented by.
        /// </summary>
        public enum LuaTokenType {
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
            Look,
            LookSelf,
            Comment,
            StdOperator,
            RelOperator,
            Concat,
            VarArgs,
            Keyword,
        }

        /// <summary>
        /// Parsing token for Lua src.
        /// </summary>
        private record LuaToken(LuaTokenType Type, string Val, LuaSourcePos Pos);

        /// <summary>
        /// Regex for Lua tokens.
        /// </summary>
        private static readonly Regex LuaRegex
            = new Regex(@"(?<c>--.*\n)|(?<n>\d*\.\d+)|(?<i>\d+)|(?<b>true|false)|(?<nil>nil)|(?<op>(=|,|\+|-|\*|/|;|\.|#|<|>|~|\^|:|%)+)|(?<e>\(|\))|(?<t>\{|\}|\[|\])|(?<id>(_|\w)(_|\d|\w)*)|(?<s>\"".*?\"")|(?<le>\n)");

        /// <summary>
        /// Operator presedence table.
        /// </summary>
        private static ILuaOperatorSyntax[][] luaOperatorPresedence = new ILuaOperatorSyntax[][] {
            new ILuaOperatorSyntax[] { new LuaCallOperatorSyntax() },
            new ILuaOperatorSyntax[] { new LuaIndexOperatorSyntax() },
            new ILuaOperatorSyntax[] { new LuaLookupOperatorSyntax("."), new LuaLookupOperatorSyntax(":") },
            new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("^") },
            new ILuaOperatorSyntax[] {
                new LuaUnaryOperatorSyntax("not", LuaUnaryPosition.Prefix), new LuaUnaryOperatorSyntax("#", LuaUnaryPosition.Prefix),
                new LuaUnaryOperatorSyntax("-", LuaUnaryPosition.Prefix)
            },
            new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("*"), new LuaBinaryOperatorSyntax("/"), new LuaBinaryOperatorSyntax("%") },
            new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("+"), new LuaBinaryOperatorSyntax("-") },
            new ILuaOperatorSyntax[] { new LuaBinaryOperatorSyntax("..") },
            new ILuaOperatorSyntax[] {
                new LuaBinaryOperatorSyntax("<"), new LuaBinaryOperatorSyntax("<="), new LuaBinaryOperatorSyntax(">"), new LuaBinaryOperatorSyntax(">="),
                new LuaBinaryOperatorSyntax("=="), new LuaBinaryOperatorSyntax("~=")
            },
            new ILuaOperatorSyntax[] { new LuaLogicOperatorSyntax("and") },
            new ILuaOperatorSyntax[] { new LuaLogicOperatorSyntax("or") },
            new ILuaOperatorSyntax[] { new LuaAssignOperatorSyntax() },
        };

        /// <summary>
        /// Parse Lua code without syntax error propogation and returns a complete chunk.
        /// </summary>
        /// <param name="result">The resulting and verified lua chunk.</param>
        /// <param name="sourceText">The Lua source code.</param>
        /// <param name="sourceFile">The filename of the Lua source code.</param>
        /// <returns>If any <see cref="LuaSyntaxError"/> occured; Otherwise <see langword="null"/>.</returns>
        public static LuaSyntaxError ParseLuaSourceToChunk(out LuaChunk result, string sourceText, string sourceFile = "?.lua") {
            if (ParseLuaSourceSafe(out List<LuaExpr> exprs, sourceText, sourceFile) is LuaSyntaxError lse) {
                result = null;
                return lse;
            } else {
                result = new LuaChunk(exprs, LuaSourcePos.Undefined);
                try {
                    VerifyChunk(result);
                    return null;
                } catch (LuaSyntaxError ve) {
                    return ve;
                }
            }
        }

        /// <summary>
        /// Parse Lua code without syntax error propogation.
        /// </summary>
        /// <param name="result">The resulting AST.</param>
        /// <param name="sourceText">The Lua source code.</param>
        /// <param name="sourceFile">The filename of the Lua source code.</param>
        /// <returns>If any <see cref="LuaSyntaxError"/> occured; Otherwise <see langword="null"/>.</returns>
        public static LuaSyntaxError ParseLuaSourceSafe(out List<LuaExpr> result, string sourceText, string sourceFile = "?.lua") {
            try {
                result = ParseLuaSource(sourceText, sourceFile); // Result is AST
                return null; // Error is null
            } catch (LuaSyntaxError lse) {
                result = null; // Result is null
                return lse; // Return syntax error
            } catch { // Propogate other errors
                throw;
            }
        }

        /// <summary>
        /// Parse Lua code into an AST.
        /// </summary>
        /// <param name="sourceText">The Lua source code.</param>
        /// <param name="sourceFile">The filename of the Lua source code.</param>
        /// <returns>The built AST.</returns>
        public static List<LuaExpr> ParseLuaSource(string sourceText, string sourceFile = "?.lua") {

            // Tokenize
            var tokens = Tokenize(sourceText, sourceFile);

            // Convert into epressions
            List<LuaExpr> expressions = new List<LuaExpr>();
            for (int i = 0; i < tokens.Count; i++) {
                expressions.Add(tokens[i].Type switch {
                    LuaTokenType.Bool => new LuaConstValueExpr(new LuaBool(bool.Parse(tokens[i].Val)), tokens[i].Pos),
                    LuaTokenType.Identifier => new LuaIdentifierExpr(tokens[i].Val, tokens[i].Pos),
                    LuaTokenType.Integer => new LuaConstValueExpr(new LuaNumber(int.Parse(tokens[i].Val)), tokens[i].Pos),
                    LuaTokenType.Nil => new LuaConstValueExpr(LuaNil.Nil, tokens[i].Pos),
                    LuaTokenType.String => new LuaConstValueExpr(new LuaString(tokens[i].Val), tokens[i].Pos),
                    LuaTokenType.Number => new LuaConstValueExpr(new LuaNumber(double.Parse(tokens[i].Val)), tokens[i].Pos),
                    LuaTokenType.Comma or LuaTokenType.Equals or LuaTokenType.IndexClose or
                    LuaTokenType.IndexOpen or LuaTokenType.Semicolon or LuaTokenType.TableClose or
                    LuaTokenType.TableOpen or LuaTokenType.Look or LuaTokenType.LookSelf or
                    LuaTokenType.ExprOpen or LuaTokenType.ExprClose => new LuaOpExpr(tokens[i].Type, tokens[i].Pos),
                    LuaTokenType.Comment => new LuaComment(tokens[i].Val, tokens[i].Pos),
                    LuaTokenType.StdOperator or LuaTokenType.RelOperator or LuaTokenType.Concat => new LuaOpExpr(tokens[i].Val, tokens[i].Pos),
                    LuaTokenType.Keyword => new LuaKeyword(tokens[i].Val, tokens[i].Pos),
                    _ => throw new Exception(),
                });
                if (expressions[^1] is LuaComment) {
                    expressions.RemoveAt(expressions.Count - 1);
                }
            }

            // Apply groups
            ApplyGroup(expressions, 0, LuaTokenType.ExprOpen, LuaTokenType.ExprClose, CreateExprGroup);
            ApplyGroup(expressions, 0, LuaTokenType.TableOpen, LuaTokenType.TableClose, x => new LuaTableExpr(x, LuaSourcePos.Undefined));
            ApplyGroup(expressions, 0, LuaTokenType.IndexOpen, LuaTokenType.IndexClose, CreateIndexer);

            // Apply OOP
            ApplyOrderOfOperations(expressions);

            // Apply scope groups
            _ = ApplyScopeGroups(expressions, 0, false);

            // Apply implicit behaviour (like implict indexing)
            ApplyImplicitBehaviour(expressions, 0);

            // Apply grammar
            ApplyGrammar(expressions, 0);

            // Return expressions
            return expressions;

        }

        /// <summary>
        /// Verify basic syntax of chunk
        /// </summary>
        /// <param name="chunk"></param>
        public static void VerifyChunk(LuaChunk chunk) {
            for (int j = 0; j < chunk.ScopeBody.Count; j++) {
                if (chunk.ScopeBody[j] is not ILuaStatementExpr) {
                    throw new LuaSyntaxError(StatementExpected(chunk.ScopeBody[j]), chunk.ScopeBody[j].SourcePos);
                }
            }
        }

        static LuaSyntaxError ForExpectNear(List<LuaExpr> luaExprs, int i) {
            bool isGeneric = luaExprs.Any(x => x is LuaKeyword { Keyword: "in" });
            if (isGeneric) {
                throw new NotImplementedException();
            } else {
                int commaCount = 0;
                int lastComma = -1;
                int j = i;
                while (j < luaExprs.Count) {
                    if (luaExprs[j] is LuaOpExpr { Type: LuaTokenType.Comma }) {
                        commaCount++;
                        lastComma = j;
                    }
                    j++;
                }
                if (commaCount == 2) {
                    return new LuaSyntaxError($"'do' expected near '{GetNodeAsString(luaExprs, lastComma + 2)}'", luaExprs[i].SourcePos);
                } else if (commaCount == 1) {
                    return new LuaSyntaxError($"'do' expected near ...", luaExprs[i].SourcePos);
                } else {
                    return new LuaSyntaxError("'do' expected near ','", luaExprs[i].SourcePos);
                }
            }
        }

        /// <summary>
        /// Applies scope rules (eg. function ... end --> LuaFuncExpr; if ... then ... end --> LuaIfStatement).
        /// </summary>
        static List<LuaExpr> ApplyScopeGroups(List<LuaExpr> luaExprs, int i, bool endSemicolon, bool allowElse = false, string endIsEnd = "end") {

            static List<LuaExpr> PickUntil(List<LuaExpr> exprs, int from, Predicate<LuaExpr> predicate, Predicate<LuaExpr> errorPredicate, Action<int> errHandler = null) {
                List<LuaExpr> elements = new List<LuaExpr>();
                while (from < exprs.Count) {
                    if (predicate(exprs[from])) {
                        return elements;
                    } else if (errorPredicate(exprs[from])) {
                        errHandler?.Invoke(elements.Count);
                        return null;
                    } else {
                        elements.Add(exprs[from]);
                        from++;
                    }
                }
                if (from > elements.Count) {
                    errHandler?.Invoke(elements.Count);
                    return null;
                }
                return elements;
            }

            static List<LuaExpr> CollectBody(List<LuaExpr> luaExprs, int i, bool endSemicolon, string statementName, bool els = false, string endname = "end") {
                var body = ApplyScopeGroups(luaExprs, i + 1, endSemicolon, els, endname);
                if (body.Count + 1 + i >= luaExprs.Count || (luaExprs[body.Count + 1 + i] is LuaKeyword kw && kw.Keyword != endname)) {
                    throw new LuaSyntaxError($"'{endname}' expected (to close '{statementName}' at line {luaExprs[i].SourcePos.Line}) near <eof>");
                }
                luaExprs.RemoveRange(i + 1, body.Count + 1);
                return body;
            }

            List<LuaExpr> result = new List<LuaExpr>();
            while (i < luaExprs.Count) {
                if ((luaExprs[i] is LuaKeyword { Keyword: "end" } && endIsEnd == "end") || (endSemicolon && luaExprs[i] is LuaOpExpr { Type: LuaTokenType.Semicolon })
                    || (luaExprs[i] is LuaKeyword kw && kw.Keyword == endIsEnd)) {
                    return result;
                } else if (luaExprs[i] is LuaKeyword { Keyword: "function" }) {
                    // Handle functions
                    if (i + 1 < luaExprs.Count) {
                        if (luaExprs[i + 1] is LuaArguments lfargs) {

                            // Get body
                            var body = ApplyScopeGroups(luaExprs, i + 2, false);

                            // Form func
                            luaExprs[i] = new LuaFuncExpr(lfargs, new LuaChunk(body, luaExprs[i].SourcePos), luaExprs[i].SourcePos);

                            // Remove body + args
                            luaExprs.RemoveRange(i + 1, body.Count + 2);

                            // Merge into assignment
                            if (i - 2 >= 0 && luaExprs[i - 1] is LuaOpExpr { Type: LuaTokenType.Equals }) {

                                luaExprs[i - 2] = new LuaAssignExpr(luaExprs[i - 2], luaExprs[i], false);
                                luaExprs.RemoveRange(i - 1, 2);

                                result.RemoveAt(result.Count - 1);
                                i -= 2;

                            }

                            result.Add(luaExprs[i]);

                        } else if (luaExprs[i + 1] is LuaCallExpr call) {

                            // Undo OOP
                            var id = call.ToCall;
                            var args = call.Arguments;

                            // Inject the 'self' parameter
                            if (call is LuaSelfCallExpr) {
                                args.Arguments.Insert(0, new LuaIdentifierExpr("self", LuaSourcePos.Undefined));
                                if (args.Arguments.Count > 1) {
                                    args.Arguments.Insert(1, new LuaOpExpr(LuaTokenType.Comma, LuaSourcePos.Undefined));
                                }
                            }

                            // Fit
                            var tmp = luaExprs[i];
                            luaExprs[i] = id;
                            luaExprs[i + 1] = new LuaOpExpr(LuaTokenType.Equals, luaExprs[i].SourcePos);
                            luaExprs.Insert(i + 2, tmp);
                            luaExprs.Insert(i + 3, args);
                            i--;

                        } else {
                            throw new LuaSyntaxError(); // TODO: Error message
                        }
                    } else {
                        throw new LuaSyntaxError("Identifier expected but found EOF");
                    }

                } else if (luaExprs[i] is LuaKeyword { Keyword: "return" }) {

                    // Collect values
                    var sub = ApplyScopeGroups(luaExprs, i + 1, true);
                    if (sub.Any(x => x is LuaStatement)) {
                        // throw error
                    }

                    // Verify return value
                    CheckCommas(sub, 0);
                    LuaExpr tuple = new LuaExpressionList(sub, luaExprs[i].SourcePos);
                    if ((tuple as LuaExpressionList).Values.Count == 1) {
                        tuple = (tuple as LuaExpressionList).Values[0];
                    }

                    // Set return value
                    luaExprs[i] = new LuaReturnStatement(tuple, luaExprs[i].SourcePos);
                    luaExprs.RemoveRange(i + 1, sub.Count);
                    result.Add(luaExprs[i]); // Add new statement to "outer" collection

                } else if (luaExprs[i] is LuaKeyword { Keyword: "break" }) {
                    luaExprs[i] = new LuaBreakStatement(luaExprs[i].SourcePos);
                    result.Add(luaExprs[i]);
                } else if (luaExprs[i] is LuaKeyword { Keyword: "do" }) {

                    // Collect body
                    var body = CollectBody(luaExprs, i, false, "do");

                    // Update expression list and add self to results
                    luaExprs[i] = new LuaDoStatement(new LuaChunk(body, luaExprs[i].SourcePos), luaExprs[i].SourcePos);
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "while" }) {

                    // Make sure we have a do
                    if (i + 2 >= luaExprs.Count || luaExprs[i + 2] is not LuaKeyword { Keyword: "do" }) {
                        throw new LuaSyntaxError($"'do' expected before '{GetNodeAsString(luaExprs, i + 2)}'");
                    }

                    // Get condition
                    var condition = luaExprs[i + 1];
                    if (condition is LuaExpressionList or LuaChunk) {
                        throw new LuaSyntaxError($"'do' expected near '{GetNodeAsString(luaExprs, i + 1)}'");
                    }
                    luaExprs.RemoveRange(i + 1, 2);

                    // Get body
                    var body = CollectBody(luaExprs, i, false, "while");

                    // Create while statement and add to outer collection
                    luaExprs[i] = new LuaWhileStatement(condition, new LuaChunk(body, luaExprs[i].SourcePos));
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "for" }) {

                    // Get statement
                    var condition = PickUntil(luaExprs, i + 1,
                        u => u is LuaKeyword { Keyword: "do" },
                        v => v is LuaKeyword and not LuaKeyword { Keyword: "in" },
                        q => throw ForExpectNear(luaExprs, i));
                    luaExprs.RemoveRange(i + 1, condition.Count + 1);

                    // Get body
                    var body = CollectBody(luaExprs, i, false, "for");

                    // Create while statement and add to outer collection
                    int stop = condition.FindIndex(x => x is LuaKeyword { Keyword: "in" });
                    if (stop != -1) { // Is "generic"

                        // Pick variable list
                        var _varargs = condition.Take(stop).ToList();
                        CheckCommas(_varargs, 0);
                        LuaVariableList varls = null;
                        if (!_varargs.All(x => x is LuaIdentifierExpr)) {
                            throw new LuaSyntaxError();
                        } else {
                            varls = new LuaVariableList(_varargs.Select(x => x as LuaIdentifierExpr).ToList(), luaExprs[i].SourcePos);
                        }

                        // Properly fetch iterators
                        condition = condition.Skip(stop + 1).ToList();
                        ApplyOrderOfOperations(condition);

                        // Set for statement
                        luaExprs[i] = new LuaGenericForStatement(varls, condition.Count == 1 ? condition[0] : throw new LuaSyntaxError(), new LuaChunk(body, luaExprs[i].SourcePos));

                    } else { // is "numeric"

                        // Select var
                        stop = condition.FindIndex(x => x is LuaOpExpr { Type: LuaTokenType.Comma });
                        var _var = condition.Take(stop).ToList();

                        ApplyOrderOfOperations(_var);
                        if (_var.Count != 1 && _var[0] is not LuaBinaryExpr) {
                            throw new LuaSyntaxError();
                        }

                        condition = condition.Skip(stop + 1).ToList();

                        // Select limit
                        stop = condition.FindIndex(x => x is LuaOpExpr { Type: LuaTokenType.Comma });
                        bool hasStep = stop != -1;
                        if (!hasStep) {
                            stop = condition.Count;
                        }

                        var _limit = new LuaChunk(condition.Take(stop).ToList(), luaExprs[i].SourcePos);
                        condition = condition.Skip(stop + 1).ToList();

                        // Select step
                        LuaExpr _step = hasStep ? new LuaChunk(condition, luaExprs[i].SourcePos) : new LuaNopExpr(luaExprs[i].SourcePos);

                        // Set for statement
                        luaExprs[i] = new LuaNumericForStatement((_var[0] as LuaAssignExpr) with { Local = true }, _limit, _step, new LuaChunk(body, luaExprs[i].SourcePos));

                    }

                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "repeat" }) {

                    // Collect body
                    var body = CollectBody(luaExprs, i, false, "repeat", false, "until");

                    // Create statement (without condition) and add self
                    luaExprs[i] = new LuaRepeatStatement(new LuaChunk(body, luaExprs[i].SourcePos), null, luaExprs[i].SourcePos);
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "if" } or LuaKeyword { Keyword: "elseif" }) {

                    // Determine if it's a "if" or "elseif"
                    bool isIfCondition = luaExprs[i] is LuaKeyword { Keyword: "if" };
                    if (!isIfCondition && !allowElse) {
                        throw new LuaSyntaxError("Unexpected elseif");
                    }

                    // Get condition
                    var condition = PickUntil(luaExprs, i + 1,
                        u => u is LuaKeyword { Keyword: "then" },
                        v => v is LuaKeyword,
                        _ => throw new LuaSyntaxError($"'then' expected before '{GetNodeAsString(luaExprs, i + 2)}'", luaExprs[i + 1].SourcePos)); // TODO: Simplify this, only one expr is needed
                    luaExprs.RemoveRange(i + 1, condition.Count + 1);

                    // Collect body
                    var body = ApplyScopeGroups(luaExprs, i + 1, false, true);
                    bool hasFollow = body[^1] is LuaBranchFollow;
                    bool hasEnd = !hasFollow && (i + body.Count + 1 < luaExprs.Count && luaExprs[i + body.Count + 1] is LuaKeyword { Keyword: "end" });

                    // Make sure we've got a valid ending
                    if (!hasEnd && !hasFollow) {
                        throw new LuaSyntaxError($"'end' expected (to close '{GetNodeAsString(luaExprs, i)}' at line {luaExprs[i].SourcePos.Line}) near <eof>", luaExprs[i].SourcePos);
                    }

                    // Remove body (+follow)
                    luaExprs.RemoveRange(i + 1, body.Count + (hasFollow ? 0 : 1));

                    // Determine if there's a follow-up branch
                    LuaBranchFollow follow = new LuaEndBranch(luaExprs[i].SourcePos);
                    if (hasFollow) {
                        follow = body[^1] as LuaBranchFollow;
                        body.RemoveAt(body.Count - 1);
                    }

                    // Create statement
                    var bodyChunk = new LuaChunk(body, luaExprs[i].SourcePos);
                    luaExprs[i] = isIfCondition ? new LuaIfStatement(condition[0], bodyChunk, follow, luaExprs[i].SourcePos) : new LuaIfElseStatement(condition[0], bodyChunk, follow, luaExprs[i].SourcePos);
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "else" }) {

                    // Make sure we allow "else"
                    if (!allowElse) {
                        throw new LuaSyntaxError("Unexpected else");
                    }

                    // Get body
                    var body = CollectBody(luaExprs, i, false, "else");

                    // Set statement and add slef
                    luaExprs[i] = new LuaElseStatement(new LuaChunk(body, luaExprs[i].SourcePos), luaExprs[i].SourcePos);
                    result.Add(luaExprs[i]);

                    // Result result
                    return result;

                } else {
                    result.Add(luaExprs[i]);
                }
                i++;
            }
            return result;
        }

        /// <summary>
        /// Applies the order of operations (operator presedence) on a sequence of expressions. OOP is defined in <see cref="luaOperatorPresedence"/>.
        /// </summary>
        static void ApplyOrderOfOperations(List<LuaExpr> luaExprs) {

            // Run through all operators
            for (int i = 0; i < luaOperatorPresedence.Length; i++) {

                // Get current available operators to consider
                var ops = luaOperatorPresedence[i];

                int j = 0;
                while (j < luaExprs.Count) {
                    if (ops.FirstOrDefault(x => x.IsOperator(luaExprs[j])) is ILuaOperatorSyntax opsyntax) {
                        bool pre = j - 1 >= 0;
                        bool post = j + 1 < luaExprs.Count;
                        if (opsyntax.PrePostCondtion(pre, post)) {
                            if (opsyntax.Apply(luaExprs, j, ApplyOrderOfOperations)) {
                                continue;
                            }
                        }
                    }
                    j++;
                }

            }

            // Run recursively
            for (int i = 0; i < luaExprs.Count; i++) {
                if (luaExprs[i] is LuaBinaryExpr binop) {
                    if (binop.Left is LuaSingleElementParenthesisGroup lpg) {
                        ApplyOrderOfOperations(lpg.Arguments);
                        if (lpg.Arguments.Count == 1) {
                            luaExprs[i] = binop with { Left = lpg.Arguments[0] };
                        }
                    }
                    if (binop.Right is LuaTableExpr table) {
                        ApplyOrderOfOperations(table.SubExpressions);
                    } else if (binop.Right is LuaSingleElementParenthesisGroup rpg) {
                        ApplyOrderOfOperations(rpg.Arguments);
                        if (rpg.Arguments.Count == 1) {
                            luaExprs[i] = binop with { Right = rpg.Arguments[0] };
                        }
                    }
                } else if (luaExprs[i] is LuaTableExpr tableNode) {
                    ApplyOrderOfOperations(tableNode.SubExpressions);
                } else if (luaExprs[i] is LuaChunk scopeNode) {
                    ApplyOrderOfOperations(scopeNode.ScopeBody);
                    if (scopeNode.ScopeBody.Count == 1) {
                        luaExprs[i] = scopeNode.ScopeBody[0];
                    }
                } else if (luaExprs[i] is LuaFuncExpr funcExpr) {
                    ApplyOrderOfOperations(funcExpr.Arguments.Arguments);
                    ApplyOrderOfOperations(funcExpr.Body.ScopeBody);
                } else if (luaExprs[i] is LuaReturnStatement returnStatement) {
                    if (returnStatement.Value is LuaExpressionList tupleExpr) {
                        ApplyOrderOfOperations(tupleExpr.Values);
                        CheckCommas(tupleExpr.Values, 0);
                    } else {
                        throw new NotImplementedException();
                    }
                } else if (luaExprs[i] is LuaWhileStatement whileStatement) {
                    whileStatement = RecursiveOOP(whileStatement, whileStatement.Condition, (self, res) => self with { Condition = res });
                    ApplyOrderOfOperations((whileStatement.Body).ScopeBody);
                    luaExprs[i] = whileStatement;
                } else if (luaExprs[i] is LuaNumericForStatement numForStatement) {
                    numForStatement = RecursiveOOP(numForStatement, numForStatement.Limit, (self, res) => self with { Limit = res });
                    numForStatement = RecursiveOOP(numForStatement, numForStatement.Step, (self, res) => self with { Step = res });
                    ApplyOrderOfOperations((numForStatement.Body).ScopeBody);
                    luaExprs[i] = numForStatement;
                } else if (luaExprs[i] is LuaGenericForStatement genForStatement) {
                    ApplyOrderOfOperations(genForStatement.Body.ScopeBody);
                } else if (luaExprs[i] is LuaDoStatement doStatement) {
                    ApplyOrderOfOperations(doStatement.Body.ScopeBody);
                } else if (luaExprs[i] is LuaRepeatStatement repeatStatement) {
                    ApplyOrderOfOperations(repeatStatement.Body.ScopeBody);
                } else if (luaExprs[i] is LuaBranch branch) {
                    luaExprs[i] = RecrusiveOOPBranching(branch);
                } else if (luaExprs[i] is LuaSingleElementParenthesisGroup single) {
                    ApplyOrderOfOperations(single.Arguments);
                    if (single.Arguments.Count == 1) {
                        if (single.Arguments[0] is LuaCallExpr callPop) {
                            luaExprs[i] = callPop with { ReturnCount = 1 };
                        } else {
                            luaExprs[i] = single.Arguments[0];
                        }
                    }
                }
            }

        }

        static LuaBranch RecrusiveOOPBranching(LuaBranch branch) {
            ApplyOrderOfOperations(branch.Body.ScopeBody);
            if (branch is LuaIfStatement _if) {
                _if = RecursiveOOP(_if, _if.Condition, (self, res) => self with { Condition = res });
                RecrusiveOOPBranching(_if.BranchFollow);
                return _if;
            } else if (branch is LuaIfElseStatement _eif) {
                _eif = RecursiveOOP(_eif, _eif.Condition, (self, res) => self with { Condition = res });
                RecrusiveOOPBranching(_eif.BranchFollow);
                return _eif;
            }
            return branch;
        }

        static T RecursiveOOP<T>(T self, LuaExpr expr, Func<T, LuaExpr, T> mutator) where T : LuaExpr {
            if (expr is LuaChunk chunk) {
                ApplyOrderOfOperations(chunk.ScopeBody);
                if (chunk.ScopeBody.Count == 1) {
                    return mutator(self, chunk.ScopeBody[0]);
                }
            }
            return self;
        }

        static void CheckCommas(List<LuaExpr> exprs, int i) {
            while (i < exprs.Count) {
                CheckComma(exprs, i);
                i++;
            }
        }

        static void CheckComma(List<LuaExpr> exprs, int i) {
            if (i + 1 < exprs.Count && exprs[i + 1] is not LuaOpExpr { Type: LuaTokenType.Comma }) {
                throw new LuaSyntaxError("',' expected.", $"Insert ',' following '{exprs[i]}'");
            } else {
                if (i + 1 < exprs.Count) {
                    exprs.RemoveAt(i + 1);
                }
            }
        }

        static void FlattenTuple(LuaExpressionList tupleExpr) {
            if (tupleExpr.Values.Count == 0) {
                return;
            }
            if (tupleExpr.Values[0] is LuaExpressionList expr) {
                FlattenTuple(expr);
                var merged = expr.Values;
                merged.AddRange(tupleExpr.Values.Skip(1));
                tupleExpr.Values.Clear();
                tupleExpr.Values.AddRange(merged);
            } else if (tupleExpr.Values[^1] is LuaExpressionList exprRight) {
                FlattenTuple(exprRight);
                var merged = tupleExpr.Values.Take(tupleExpr.Values.Count - 1).ToList();
                merged.AddRange(exprRight.Values);
                tupleExpr.Values.Clear();
                tupleExpr.Values.AddRange(merged);
            }
        }

        /// <summary>
        /// Applies grammatical rules on a sequence of expressions.
        /// </summary>
        static void ApplyGrammar(List<LuaExpr> luaExprs, int i) {

            // Apply binary operation
            static void ApplyBinop(LuaBinaryExpr binop) {

                // LHS
                if (binop.Left is LuaTableExpr lt) {
                    ApplyTable(lt);
                } else if (binop.Left is LuaCallExpr lc) {
                    ApplyArgs(lc.Arguments);
                }

                // RHS
                if (binop.Right is LuaTableExpr lrt) {
                    ApplyTable(lrt);
                } else if (binop.Right is LuaFuncExpr lrf) {
                    ApplyArgs(lrf.Arguments);
                    ApplyGrammar(lrf.Body.ScopeBody, 0);
                } else if (binop.Right is LuaCallExpr rc) {
                    ApplyArgs(rc.Arguments);
                }

            }

            // Apply table
            static void ApplyTable(LuaTableExpr table) {
                int j = 0;
                while (j < table.SubExpressions.Count) {
                    if (table.SubExpressions[j] is LuaTableExpr sub) {
                        ApplyGrammar(sub.SubExpressions, 0);
                    } else if (table.SubExpressions[j] is LuaBinaryExpr tablebin) {
                        ApplyBinop(tablebin);
                    }
                    CheckComma(table.SubExpressions, j);
                    j++;
                }
            }

            // Apply arguments
            static void ApplyArgs(LuaArguments args) {
                CheckCommas(args.Arguments, 0);
            }

            // Handle multiple value assignment
            static void ApplyMultivalueAssignment(List<LuaExpr> luaExprs, ref int i) {
                if (i + 1 < luaExprs.Count && luaExprs[i + 1] is LuaOpExpr { Type: LuaTokenType.Comma }) {
                    if (i + 2 < luaExprs.Count && luaExprs[i + 2] is LuaAssignExpr assign) {
                        var lhs = new LuaExpressionList(new List<LuaExpr>() { luaExprs[i], assign.Left }, luaExprs[i].SourcePos);
                        FlattenTuple(lhs);
                        luaExprs[i] = assign with { Left = lhs };
                        luaExprs.RemoveRange(i + 1, 2);
                        ApplyBinop(luaExprs[i] as LuaAssignExpr);
                    } else if (i + 2 < luaExprs.Count && luaExprs[i + 2] is LuaIdentifierExpr or LuaLookupExpr) {
                        luaExprs[i] = new LuaExpressionList(new List<LuaExpr>() { luaExprs[i], luaExprs[i + 2] }, luaExprs[i].SourcePos);
                        luaExprs.RemoveRange(i + 1, 2);
                        i--;
                    } else {
                        // ERR
                    }
                } else if (i - 2 >= 0 && luaExprs[i-1] is LuaOpExpr { Type: LuaTokenType.Comma } && luaExprs[i-2] is LuaAssignExpr assign) {
                    var rhs = new LuaExpressionList(new List<LuaExpr>() { assign.Right, luaExprs[i] }, luaExprs[i - 2].SourcePos);
                    FlattenTuple(rhs);
                    luaExprs[i - 2] = assign with { Right = rhs };
                    luaExprs.RemoveRange(i - 1, 2);
                    i -= 2;
                }
            }

            // Apply local modifier
            void ApplyLocal() {
                if (luaExprs[i + 1] is LuaAssignExpr laexp) {
                    luaExprs[i + 1] = laexp with { Local = true };
                    luaExprs.RemoveAt(i);
                } else {
                    // throw error
                }
                i--;
            }

            // Apply for-statement corrections
            void ApplyForStatement(LuaExpr expr) {
                if (expr is LuaGenericForStatement generic) {
                    Single(generic.Body);
                } else if (expr is LuaNumericForStatement numeric) {
                    Single(numeric.Body);
                    if (numeric.Limit is LuaBinaryExpr lim) {
                        ApplyBinop(lim);
                    }
                    if (numeric.Step is LuaBinaryExpr step) {
                        ApplyBinop(step);
                    }
                }
            }

            // Apply grammar on branching
            void ApplyBranch(LuaBranch branch) {
                Single(branch.Body);
                if (branch is LuaIfStatement ifbranch) {
                    ApplySingleGrammar(ifbranch.Condition);
                    ApplyBranch(ifbranch.BranchFollow);
                } else if (branch is LuaIfElseStatement ifElseBranch) {
                    ApplySingleGrammar(ifElseBranch.Condition);
                    ApplyBranch(ifElseBranch.BranchFollow);
                }
            }

            // Apply repeat statement
            void ApplyRepeat(List<LuaExpr> expr, int i, LuaRepeatStatement repeatStatement) {
                Single(repeatStatement.Body);
                if (i + 1 < expr.Count && expr[i + 1] is not LuaStatement) {
                    Single(expr[i + 1]);
                    expr[i] = repeatStatement with { Condition = expr[i + 1] };
                    expr.RemoveAt(i + 1);
                } else {
                    throw new LuaSyntaxError("");
                }
            }

            void Single(LuaExpr expr) {
                if (expr is LuaTableExpr table) {
                    ApplyTable(table);
                } else if (expr is LuaBinaryExpr binop) {
                    ApplyBinop(binop);
                } else if (expr is LuaCallExpr call) {
                    ApplyArgs(call.Arguments);
                } else if (expr is LuaFuncExpr func) {
                    ApplyArgs(func.Arguments);
                    Single(func.Body);
                } else if (expr is LuaIdentifierExpr or LuaLookupExpr or LuaExpressionList) {
                    ApplyMultivalueAssignment(luaExprs, ref i);
                } else if (expr is LuaKeyword { Keyword: "local" }) {
                    ApplyLocal();
                } else if (expr is LuaGenericForStatement or LuaNumericForStatement) {
                    ApplyForStatement(luaExprs[i]);
                } else if (expr is LuaDoStatement doStatement) {
                    Single(doStatement.Body);
                } else if (expr is LuaRepeatStatement repeat) {
                    ApplyRepeat(luaExprs, i, repeat);
                } else if (expr is LuaBranch branch) {
                    ApplyBranch(branch);
                } else if (expr is LuaReturnStatement returnStatement) {
                    Single(returnStatement.Value);
                } else if (expr is LuaChunk chunk) {
                    ApplyGrammar(chunk.ScopeBody, 0);
                    VerifyChunk(chunk);
                } else if (expr is LuaOpExpr { Type: LuaTokenType.Semicolon }) {
                    luaExprs.RemoveAt(i); // Can just remove
                    i--;
                }
            }

            // Run through all expressions and apply appropriate late-syntax
            while (i < luaExprs.Count) {
                Single(luaExprs[i]);
                i++;
            }

        }

        /// <summary>
        /// Apply the <see cref="ApplyGrammar(List{LuaExpr}, int)"/> function on a single element.
        /// </summary>
        static void ApplySingleGrammar(LuaExpr exprSrc) {
            if (exprSrc is LuaChunk chunk) {
                ApplyGrammar(chunk.ScopeBody, 0);
            } else {
                ApplyGrammar(new List<LuaExpr> { exprSrc }, 0);
            }
        }

        /// <summary>
        /// Applies implicit behaviour, like implicit indexing of table elements.
        /// </summary>
        static void ApplyImplicitBehaviour(List<LuaExpr> luaExprs, int i) {

            static void FixIndex(LuaTableExpr luaTable) {
                int j = 0; // using j so we dont confuse with i from outer scope
                int k = 1; // Lua indexing starts from 1
                while (j < luaTable.SubExpressions.Count) {
                    if (luaTable.SubExpressions[j] is LuaConstValueExpr or LuaTableExpr or LuaUnaryExpr or LuaCallExpr) {
                        luaTable.SubExpressions[j] = new LuaAssignExpr(new LuaIndexExpr(new LuaConstValueExpr(new LuaNumber(k), LuaSourcePos.Undefined)), luaTable.SubExpressions[j], false);
                        k++;
                    } else if (luaTable.SubExpressions[j] is LuaBinaryExpr binaryExpr) {
                        if (binaryExpr.Operator != "=") {
                            luaTable.SubExpressions[j] = new LuaAssignExpr(new LuaIndexExpr(new LuaConstValueExpr(new LuaNumber(k), LuaSourcePos.Undefined)), luaTable.SubExpressions[j], false);
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

            static void Single(LuaExpr expr) {
                if (expr is LuaTableExpr t) {
                    TableImplicits(t);
                } else if (expr is LuaBinaryExpr binop) {
                    if (binop.Left is LuaTableExpr tl) {
                        TableImplicits(tl);
                    }
                    if (binop.Right is LuaTableExpr tr) {
                        TableImplicits(tr);
                    }
                    // TODO: Loop through other elements
                } else if (expr is LuaReturnStatement returnStatement) {
                    Single(returnStatement.Value);
                } else if (expr is LuaExpressionList tupleExpr) {
                    ApplyImplicitBehaviour(tupleExpr.Values, 0);
                }
            }

            while (i < luaExprs.Count) {
                Single(luaExprs[i]);
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
                return new LuaEmptyParenthesisGroup(LuaSourcePos.Undefined);
            } else {
                if (exprs.Any(x => x is LuaOpExpr { Type: LuaTokenType.Comma })) {
                    return new LuaArguments(exprs, exprs[0].SourcePos);
                } else {
                    return new LuaSingleElementParenthesisGroup(exprs, exprs[0].SourcePos);
                }
            }
        }

        /// <summary>
        /// Applies groupings on a sequence of expressions. (So for example, () --> LuaSingleElementParenthesisGroup).
        /// </summary>
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

                } else if (expressions[i] is LuaArguments args) {
                    ApplyGroup(args.Arguments, 0, open, close, creator);
                } else if (expressions[i] is LuaTableExpr table) {
                    int j = 0;
                    ApplyGroup(table.SubExpressions, j, open, close, creator);
                }

                i++;

            }

        }

        private static string GetNodeAsString(List<LuaExpr> e, int i) => i < e.Count ? GetNodeAsString(e[i]) : "<eof>";

        private static string GetNodeAsString(LuaExpr e) => e switch {
            LuaIdentifierExpr id => id.Identifier,
            LuaKeyword kw => kw.Keyword,
            LuaConstValueExpr c => c.Value.Str(),
            LuaCallExpr call => GetNodeAsString(call.ToCall),
            _ => "<expression>"
        };

        private static string GetTypeName(LuaExpr e) => e switch {
            ILuaStatementExpr => "<statement>",
            LuaConstValueExpr c => c.Value.GetLuaType().LuaTypeName,
            _ => "<expression>"
        };

        private static string StatementExpected(LuaExpr e) => e switch {
            LuaBinaryExpr binop when binop.Left is not LuaBinaryExpr => $"unexpected {GetTypeName(binop.Left)} '{GetNodeAsString(binop.Left)}' near '{binop.Operator}'", 
            LuaBinaryExpr binop => StatementExpected(binop.Left),
            _ => "<statement> expected"
        };

        /// <summary>
        /// Convert the whole source code into a sequence of <see cref="LuaToken"/> instances.
        /// </summary>
        private static List<LuaToken> Tokenize(string source, string sourcefile) {

            List<LuaToken> tokens = new List<LuaToken>();

            int ln = 1;

            var matches = LuaRegex.Matches(source);
            foreach (RegexMatch match in matches) {

                string type = "0";
                foreach (Group g in match.Groups) {
                    if (!string.IsNullOrEmpty(g.Value) && g.Name.CompareTo("0") != 0) {
                        type = g.Name;
                    }
                }

                if (type == "le") {
                    ln++;
                } else if (type == "c") {
                    ln += match.Value.Count(x => x == '\n');
                } else {

                    var src = new LuaSourcePos(sourcefile, ln);

                    tokens.Add(type switch {
                        "n" => new LuaToken(LuaTokenType.Number, match.Value, src),
                        "i" => new LuaToken(LuaTokenType.Integer, match.Value, src),
                        "b" => new LuaToken(LuaTokenType.Bool, match.Value, src),
                        "t" => new LuaToken(match.Value switch {
                            "[" => LuaTokenType.IndexOpen,
                            "]" => LuaTokenType.IndexClose,
                            "{" => LuaTokenType.TableOpen,
                            "}" => LuaTokenType.TableClose,
                            _ => throw new Exception()
                        }, match.Value, src),
                        "e" => new LuaToken(match.Value switch {
                            "(" => LuaTokenType.ExprOpen,
                            ")" => LuaTokenType.ExprClose,
                            _ => throw new Exception()
                        }, match.Value, src),
                        "s" => new LuaToken(LuaTokenType.String, match.Value.Trim('\"'), src),
                        "op" => new LuaToken(match.Value switch {
                            "," => LuaTokenType.Comma,
                            "=" => LuaTokenType.Equals,
                            ";" => LuaTokenType.Semicolon,
                            ".." => LuaTokenType.Concat,
                            "..." => LuaTokenType.VarArgs,
                            "." => LuaTokenType.Look,
                            ":" => LuaTokenType.LookSelf,
                            "+" or "-" or "*" or "/" or "#" or "%" or "^" => LuaTokenType.StdOperator,
                            "==" or "~=" or "<=" or ">=" or "<" or ">" => LuaTokenType.RelOperator,
                            "\"" => LuaTokenType.Quote,
                            _ => throw new Exception()
                        }, match.Value, src),
                        "id" => match.Value switch {
                            "and" or "not" or "or" => new LuaToken(LuaTokenType.StdOperator, match.Value, src),
                            "if" or "else" or "elseif" or "then" or "in" or "for" or "while" or "repeat" or "until"
                            or "do" or "function" or "end" or "break" or "return" or "local"
                            => new LuaToken(LuaTokenType.Keyword, match.Value, src),
                            _ => new LuaToken(LuaTokenType.Identifier, match.Value, src)
                        },
                        "nil" => new LuaToken(LuaTokenType.Nil, match.Value, src),
                        _ => throw new Exception(),
                    });

                }

            }

            // Return tokens
            return tokens;

        }

    }

}
