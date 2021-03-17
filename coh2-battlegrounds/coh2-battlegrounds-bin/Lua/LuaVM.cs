using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Parsing;

namespace Battlegrounds.Lua {

    /// <summary>
    /// C# implementation of an interpretted Lua language. This is an emulated version of Lua and may run otherwise incorrect/invalid Lua code.
    /// </summary>
    public static class LuaVM {

        /// <summary>
        /// Do a <see cref="LuaExpr"/> in the current <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="luaState"></param>
        /// <param name="expr">The <see cref="LuaExpr"/> to run in enviornment.</param>
        /// <param name="stack"></param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        public static LuaValue DoExpression(LuaState luaState, LuaExpr expr, Stack<LuaValue> stack = null) {

            // Init Lua stack if none
            if (stack is null) {
                stack = new Stack<LuaValue>();
            }

            // Get top value (if none, return _G)
            LuaValue GetTop() {
                if (stack.Count > 0) {
                    return stack.Pop();
                } else {
                    return luaState._G;
                }
            }

            // Lookup function for handling variable lookup
            LuaValue Lookup(LuaValue identifier) {
                var s = GetTop();
                if (s is LuaTable topTable) {
                    return topTable[identifier];
                } else {
                    throw new LuaRuntimeError("Attempt to index '?', a nil value.", stack, luaState);
                }
            }

