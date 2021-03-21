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
        private record LuaToken(LuaTokenType Type, string Val);

        /// <summary>
        /// Regex for Lua tokens.
        /// </summary>
        private static readonly Regex LuaRegex 
            = new Regex(@"(?<c>--.*\n)|(?<n>\d*\.\d+)|(?<i>\d+)|(?<b>true|false)|(?<nil>nil)|(?<op>(=|,|\+|-|\*|/|;|\.|#|<|>|~|\^|:|%)+)|(?<e>\(|\))|(?<t>\{|\}|\[|\])|(?<id>(_|\w)(_|\d|\w)*)|(?<s>\"".*?\"")");

        /// <summary>
        /// Operator presedence table.
        /// </summary>
        private static ILuaOperatorSyntax[][] luaOperatorPresedence = new ILuaOperatorSyntax[][] {
                new ILuaOperatorSyntax[] { new LuaCallOperatorSyntax() },
                new ILuaOperatorSyntax[] { new LuaIndexOperatorSyntax() },
                new ILuaOperatorSyntax[] { new LuaLookupOperatorSyntax(":"), new LuaLookupOperatorSyntax(".") },
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
                    new LuaBinaryOperatorSyntax("=="), new LuaBinaryOperatorSyntax("!=")
                },
                new ILuaOperatorSyntax[] { new LuaLogicOperatorSyntax("and") },
                new ILuaOperatorSyntax[] { new LuaLogicOperatorSyntax("or") },
                new ILuaOperatorSyntax[] { new LuaAssignOperatorSyntax() },
            };


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
                    LuaTokenType.TableOpen or LuaTokenType.Look or LuaTokenType.ExprOpen or
                    LuaTokenType.ExprClose => new LuaOpExpr(tokens[i].Type),
                    LuaTokenType.Comment => new LuaComment(tokens[i].Val),
                    LuaTokenType.StdOperator or LuaTokenType.RelOperator or LuaTokenType.Concat => new LuaOpExpr(tokens[i].Val),
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
            _ = ApplyScopeGroups(expressions, 0, false);

            // Apply OOP
            ApplyOrderOfOperations(expressions);

            // Apply implicit behaviour (like implict indexing)
            ApplyImplicitBehaviour(expressions, 0);

            // Apply grammar
            ApplyGrammar(expressions, 0);

            // Return expressions
            return expressions;

        }

        static List<LuaExpr> ApplyScopeGroups(List<LuaExpr> luaExprs, int i, bool endSemicolon, bool allowElseif = false) {

            static List<LuaExpr> PickUntil(List<LuaExpr> exprs, int from, Predicate<LuaExpr> predicate, Predicate<LuaExpr> errorPredicate) {
                List<LuaExpr> elements = new List<LuaExpr>();
                while (from < exprs.Count) {
                    if (predicate(exprs[from])) {
                        break;
                    } else if (errorPredicate(exprs[from])) {
                        throw new LuaSyntaxError();
                    } else {
                        elements.Add(exprs[from]);
                        from++;
                    }
                }
                return elements;
            }

            List<LuaExpr> result = new List<LuaExpr>();
            while (i < luaExprs.Count) {
                if (luaExprs[i] is LuaKeyword { Keyword: "end" } || (endSemicolon && luaExprs[i] is LuaOpExpr { Type: LuaTokenType.Semicolon })) {
                    return result;
                } else if (luaExprs[i] is LuaKeyword { Keyword: "function" }) {
                    // Handle functions
                    if (i + 1 < luaExprs.Count) {
                        if (luaExprs[i + 1] is LuaArguments lfargs) {

                            // Get body
                            var body = ApplyScopeGroups(luaExprs, i + 2, false);

                            // Form func
                            luaExprs[i] = new LuaFuncExpr(lfargs, new LuaChunk(body));

                            // Remove body + args
                            luaExprs.RemoveRange(i + 1, body.Count + 2);

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

                } else if (luaExprs[i] is LuaKeyword { Keyword: "return" }) {

                    var sub = ApplyScopeGroups(luaExprs, i + 1, true);
                    if (sub.Any(x => x is LuaStatement)) {
                        // throw error
                    }

                    luaExprs[i] = new LuaReturnStatement(new LuaTupleExpr(sub));
                    luaExprs.RemoveRange(i + 1, sub.Count);
                    result.Add(luaExprs[i]); // Add new statement to "outer" collection

                } else if (luaExprs[i] is LuaKeyword { Keyword: "break" }) {
                    luaExprs[i] = new LuaBreakStatement();
                    result.Add(luaExprs[i]);
                } else if (luaExprs[i] is LuaKeyword { Keyword: "do" }) {

                    // Collect body
                    var body = ApplyScopeGroups(luaExprs, i + 1, false);

                    // Update expression list
                    luaExprs[i] = new LuaDoStatement(new LuaChunk(body));
                    luaExprs.RemoveRange(i + 1, body.Count + 1);

                    // Add self to results
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "while" }) {

                    // Get condition
                    var condition = PickUntil(luaExprs, i+1, u => u is LuaKeyword { Keyword: "do" }, v => v is LuaKeyword);
                    luaExprs.RemoveRange(i + 1, condition.Count + 1);

                    // Get body
                    var body = ApplyScopeGroups(luaExprs, i + 1, true);
                    luaExprs.RemoveRange(i + 1, body.Count + 1);

                    // Create while statement and add to outer collection
                    luaExprs[i] = new LuaWhileStatement(new LuaChunk(condition), new LuaChunk(body));
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "for" }) {

                    // Get statement
                    var condition = PickUntil(luaExprs, i + 1, u => u is LuaKeyword { Keyword: "do" }, v => v is LuaKeyword and not LuaKeyword { Keyword: "in" });
                    luaExprs.RemoveRange(i + 1, condition.Count + 1);

                    // Get body
                    var body = ApplyScopeGroups(luaExprs, i + 1, true);
                    luaExprs.RemoveRange(i + 1, body.Count + 1);

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
                            varls = new LuaVariableList(_varargs.Select(x => x as LuaIdentifierExpr).ToList());
                        }

                        // Properly fetch iterators
                        condition = condition.Skip(stop + 1).ToList();
                        ApplyOrderOfOperations(condition);

                        // Set for statement
                        luaExprs[i] = new LuaGenericForStatement(varls, condition.Count == 1 ? condition[0] : throw new LuaSyntaxError(), new LuaChunk(body));

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

                        var _limit = new LuaChunk(condition.Take(stop).ToList());
                        condition = condition.Skip(stop + 1).ToList();

                        // Select step
                        LuaExpr _step = hasStep ? new LuaChunk(condition) : new LuaNopExpr();

                        // Set for statement
                        luaExprs[i] = new LuaNumericForStatement((_var[0] as LuaAssignExpr) with { Local = true }, _limit, _step, new LuaChunk(body));

                    }

                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "repeat" }) {

                } else if (luaExprs[i] is LuaKeyword { Keyword: "if" } or LuaKeyword { Keyword: "elseif" }) {

                    // Determine if it's a "if" or "elseif"
                    bool isIfCondition = luaExprs[i] is LuaKeyword { Keyword: "if" };
                    if (!isIfCondition && !allowElseif) {
                        throw new LuaSyntaxError("Unexpected elseif");
                    }

                    // Get condition
                    var condition = PickUntil(luaExprs, i + 1, u => u is LuaKeyword { Keyword: "then" }, v => v is LuaKeyword);
                    luaExprs.RemoveRange(i + 1, condition.Count + 1);

                    // Collect body
                    var body = ApplyScopeGroups(luaExprs, i + 1, false, true);
                    bool hasFollow = body[^1] is LuaBranchFollow;
                    luaExprs.RemoveRange(i + 1, body.Count + (hasFollow ? 0 : 1));

                    // Determine if there's a follow-up branch
                    LuaBranchFollow follow = new LuaEndBranch();
                    if (hasFollow) {
                        follow = body[^1] as LuaBranchFollow;
                        body.RemoveAt(body.Count - 1);
                    }

                    // Create statement
                    var conditionChunk = new LuaChunk(condition);
                    var bodyChunk = new LuaChunk(body);
                    luaExprs[i] = isIfCondition ? new LuaIfStatement(conditionChunk, bodyChunk, follow) : new LuaIfElseStatement(conditionChunk, bodyChunk, follow);
                    result.Add(luaExprs[i]);

                } else if (luaExprs[i] is LuaKeyword { Keyword: "else" }) {

                    var body = ApplyScopeGroups(luaExprs, i + 1, false, false);
                    luaExprs[i] = new LuaElseStatement(new LuaChunk(body));
                    luaExprs.RemoveRange(i + 1, body.Count + 1);

                    result.Add(luaExprs[i]);

                } else {
                    result.Add(luaExprs[i]);
                }
                i++;
            }
            return result;
        }

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
                    if (binop.Right is LuaTableExpr table) {
                        ApplyOrderOfOperations(table.SubExpressions);
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
                    if (returnStatement.Value is LuaTupleExpr tupleExpr) {
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
                } else if (luaExprs[i] is LuaBranch branch) {
                    luaExprs[i] = RecrusiveOOPBranching(branch);
                } else if (luaExprs[i] is LuaSingleElementParenthesisGroup single) {
                    ApplyOrderOfOperations(single.Arguments);
                    if (single.Arguments.Count == 1) {
                        luaExprs[i] = single.Arguments[0];
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

        static void FlattenTuple(LuaTupleExpr tupleExpr) {

        }

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
                } else if(binop.Right is LuaCallExpr rc) {
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
                        var lhs = new LuaTupleExpr(new List<LuaExpr>() { luaExprs[i], assign.Left });
                        FlattenTuple(lhs);
                        luaExprs[i] = assign with { Left = lhs };
                        luaExprs.RemoveRange(i + 1, 2);
                        ApplyBinop(luaExprs[i] as LuaAssignExpr);
                    } else if (i + 2 < luaExprs.Count && luaExprs[i + 2] is LuaIdentifierExpr or LuaLookupExpr) {
                        luaExprs[i] = new LuaTupleExpr(new List<LuaExpr>() { luaExprs[i], luaExprs[i + 2] });
                        i--;
                    } else {
                        // ERR
                    }
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
                    ApplyGrammar(generic.Body.ScopeBody, 0);
                } else if (expr is LuaNumericForStatement numeric) {
                    ApplyGrammar(numeric.Body.ScopeBody, 0);
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
                ApplyGrammar(branch.Body.ScopeBody, 0);
                if (branch is LuaIfStatement ifbranch) {
                    ApplySingleGrammar(ifbranch.Condition);
                    ApplyBranch(ifbranch.BranchFollow);
                } else if (branch is LuaIfElseStatement ifElseBranch) {
                    ApplySingleGrammar(ifElseBranch.Condition);
                    ApplyBranch(ifElseBranch.BranchFollow);
                }
            }

            // Run through all expressions and apply appropriate late-syntax
            while (i < luaExprs.Count) {
                if (luaExprs[i] is LuaTableExpr table) {
                    ApplyTable(table);
                } else if (luaExprs[i] is LuaBinaryExpr binop) {
                    ApplyBinop(binop);
                } else if (luaExprs[i] is LuaCallExpr call) {
                    ApplyArgs(call.Arguments);
                } else if (luaExprs[i] is LuaFuncExpr func) {
                    ApplyArgs(func.Arguments);
                    ApplyGrammar(func.Body.ScopeBody, 0);
                } else if (luaExprs[i] is LuaIdentifierExpr or LuaLookupExpr or LuaTupleExpr) {
                    ApplyMultivalueAssignment(luaExprs, ref i);
                } else if (luaExprs[i] is LuaKeyword { Keyword: "local" }) {
                    ApplyLocal();
                } else if (luaExprs[i] is LuaGenericForStatement or LuaNumericForStatement) {
                    ApplyForStatement(luaExprs[i]);
                } else if (luaExprs[i] is LuaDoStatement doStatement) {
                    ApplyGrammar(doStatement.Body.ScopeBody, 0);
                } else if (luaExprs[i] is LuaBranch branch) {
                    ApplyBranch(branch);
                }
                i++;
            }

        }

        static void ApplySingleGrammar(LuaExpr exprSrc) {
            if (exprSrc is LuaChunk chunk) {
                ApplyGrammar(chunk.ScopeBody, 0);
            } else {
                ApplyGrammar(new List<LuaExpr> { exprSrc }, 0);
            }
        }

        static void ApplyImplicitBehaviour(List<LuaExpr> luaExprs, int i) {
            
            static void FixIndex(LuaTableExpr luaTable) {
                int j = 0; // using j so we dont confuse with i from outer scope
                int k = 1; // Lua indexing starts from 1
                while (j < luaTable.SubExpressions.Count){
                    if (luaTable.SubExpressions[j] is LuaConstValueExpr or LuaTableExpr or LuaUnaryExpr) {
                        luaTable.SubExpressions[j] = new LuaAssignExpr(new LuaIndexExpr(new LuaConstValueExpr(new LuaNumber(k))), luaTable.SubExpressions[j], false);
                        k++;
                    } else if (luaTable.SubExpressions[j] is LuaBinaryExpr binaryExpr) {
                        if (binaryExpr.Operator != "=") {
                            luaTable.SubExpressions[j] = new LuaAssignExpr(new LuaIndexExpr(new LuaConstValueExpr(new LuaNumber(k))), luaTable.SubExpressions[j], false);
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
                        "." => LuaTokenType.Look,
                        ":" => LuaTokenType.Look,
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
                        "and" => new LuaToken(LuaTokenType.StdOperator, match.Value),
                        "not" => new LuaToken(LuaTokenType.StdOperator, match.Value),
                        "or" => new LuaToken(LuaTokenType.StdOperator, match.Value),
                        "if" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "else" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "elseif" => new LuaToken(LuaTokenType.Keyword, match.Value),
                        "then" => new LuaToken(LuaTokenType.Keyword, match.Value),
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
