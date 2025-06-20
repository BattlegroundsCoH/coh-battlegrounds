﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Battlegrounds.Scripting.Lua.Exceptions;
using Battlegrounds.Scripting.Lua.Parsing;

using static Battlegrounds.Scripting.Lua.Interpreter.LuaNil;

namespace Battlegrounds.Scripting.Lua.Interpreter;

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
    public static LuaStack DoExpression(LuaState luaState, LuaExpr expr, LuaStack? stack = null) {

        // Init debug OP id
        int opID = 0;

        // Init Lua stack if none
        if (stack is null) {
            stack = new();
        }

        // Init run-values
        int pop = 0;
        bool halt = false;
        bool haltLoop = false;

        // Debug trace
        void TraceDb(string ins, params string[] args) {
            if (luaState.DebugMode) {
                string name = args.Length == 0 ? ins : $"{ins} [{string.Join(", ", args)}]";
                string stackContent = luaState.StackTrace ? $" Stack := {string.Join(", ", stack.ToString(true))}" : string.Empty;
                Trace.WriteLine($"[{opID}] {name} ;;{stackContent}", "LuaStackTrace");
                opID++;
            }
        }

        // Get top value (if none, return _G)
        LuaValue GetTop(bool local) {
            if (stack.Any && stack.Peek() is LuaTable or LuaUserObject) {
                return stack.Pop();
            } else {
                return local ? luaState.Envionment.Table : luaState._G;
            }
        }

        // Lookup function for handling variable lookup
        LuaValue Lookup(LuaValue identifier, LuaValue top = null) {
            var s = top is null ? GetTop(false) : top;
            if (s is IMetatableParent topTable) {
                return LuaMetatableUtil.__Index(topTable, identifier, luaState);
            } else {
                throw new LuaRuntimeError("Attempt to index '?', a nil value.", stack, luaState);
            }
        }

        // Do the expression and pop whatever was last pushed to stack
        LuaValue DoExprAndPop(LuaExpr exp) {
            DoExpr(exp);
            return stack.Pop();
        }

        // Get assignable value
        (LuaTable scope, LuaValue key) GetAssignable(LuaIdentifierExpr idExpr, bool declareLocal) {
            LuaString lstrKey = new LuaString(idExpr.Identifier);
            if (declareLocal) {
                luaState.Envionment.Define(idExpr.Identifier, Nil);
                return (luaState.Envionment.Table, new LuaString(idExpr.Identifier));
            }
            if (luaState.Envionment.Lookup(GetTop(false) as LuaTable, idExpr.Identifier, out LuaTable envt) is LuaValue v) {
                return (envt, new LuaString(idExpr.Identifier));
            } else {
                throw new Exception();
            }
        }

        // Execute branch
        void Branch(LuaExpr condition, LuaChunk body, LuaBranchFollow follow) {
            DoExpr(condition);
            if (IsFalse(stack.Pop())) {
                TraceDb("LuaBranch::False");
                if (follow is not LuaEndBranch) {
                    DoExpr(follow);
                }
            } else {
                TraceDb("LuaBranch::True");
                DoChunk(body, true);
            }
        }

        // Do a call
        void DoCall(LuaCallExpr call) {

            // Push arguments
            call.Arguments.Arguments.ForEach(x => DoExpr(x));

            // Push closure
            DoExpr(call.ToCall);

            // Log instruction (For debugging)
            if (luaState.DebugMode) {
                string prntName = "???";
                if (call.ToCall is LuaIdentifierExpr callee) {
                    prntName = callee.Identifier;
                }
                TraceDb(nameof(LuaCallExpr), prntName);
                luaState.PushStackTrace(call.SourcePos, prntName);
            } else {
                luaState.PushStackTrace(call.SourcePos, "?");
            }

            // Pop closure and invoke
            var topValue = stack.Pop();
            if (topValue is LuaClosure closure) {
                pop = closure.Invoke(luaState, stack);
                if (call.ReturnCount != -1) {
                    pop = stack.ShiftLeft(pop - call.ReturnCount);
                }
            } else {
                throw new LuaRuntimeError($"Attempt to run a {topValue.GetLuaType()} value.");
            }

        }

        // Execute chunk
        void DoChunk(LuaChunk chunk, bool stackframe) {
            if (stackframe) {
                luaState.Envionment.NewFrame();
            }
            chunk.ScopeBody.ForEach(x => {
                if (!halt && !haltLoop) {
                    DoExpr(x);
                    if (x is LuaCallExpr) { // Is single call expression not used within any context - pop all stack values
                        stack.Pop(pop);
                    }
                }
            });
            if (stackframe) {
                luaState.Envionment.PopFrame();
            }
        }

        // Do lua expressions
        void DoExpr(LuaExpr exp) {
            if (halt) {
                return;
            }

            // Handle expression
            switch (exp) {
                case LuaNopExpr:
                    TraceDb(nameof(LuaNopExpr));
                    break;
                case LuaAssignExpr assign:
                    List<string> assignedValues = new List<string>(); {
                        if (assign.Left is not LuaExpressionList varExprLst) {
                            varExprLst = new LuaExpressionList(new List<LuaExpr>() { assign.Left }, LuaSourcePos.Undefined);
                        }
                        int stackTop = stack.Top;
                        DoExpr(assign.Right);
                        int stackNewTop = stack.Top;
                        int vars = varExprLst.Values.Count;
                        int stackDelta = stackNewTop - stackTop;
                        for (int k = 0; k < vars; k++) {
                            LuaValue value = Nil;
                            if (stackDelta > 0) {
                                value = stack.Pop();
                                stackDelta--;
                            }
                            (LuaTable scope, LuaValue key) = varExprLst.Values[k] switch {
                                LuaIdentifierExpr idExpr => GetAssignable(idExpr, assign.Local),
                                LuaIndexExpr indExpr => (stack.Pop() as LuaTable, DoExprAndPop(indExpr.Key)),
                                LuaLookupExpr look => (DoExprAndPop(look.Left) as LuaTable, new LuaString((look.Right as LuaIdentifierExpr).Identifier)),
                                _ => throw new Exception()
                            };
                            assignedValues.Add(key.Str());
                            scope[key] = value;
                        }
                    }
                    TraceDb(nameof(LuaAssignExpr), assignedValues.ToArray());
                    break;
                case LuaLogicExpr logic: {
                    DoExpr(logic.Left);
                    var lhs = stack.Pop();
                    stack.Push(logic.Operator switch {
                        "and" => lhs switch {
                            LuaBool b when !b.IsTrue => lhs,
                            LuaNil => lhs,
                            _ => DoExprAndPop(logic.Right)
                        },
                        "or" => lhs switch {
                            LuaBool b when !b.IsTrue => DoExprAndPop(logic.Right),
                            LuaNil => DoExprAndPop(logic.Right),
                            _ => lhs
                        },
                        _ => throw new Exception()
                    });
                }
                TraceDb(nameof(LuaLogicExpr), logic.Operator);
                break;
                case LuaBinaryExpr bin: {
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
                            LuaNumber ln when rhs is LuaNumber rn => new LuaNumber(ln - Math.Floor(ln / rn) * rn),
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
                        "or" => lhs switch {
                            _ => throw new LuaRuntimeError()
                        },
                        "and" => lhs switch {
                            _ => throw new LuaRuntimeError()
                        },
                        _ => throw new Exception(),
                    });
                }
                TraceDb(nameof(LuaBinaryExpr), bin.Operator);
                break;
                case LuaLookupExpr lookup: {
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
                    TraceDb(lookup.GetType().Name);
                    break;
                }
                case LuaTableExpr table:
                    LuaTable t = new LuaTable(luaState);
                    stack.Push(t);
                    for (int i = 0; i < table.SubExpressions.Count; i++) {
                        DoExpr(table.SubExpressions[i]);
                        stack.Push(t);
                    }
                    TraceDb(nameof(LuaTableExpr), t.GetHashCode().ToString());
                    break;
                case LuaUnaryExpr unary:
                    DoExpr(unary.Expr);
                    LuaValue top = stack.Pop();
                    stack.Push(unary.Operator switch {
                        "-" => top switch {
                            LuaNumber n => new LuaNumber(-n),
                            LuaString s => s.Num() is LuaNumber nx ? new LuaNumber(-nx) :  throw new LuaRuntimeError($"Attempt to perform arithmetic on a {top.GetLuaType()} value."),
                            _ => throw new LuaRuntimeError($"Attempt to perform arithmetic on a {top.GetLuaType()} value.", Repush(stack, top), luaState),
                        },
                        "#" => top switch {
                            LuaString s => new LuaNumber(s.Length),
                            LuaTable ut => new LuaNumber(ut.Len()),
                            _ => throw new Exception()
                        },
                        "not" => top switch {
                            LuaNil => new LuaBool(true),
                            LuaBool b => new LuaBool(!b.IsTrue),
                            _ => new LuaBool(false)
                        },
                        _ => throw new Exception()
                    });
                    TraceDb(nameof(LuaUnaryExpr), unary.Operator);
                    break;
                case LuaConstValueExpr value:
                    TraceDb(nameof(LuaConstValueExpr), value.Value.Str());
                    stack.Push(value.Value);
                    break;
                case LuaIdentifierExpr id:
                    TraceDb(nameof(LuaIndexExpr), id.Identifier, "global");
                    stack.Push(luaState.Envionment.Lookup(luaState._G, id.Identifier, out _));
                    break;
                case LuaIndexExpr iex:
                    DoExpr(iex.Key);
                    TraceDb(nameof(LuaIndexExpr));
                    break;
                case LuaReturnStatement returnStatement:
                    if (returnStatement.Value is LuaExpressionList returnTuple) {
                        returnTuple.Values.ForEach(DoExpr);
                        TraceDb(nameof(LuaReturnStatement), returnTuple.Values.Count.ToString());
                    } else {
                        DoExpr(returnStatement.Value);
                        TraceDb(nameof(LuaReturnStatement), "1");
                    }
                    halt = true;
                    return;
                case LuaBreakStatement:
                    TraceDb(nameof(LuaBreakStatement));
                    haltLoop = true;
                    break;
                case LuaSelfCallExpr selfCall: {
                    // Handle 'self' argument
                    if (selfCall.ToCall is LuaLookupExpr lookup) {
                        stack.Lock(stack.Top);
                        DoExpr(lookup.Left); // Do LHS (--> SELF)
                    } else {
                        throw new LuaRuntimeError();
                    }
                    DoCall(selfCall);
                    stack.Unlock();
                }
                break;
                case LuaCallExpr call:
                    DoCall(call);
                    break;
                case LuaFuncExpr func: {
                    // TODO: Capture locals
                    string[] parameters = func.Arguments.Arguments.Select(x => (x as LuaIdentifierExpr).Identifier).ToArray();
                    LuaClosure closure = new LuaClosure(new LuaFunction(func.Body, parameters));
                    stack.Push(closure);
                    TraceDb(nameof(LuaFuncExpr), closure.Str());
                    break;
                }
                case LuaChunk chunk:
                    TraceDb(nameof(LuaChunk));
                    DoChunk(chunk, true);
                    break;
                case LuaWhileStatement luaWhile:
                    TraceDb(nameof(LuaWhileStatement)); {
                        while (!haltLoop && !halt) {
                            DoExpr(luaWhile.Condition);
                            if (IsFalse(stack.Pop())) {
                                break;
                            } else {
                                DoChunk(luaWhile.Body, true);
                            }
                        }
                    }
                    haltLoop = false;
                    break;
                case LuaNumericForStatement luaNumFor:
                    TraceDb(nameof(LuaNumericForStatement)); {
                        luaState.Envionment.NewFrame();
                        DoExpr(luaNumFor.Var.Right);
                        string varId = (luaNumFor.Var.Left as LuaIdentifierExpr).Identifier;
                        if (stack.Pop() is LuaNumber controlVar) {
                            DoExpr(luaNumFor.Limit);
                            if (stack.Pop() is LuaNumber limit) {
                                DoExpr(luaNumFor.Step);
                                if ((luaNumFor.Step is not LuaNopExpr ? stack.Pop() as LuaNumber : new LuaNumber(1)) is LuaNumber step) {
                                    while (!haltLoop && !halt) {
                                        if (step > 0.0 && controlVar <= limit || step <= 0 && controlVar >= limit) {
                                            luaState.Envionment.Define(varId, controlVar);
                                            DoChunk(luaNumFor.Body, true);
                                            controlVar = new LuaNumber(controlVar + step);
                                        } else {
                                            break;
                                        }
                                    }
                                } else {
                                    throw new LuaRuntimeError();
                                }
                            } else {
                                throw new LuaRuntimeError();
                            }
                        } else {
                            throw new LuaRuntimeError();
                        }
                        luaState.Envionment.PopFrame();
                    }
                    haltLoop = false;
                    break;
                case LuaGenericForStatement luaGenFor:
                    TraceDb(nameof(LuaGenericForStatement)); {
                        luaState.Envionment.NewFrame();
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
                                    var cv = luaState.Envionment.Define(varID, stack.Pop());
                                    if (k == 0) { v = cv; }
                                } else {
                                    luaState.Envionment.Define(varID, Nil);
                                }
                            }
                            if (v is LuaNil) {
                                break;
                            } else {
                                DoChunk(luaGenFor.Body, true);
                            }
                        }
                        luaState.Envionment.PopFrame();
                    }
                    haltLoop = false;
                    break;
                case LuaRepeatStatement lrs:
                    TraceDb(nameof(LuaRepeatStatement)); {
                        while (!halt && !haltLoop) {
                            luaState.Envionment.NewFrame();
                            DoChunk(lrs.Body, false);
                            DoExpr(lrs.Condition);
                            if (!IsFalse(stack.Pop())) {
                                break;
                            }
                            luaState.Envionment.PopFrame();
                        }
                    }
                    haltLoop = false;
                    break;
                case LuaIfStatement lif:
                    TraceDb(nameof(LuaIfStatement));
                    Branch(lif.Condition, lif.Body, lif.BranchFollow);
                    break;
                case LuaIfElseStatement leif:
                    TraceDb(nameof(LuaIfElseStatement));
                    Branch(leif.Condition, leif.Body, leif.BranchFollow);
                    break;
                case LuaElseStatement le:
                    TraceDb(nameof(LuaElseStatement));
                    DoChunk(le.Body, true);
                    break;
                case LuaDoStatement ldo:
                    TraceDb(nameof(LuaDoStatement));
                    DoChunk(ldo.Body, true);
                    break;
                case LuaExpressionList ltuple:
                    for (int j = 0; j < ltuple.Values.Count; j++) {
                        DoExpr(ltuple.Values[ltuple.Values.Count - 1 - j]);
                    }
                    break;
                default:
                    throw new Exception();
            }

        }

        // Invoke top expression
        DoExpr(expr);

        // Return the whole stack
        return stack;

    }

    private static LuaStack Repush(LuaStack stack, params LuaValue[] top) {
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
    /// 
    /// </summary>
    /// <param name="luaState"></param>
    /// <param name="chunk"></param>
    /// <returns></returns>
    public static LuaValue RunChunk(LuaState luaState, LuaChunk chunk) {

        // Define lua value to return
        LuaValue value;

        // Init stack
        LuaStack stack = new();

        // Invoke
        try {
            luaState.PushStackTrace(LuaSourcePos.Undefined, "main chunk");
            DoExpression(luaState, chunk, stack);
            if (stack.Any) {
                value = stack.Pop();
            } else {
                value = Nil;
            }
        } catch (LuaRuntimeError runtime) {
            luaState.SetLastError(runtime);
            Trace.WriteLine(runtime.Message, "LuaRuntimeError");
            return Nil;
        } catch {
            throw;
        }

        // Return the lua value
        return value;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="luaState"></param>
    /// <param name="luaExpression"></param>
    /// <param name="luaSource"></param>
    /// <returns></returns>
    public static LuaChunk LoadChunk(LuaState luaState, string luaExpression, string luaSource = "?.lua") {

        // Get expressions
        if (LuaParser.ParseLuaSourceToChunk(out LuaChunk chunk, luaExpression, luaSource) is LuaSyntaxError lse) {

            // The error message
            string syntaxErrMessage = $"{lse.SourcePos}: {lse.Message}";

            // Dump syntax error
            Trace.WriteLine(syntaxErrMessage, "Lua Syntax Error");
            if (!string.IsNullOrEmpty(lse.Suggestion)) {
                Trace.WriteLine(lse.Suggestion, "Lua Syntax Error - Suggestion");
            }

            // Set syntax error
            luaState.SetLastError(lse);

            // Return error message as string
            return null;

        } else {

            // Return result
            return chunk;

        }

    }

    /// <summary>
    /// Do a Lua string expression in the current <see cref="LuaState"/> environment.
    /// </summary>
    /// <param name="luaState">The <see cref="LuaState"/> to use when running the code.</param>
    /// <param name="luaExpression">The lua-code string containing the expression(s) to do.</param>
    /// <param name="luaSource">The source name. By default "?.lua".</param>
    /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
    public static LuaValue DoString(LuaState luaState, string luaExpression, string luaSource = "?.lua") {

        // Load chunk
        if (LoadChunk(luaState, luaExpression, luaSource) is LuaChunk chunk) {

            // Reun the chunk
            return RunChunk(luaState, chunk);

        } else {

            // Return nil
            return Nil;

        }

    }

    /// <summary>
    /// Do a Lua file containing Lua source code in the current <see cref="LuaState"/> envionment.
    /// </summary>
    /// <param name="luaState">The state to execute the file in.</param>
    /// <param name="luaSourceFilePath">The location of the Lua source code.</param>
    /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
    /// <exception cref="FileNotFoundException"/>
    public static LuaValue DoFile(LuaState luaState, string luaSourceFilePath) {
        if (File.Exists(luaSourceFilePath)) {
            return DoString(luaState, File.ReadAllText(luaSourceFilePath), Path.GetFileName(luaSourceFilePath));
        } else {
            throw new FileNotFoundException(luaSourceFilePath);
        }
    }

    /// <summary>
    /// Evaluate a single and raw lua expression and get result of expression.
    /// </summary>
    /// <param name="luaState">The state to execute the raw expression in.</param>
    /// <param name="luaExpression">The actual string containing a single lua expression to evaluate.</param>
    /// <returns>If no syntax-error occured, the result of the evaluation; Otherwise a <see cref="LuaString"/> describing the syntax error.</returns>
    public static LuaValue DoRawString(LuaState luaState, string luaExpression) {

        // Get expressions
        if (LuaParser.ParseLuaSourceSafe(out List<LuaExpr> ast, luaExpression) is LuaSyntaxError lse) {

            // The error message
            string syntaxErrMessage = $"{lse.SourcePos}: {lse.Message}";

            // Dump syntax error
            Trace.WriteLine(syntaxErrMessage, "Lua Syntax Error");
            if (!string.IsNullOrEmpty(lse.Suggestion)) {
                Trace.WriteLine(lse.Suggestion, "Lua Syntax Error - Suggestion");
            }

            // Set syntax error
            luaState.SetLastError(lse);
            // Return error message as string
            return new LuaString(syntaxErrMessage);

        }

        // Reun the chunk
        return RunChunk(luaState, new LuaChunk(ast, LuaSourcePos.Undefined));

    }

    /// <summary>
    /// Load but do not run a Lua string.
    /// </summary>
    /// <param name="luaState">The state to execute the raw expression in.</param>
    /// <param name="luaExpression">The actual string containing lua expression(s) to load.</param>
    /// <param name="luaSource">The name of the source file containing the expression.</param>
    /// <returns>If no syntax error occured, a <see cref="LuaClosure"/> is returned, containing the parsed lua chunk to be executed; Otherwise <see cref="LuaNil"/>.</returns>
    public static LuaValue LoadString(LuaState luaState, string luaExpression, string luaSource = "?.lua") {

        if (LoadChunk(luaState, luaExpression, luaSource) is LuaChunk chunk) {
            return new LuaClosure(new LuaFunction(chunk));
        } else {
            return Nil;
        }

    }

    /// <summary>
    /// Loads the contents of a lua source file without running the loaded code.
    /// </summary>
    /// <param name="luaState">The state to execute the raw expression in.</param>
    /// <param name="luaSource">The name of the source file containing the expression.</param>
    /// <returns>If no syntax error occured, a <see cref="LuaClosure"/> is returned, containing the parsed lua chunk to be executed; Otherwise <see cref="LuaNil"/>.</returns>
    public static LuaValue LoadFile(LuaState luaState, string luaSource) {
        if (File.Exists(luaSource)) {
            return LoadString(luaState, File.ReadAllText(luaSource), Path.GetFileName(luaSource));
        } else {
            throw new FileNotFoundException();
        }
    }

}
