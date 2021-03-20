using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Battlegrounds.Functional;
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
        public static Stack<LuaValue> DoExpression(LuaState luaState, LuaExpr expr, Stack<LuaValue> stack = null) {

            // Init debug OP id
            int opID = 0;

            // Init Lua stack if none
            if (stack is null) {
                stack = new Stack<LuaValue>();
            }

            // Get top value (if none, return _G)
            LuaValue GetTop(bool local) {
                if (stack.Count > 0) {
                    return stack.Pop();
                } else {
                    return local ? luaState.Envionment : luaState._G;
                }
            }

            // Lookup function for handling variable lookup
            LuaValue Lookup(LuaValue identifier, LuaValue top = null) {
                var s = top is null ? GetTop(false) : top;
                if (s is LuaTable topTable) {
                    return topTable[identifier];
                } else {
                    throw new LuaRuntimeError("Attempt to index '?', a nil value.", stack, luaState);
                }
            }

            int pop = 0;
            bool halt = false;
            bool haltLoop = false;

            // Do lua expressions
            void DoExpr(LuaExpr exp) {
                if (halt) {
                    return;
                }

                // TODO: Make the whole assignment stuff work a bit better, too much code-duplication, stack popping/pushing

                void PushAssignable(LuaExpr lxp) {
                    if (lxp is LuaIdentifierExpr declID) {
                        stack.Push(new LuaString(declID.Identifier));
                    } else if (lxp is LuaIndexExpr indexOp) {
                        DoExpr(indexOp);
                    } else {
                        return;
                    }
                }

                void SingleAssign(bool isLocal, LuaValue identifier = null, LuaValue value = null) {
                    LuaValue tableIdentifier = identifier is null ? stack.Pop() : identifier;
                    LuaValue tableValue = value is null ? stack.Pop() : value;
                    LuaValue assignScope = GetTop(isLocal);
                    if (assignScope is LuaTable scopeTable) {
                        scopeTable[tableIdentifier] = tableValue;
                    } else if (assignScope is LuaNil) {
                        throw new LuaRuntimeError("Attempt to index '?' a nil value.", stack, luaState);
                    } else {
                        throw new Exception();
                    }
                }

                void MultiAssign(bool isLocal, LuaTupleExpr mval) {
                    int stackSize = stack.Count;
                    int j = 0;
                    while (j < mval.Values.Count) {
                        if (j < stackSize) {
                            var v = stack.Pop();
                            stack.Push(luaState._G);
                            stack.Push(v);
                            PushAssignable(mval.Values[j]);
                        } else {
                            stack.Push(new LuaNil());
                            PushAssignable(mval.Values[j]);
                        }
                        SingleAssign(isLocal);
                        j++;
                    }
                }

                void Branch(LuaExpr condition, LuaExpr body, LuaBranchFollow follow) {
                    DoExpr(condition);
                    if (IsFalse(stack.Pop())) {
                        if (follow is not LuaEndBranch) {
                            DoExpr(follow);
                        }
                    } else {
                        DoExpr(body);
                    }
                }

                // Handle expression
                switch (exp) {
                    case LuaNopExpr:
                        break;
                    case LuaAssignExpr assign:
                        DoExpr(assign.Right);
                        if (assign.Left is LuaTupleExpr multival) {
                            MultiAssign(assign.Local, multival);
                        } else {
                            PushAssignable(assign.Left);
                            SingleAssign(assign.Local);
                        }
                        break;
                    case LuaBinaryExpr bin:
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
                            "%" => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(ln % rn),
                                _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {LuaType.NoArithmetic(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                            },
                            "^" => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(Math.Pow(ln, rn)),
                                _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {LuaType.NoArithmetic(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                            },
                            "~=" => new LuaBool(!lhs.Equals(rhs)),
                            "==" => new LuaBool(lhs.Equals(rhs)),
                            ">" => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaBool(ln > rn),
                                _ => throw new LuaRuntimeError($"Attempt to compare {lhs.GetLuaType()} with {rhs.GetLuaType()}.", Repush(stack, lhs, rhs), luaState)
                            },
                            "<" => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaBool(ln < rn),
                                _ => throw new LuaRuntimeError($"Attempt to compare {lhs.GetLuaType()} with {rhs.GetLuaType()}.", Repush(stack, lhs, rhs), luaState)
                            },
                            ">=" => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaBool(ln >= rn),
                                _ => throw new LuaRuntimeError($"Attempt to compare {lhs.GetLuaType()} with {rhs.GetLuaType()}.", Repush(stack, lhs, rhs), luaState)
                            },
                            "<=" => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaBool(ln <= rn),
                                _ => throw new LuaRuntimeError($"Attempt to compare {lhs.GetLuaType()} with {rhs.GetLuaType()}.", Repush(stack, lhs, rhs), luaState)
                            },
                            ".." => lhs switch {
                                LuaNumber ln when rhs is LuaNumber rn => new LuaString(ln.Str() + rn.Str()),
                                LuaNumber ln when rhs is LuaString rs => new LuaString(ln.Str() + rs.Str()),
                                LuaString ls when rhs is LuaNumber rn => new LuaString(ls.Str() + rn.Str()),
                                LuaString ls when rhs is LuaString rs => new LuaString(ls.Str() + rs.Str()),
                                _ => throw new LuaRuntimeError($"Attempt to concatenate a {LuaType.NoConcatenation(lhs, rhs).GetLuaType()} value.", Repush(stack, lhs, rhs), luaState),
                            },
                            _ => throw new Exception(),
                        });                        
                        break;
                    case LuaLookupExpr lookup: 
                        {
                            DoExpr(lookup.Left);
                            switch (lookup.Right) {
                                case LuaIdentifierExpr id:
                                    stack.Push(Lookup(new LuaString(id.Identifier)));
                                    break;
                                case LuaIndexExpr ixe:
                                    DoExpr(ixe.Key);
                                    var keyValue = stack.Pop();
                                    stack.Push(Lookup(keyValue));
                                    break;
                                case LuaLookupExpr lex:
                                    if (lex.Left is LuaIdentifierExpr lexid) {
                                        stack.Push(Lookup(new LuaString(lexid.Identifier)));
                                    } else {
                                        DoExpr(lex.Left);
                                    }
                                    DoExpr(lex.Right);
                                    stack.Push(Lookup(stack.Pop()));
                                    break;
                                default:
                                    throw new Exception();
                            }
                            break;
                        }
                    case LuaTableExpr table:
                        LuaTable t = new LuaTable();
                        stack.Push(t);
                        for (int i = 0; i < table.SubExpressions.Count; i++) {
                            DoExpr(table.SubExpressions[i]);
                            stack.Push(t);
                        }
                        break;
                    case LuaUnaryExpr unary:
                        DoExpr(unary.Expr);
                        LuaValue top = stack.Pop();
                        stack.Push(unary.Operator switch {
                            "-" => top switch {
                                LuaNumber n => new LuaNumber(-n),
                                LuaString s => s.Num().IfTrue(sn => sn is LuaNumber).ThenDo(x => new LuaNumber(-(x as LuaNumber)))
                                .OrDefaultTo(() => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {top.GetLuaType()} value.", Repush(stack, top), luaState)),
                                _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {top.GetLuaType()} value.", Repush(stack, top), luaState),
                            },
                            "#" => top switch {
                                LuaString s => new LuaNumber(s.Length),
                                LuaTable ut => new LuaNumber(ut.Len()),
                                _ => throw new Exception()
                            },
                            _ => throw new Exception()
                        });
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
                    case LuaReturnStatement returnStatement:
                        if (returnStatement.Value is LuaTupleExpr returnTuple) {
                            returnTuple.Values.ForEach(DoExpr);
                        } else {
                            DoExpr(returnStatement.Value);
                        }
                        halt = true;
                        return;
                    case LuaBreakStatement:
                        haltLoop = true;
                        break;
                    case LuaCallExpr call: {
                            
                            // Push arguments
                            call.Arguments.Arguments.ForEach(x => DoExpr(x));

                            // Push closure
                            DoExpr(call.ToCall);

                            // Log trace etc. (For debugging)
                            if (luaState.StackTrace) {
                                string prntName = "???";
                                if (call.ToCall is LuaIdentifierExpr callee) {
                                    prntName = callee.Identifier;
                                }
                                Trace.WriteLine($"[{opID}] LuaCallExpr [{prntName}] ;; Stack := {string.Join(", ", stack.ToList())}", "LuaStackTrace");
                                opID++;
                            }

                            // Pop closure and invoke
                            var topValue = stack.Pop();
                            if (topValue is LuaClosure closure) {
                                pop = closure.Invoke(luaState, stack);
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
                    case LuaChunk chunk: 
                        {
                            var env = luaState.Envionment.Clone();
                            chunk.ScopeBody.ForEach(x => {
                                if (!halt && !haltLoop) {
                                    DoExpr(x);
                                    if (x is LuaCallExpr) { // Is single call expression not used within any context - pop all stack values
                                        pop.Do(i => stack.Count.IfTrue(_ => stack.Count > 0).Then(() => stack.Pop()));
                                    }
                                }
                            });
                            luaState.Envionment = env;
                            break;
                        }
                    case LuaWhileStatement luaWhile: 
                        {
                            var env = luaState.Envionment.Clone();
                            while (!haltLoop && !halt) {
                                DoExpr(luaWhile.Condition);
                                if (IsFalse(stack.Pop())) {
                                    break;
                                }
                                DoExpr(luaWhile.Body);
                            }
                            luaState.Envionment = env;
                        }
                        haltLoop = false;
                        break;
                    case LuaNumericForStatement luaNumFor: 
                        {
                            var env = luaState.Envionment.Clone();
                            DoExpr(luaNumFor.Var.Right);
                            string varId = (luaNumFor.Var.Left as LuaIdentifierExpr).Identifier;
                            LuaNumber controlVar = stack.Pop() as LuaNumber;
                            DoExpr(luaNumFor.Limit);
                            LuaNumber limit = stack.Pop() as LuaNumber;
                            DoExpr(luaNumFor.Step);
                            LuaNumber step = luaNumFor.Step is not LuaNopExpr ? (stack.Pop() as LuaNumber) : new LuaNumber(1);
                            if (controlVar is null || limit is null || step is null) {
                                throw new LuaRuntimeError();
                            }
                            while (!haltLoop && !halt) {
                                if ((step > 0.0 && controlVar <= limit) || (step <= 0 && controlVar >= limit)) {
                                    luaState.Envionment[varId] = controlVar;
                                    DoExpr(luaNumFor.Body);
                                    controlVar = new LuaNumber(controlVar + step);
                                } else {
                                    break;
                                }
                            }
                            luaState.Envionment = env;
                        }
                        haltLoop = false;
                        break;
                    case LuaGenericForStatement luaGenFor: 
                        {
                            var env = luaState.Envionment.Clone();
                            DoExpr(luaGenFor.Iterator);
                            var f = stack.Pop() as LuaClosure;
                            var s = stack.Pop();
                            var v = stack.Pop();
                            while (!haltLoop && !halt) {
                                stack.Push(s);
                                stack.Push(v);
                                int q = f.Invoke(luaState, stack);
                                for (int k = 0; k < luaGenFor.VarList.Variables.Count; k++) {
                                    string varID = luaGenFor.VarList.Variables[k].Identifier;
                                    if (k < q) {
                                        luaState.Envionment[varID] = stack.Pop();
                                        if (k == 0) { v = luaState.Envionment[varID]; }
                                    } else {
                                        luaState.Envionment[varID] = new LuaNil();
                                    }
                                }
                                if (v is LuaNil) {
                                    break;
                                } else {
                                    DoExpr(luaGenFor.Body);
                                }
                            }
                            luaState.Envionment = env;
                        }
                        haltLoop = false;
                        break;
                    case LuaIfStatement lif:
                        Branch(lif.Condition, lif.Body, lif.BranchFollow);
                        break;
                    case LuaIfElseStatement leif:
                        Branch(leif.Condition, leif.Body, leif.BranchFollow);
                        break;
                    case LuaElseStatement le:
                        DoExpr(le.Body);
                        break;
                    default:
                        throw new Exception();
                }

                // Log trace etc. (For debugging)
                if (luaState.StackTrace && exp is not LuaCallExpr) {
                    Trace.WriteLine($"[{opID}] {exp.GetType().Name} ;; Stack := {string.Join(", ", stack.ToList())}", "LuaStackTrace");
                    opID++;
                }

            }

            // Invoke top expression
            DoExpr(expr);

            // Return the whole stack
            return stack;

        }

        private static Stack<LuaValue> Repush(Stack<LuaValue> stack, params LuaValue[] top) {
            for (int i = 0; i < top.Length; i++) {
                stack.Push(top[i]);
            }
            return stack;
        }

        private static bool IsFalse(LuaValue value)
            => value is LuaNil or LuaBool { IsTrue: false };

        //
        // TODO:
        // Wrap the following two methods in proper try-catch statements so we can run "Lua" safely.
        //

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
            LuaValue value;

            // Create chunk
            Stack<LuaValue> stack = new Stack<LuaValue>();
            LuaChunk chunk = new LuaChunk(expressions);

            // Invoke
            try {
                DoExpression(luaState, chunk, stack);
                if (stack.Count > 0) {
                    value = stack.Pop();
                } else {
                    value = new LuaNil();
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