            // Do lua expressions
            void DoExpr(LuaExpr exp) {

                // Handle expression
                switch (exp) {
                    case LuaBinaryExpr bin:
                        if (bin.Operator.CompareTo("=") == 0) {
                            DoExpr(bin.Right);
                            if (bin.Left is LuaIdentifierExpr declID) {
                                stack.Push(new LuaString(declID.Identifier));
                            } else if (bin.Left is LuaIndexExpr indexOp) {
                                DoExpr(indexOp);
                            } else {
                                return;
                            }
                            LuaValue tableIdentifier = stack.Pop();
                            LuaValue tableValue = stack.Pop();
                            LuaValue scope = GetTop();
                            if (scope is LuaTable scopeTable) {
                                scopeTable[tableIdentifier] = tableValue;
                            } else if (scope is LuaNil) {
                                throw new LuaRuntimeError("Attempt to index '?' a nil value.", stack, luaState);
                            } else {
                                throw new Exception();
                            }
                        } else {
                            DoExpr(bin.Right);
                            DoExpr(bin.Left);
                            LuaValue lhs = stack.Pop();
                            LuaValue rhs = stack.Pop();
                            stack.Push(bin.Operator switch {
                                "+" => lhs switch {
                                    LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(ln + rn),
                                    _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {LuaType.NoArithmetic(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                                },
                                "-" => lhs switch {
                                    LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(ln - rn),
                                    _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {LuaType.NoArithmetic(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                                },
                                "*" => lhs switch {
                                    LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(ln * rn),
                                    _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {LuaType.NoArithmetic(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                                },
                                "/" => lhs switch {
                                    LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(ln / rn),
                                    _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {LuaType.NoArithmetic(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                                },
                                _ => throw new Exception(),
                            });
                        }
                        break;
                    case LuaLookupExpr lookup:
                        DoExpr(lookup.Left);
                        switch (lookup.Right) {
                            case LuaIdentifierExpr lid:
                                stack.Push(Lookup(new LuaString(lid.Identifier)));
                                break;
                            case LuaIndexExpr ixe:
                                DoExpr(ixe.Key);
                                stack.Push(Lookup(stack.Pop()));
                                break;
                            default:
                                throw new Exception();
                        }
                        break;
                    case LuaTableExpr table:
                        LuaTable t = new LuaTable();
                        stack.Push(t);
                        for (int i = 0; i < table.SubExpressions.Count; i++) {
                            DoExpr(table.SubExpressions[i]);
                            stack.Push(t);
                        }
                        break;
                    case LuaNegateExpr negateExpr:
                        DoExpr(negateExpr.Expr);
                        LuaValue top = stack.Pop();
                        if (top is LuaNumber n) {
                            stack.Push(new LuaNumber(-n));
                        } else {
                            throw new LuaRuntimeError($"Attempt to perform arithmetic on a {top.GetLuaType()} value.", Repush(stack, top), luaState);
                        }
                        break;
                    case LuaConstValueExpr value:
                        stack.Push(value.Value);
                        break;
                    case LuaIdentifierExpr id:
                        if (luaState.Envionment.GetIfExists(id.Identifier, out LuaValue envValue)) {
                            stack.Push(envValue);
                        } else {
                            stack.Push(luaState._G);
                            stack.Push(Lookup(new LuaString(id.Identifier)));
                        }
                        break;
                    case LuaIndexExpr iex:
                        DoExpr(iex.Key);
                        break;
                    case LuaCallExpr call: {
                            
                            // Push arguments
                            call.Arguments.Arguments.ForEach(x => DoExpr(x));

                            // Push closure
                            DoExpr(call.ToCall);
                            
                            // Pop closure and invoke
                            var topValue = stack.Pop();
                            if (topValue is LuaClosure closure) {
                                closure.Invoke(luaState, stack);
                            } else {
                                throw new LuaRuntimeError($"Attempt to run a {topValue.GetLuaType()} value.");
                            }

                            break;
                        }
                    case LuaFuncExpr func: {
                            // TODO: Capture locals
                            string[] parameters = func.Arguments.Arguments.Select(x => (x as LuaIdentifierExpr).Identifier).ToArray();
                            LuaClosure closure = new LuaClosure(new LuaFunction(func.Body, parameters));
                            stack.Push(closure);
                            break;
                        }
                    case LuaScope scope:
                        scope.ScopeBody.ForEach(x => DoExpr(x));
                        break;
                    default:
                        throw new Exception();
                }
            }

            // Invoke top expression
            DoExpr(expr);

            // Return whatever's on top (or nil if nothing on top)
            if (stack.Count >= 1)
                return stack.Pop();
            else
                return new LuaNil();

        }

        //
        // TODO:
        // Wrap the following two methods in proper try-catch statements so we can run "Lua" safely.
        //

        private static Stack<LuaValue> Repush(Stack<LuaValue> stack, params LuaValue[] top) {
            for (int i = 0; i < top.Length; i++) {
                stack.Push(top[i]);
            }
            return stack;
        }

        /// <summary>
        /// Do a Lua string expression in the current <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="luaExpression">The lua-code string containing the expression(s) to do.</param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        public static LuaValue DoString(LuaState luaState, string luaExpression) {

            // Get expressions
            var expressions = LuaParser.ParseLuaSource(luaExpression);
            if (expressions.Count == 0) {
                return new LuaNil();
            }

            // Define lua value to return
            LuaValue value = new LuaNil();

            // Invoke
            try {
                for (int i = 0; i < expressions.Count; i++) {
                    if (expressions[i] is not LuaOpExpr) {
                        value = DoExpression(luaState, expressions[i]);
                    } else {
                        // TODO: Stuff
                    }
                }
            } catch (LuaRuntimeError runtime) {
                luaState.SetLastError(runtime);
                Trace.WriteLine(runtime.Message, "LuaRuntimeError");
                return new LuaNil();
            } catch {
                throw;
            }

            // Return the lua value
            return value;

        }

        /// <summary>
        /// Do a Lua file containing Lua source code in the current <see cref="LuaState"/> envionment.
        /// </summary>
        /// <param name="luaSourceFilePath"></param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        /// <exception cref="FileNotFoundException"/>
        public static LuaValue DoFile(LuaState luaState, string luaSourceFilePath) {
            if (File.Exists(luaSourceFilePath)) {
                return DoString(luaState, File.ReadAllText(luaSourceFilePath));
            } else {
                throw new FileNotFoundException(luaSourceFilePath);
            }
        }

    }

}
