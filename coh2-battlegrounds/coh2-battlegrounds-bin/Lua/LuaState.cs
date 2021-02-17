using System;
using System.Collections.Generic;
using System.IO;
using Battlegrounds.Lua.Debugging;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Basic C# implementation of the C-type lua_State for Lua. This is an emulated version of Lua and may run otherwise incorrect/invalid Lua code.
    /// </summary>
    public class LuaState {

        private int m_initialGSize;

#pragma warning disable IDE1006 // Naming Styles (This is intentional in Lua

        /// <summary>
        /// Get the global table containing all data relevant to the <see cref="LuaState"/>.
        /// </summary>
        public LuaTable _G { get; }

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Get the initial size of the <see cref="_G"/> table.
        /// </summary>
        public int InitialGlobalSize => this.m_initialGSize;

        /// <summary>
        /// Create new <see cref="LuaState"/> with <see cref="_G"/> initialized.
        /// </summary>
        public LuaState() {
            
            // Create _G table
            this._G = new LuaTable();
            this._G["_G"] = this._G; // Assign _G to self
            this._G["__version"] = new LuaString("Battlegrounds.Lua V1.0 (Emulates Lua 5.1)");
            
            // Store the size of the initial _G table
            this.m_initialGSize = _G.Size;

        }

        /// <summary>
        /// Do a <see cref="LuaExpr"/> in the current <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="expr">The <see cref="LuaExpr"/> to run in enviornment.</param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        public LuaValue DoExpression(LuaExpr expr) {
            
            // Lua stack
            Stack<LuaValue> stack = new Stack<LuaValue>();

            // Get top value (if none, return _G)
            LuaValue GetTop() {
                if (stack.Count > 0) {
                    return stack.Pop();
                } else {
                    return _G;
                }
            }

            // Lookup function for handling variable lookup
            LuaValue Lookup(LuaValue identifier) {
                var s = GetTop();
                if (s is LuaTable topTable) {
                    return topTable[identifier];
                } else {
                    throw new LuaRuntimeError("Attempt to index '?', a nil value.", stack, this);
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
                                throw new LuaRuntimeError("Attempt to index '?' a nil value.", stack, this);
                            } else {
                                throw new Exception();
                            }
                        } else {
                            DoExpr(bin.Right);
                            DoExpr(bin.Left);
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
                    case LuaValueExpr value:
                        stack.Push(value.Value);
                        break;
                    case LuaIdentifierExpr id: // If at some point we add more lua behaviour, we need to check local registry here before pushing _G)
                        stack.Push(_G);
                        stack.Push(Lookup(new LuaString(id.Identifier)));
                        break;
                    case LuaIndexExpr iex:
                        DoExpr(iex.Key);
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

        /// <summary>
        /// Do a Lua string expression in the current <see cref="LuaState"/> environment.
        /// </summary>
        /// <param name="luaExpression">The lua-code string containing the expression(s) to do.</param>
        /// <returns>The <see cref="LuaValue"/> that was on top of the stack after execution finished.</returns>
        public LuaValue DoString(string luaExpression) {

            // Get expressions
            var expressions = LuaParser.ParseLuaSource(luaExpression);
            if (expressions.Count == 0) {
                // TODO: Error
            }

            // Define lua value to return
            LuaValue value = new LuaNil();

            // Invoke
            try {
                for (int i = 0; i < expressions.Count; i++) {
                    if (expressions[i] is not LuaOpExpr) {
                        value = this.DoExpression(expressions[i]);
                    } else {
                        // TODO: Stuff
                    }
                }
            } catch {
                return null;
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
        public LuaValue DoFile(string luaSourceFilePath) {
            if (File.Exists(luaSourceFilePath)) {
                return this.DoString(File.ReadAllText(luaSourceFilePath));
            } else {
                throw new FileNotFoundException(luaSourceFilePath);
            }
        }

    }

}
