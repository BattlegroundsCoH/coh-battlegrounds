using System.Collections.Generic;

namespace Battlegrounds.Lua.Runtime {
    
    /// <summary>
    /// Represents the currently executing Lua variable environment.
    /// </summary>
    public class LuaEnvironment {

        private class EnvFrame {
            public EnvFrame ParentFrame { get; set; }
            public LuaTable TableEnv { get; }
            public EnvFrame() {
                this.TableEnv = new LuaTable();
            }
            public bool TryGet(LuaString key, out LuaValue value, out LuaTable container) {
                if (this.TableEnv.GetIfExists(key, out value)) {
                    container = this.TableEnv;
                    return true;
                } else {
                    if (this.ParentFrame is not null) {
                        return this.ParentFrame.TryGet(key, out value, out container);
                    } else {
                        container = null;
                        value = LuaNil.Nil;
                        return false;
                    }
                }
            }
        }

        private Stack<EnvFrame> m_frames;
        private EnvFrame m_currentFrame;

        public LuaTable Table => this.m_currentFrame.TableEnv;

        /// <summary>
        /// Initialize a new and empty <see cref="LuaEnvironment"/> class.
        /// </summary>
        public LuaEnvironment() {
            this.m_currentFrame = new EnvFrame();
            this.m_frames = new Stack<EnvFrame>();
        }

        /// <summary>
        /// Push a new environment frame.
        /// </summary>
        public void NewFrame() {
            EnvFrame frame = new EnvFrame() { ParentFrame = this.m_currentFrame };
            this.m_frames.Push(this.m_currentFrame);
            this.m_currentFrame = frame;
        }

        /// <summary>
        /// Pop the current environment frame.
        /// </summary>
        public void PopFrame() => this.m_currentFrame = this.m_frames.Pop();

        /// <summary>
        /// Lookup a variable in the environment.
        /// </summary>
        /// <param name="_G">The global table.</param>
        /// <param name="id">The string ID of the variable to lookup.</param>
        /// <param name="table">The environment table containing the variable.</param>
        /// <returns>The value of the variable.</returns>
        public LuaValue Lookup(LuaTable _G, string id, out LuaTable table) {
            if (this.m_currentFrame.TryGet(new LuaString(id), out LuaValue value, out table)) {
                return value;
            } else {
                table = _G;
                return _G[id];
            }
        }

        /// <summary>
        /// Define a variable in the current environment frame.
        /// </summary>
        /// <param name="id">The string ID</param>
        /// <param name="value">The value of the variable</param>
        /// <returns><paramref name="value"/>.</returns>
        public LuaValue Define(string id, LuaValue value) { 
            this.m_currentFrame.TableEnv[id] = value;
            return value; 
        }

    }

}
