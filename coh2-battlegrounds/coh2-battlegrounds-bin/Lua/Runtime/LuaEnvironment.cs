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
                        value = new LuaNil();
                        return false;
                    }
                }
            }
        }

        private Stack<EnvFrame> m_frames;
        private EnvFrame m_currentFrame;

        public LuaTable Table => this.m_currentFrame.TableEnv;

        /// <summary>
        /// 
        /// </summary>
        public LuaEnvironment() {
            this.m_currentFrame = new EnvFrame();
            this.m_frames = new Stack<EnvFrame>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void NewFrame() {
            EnvFrame frame = new EnvFrame() { ParentFrame = this.m_currentFrame };
            this.m_frames.Push(this.m_currentFrame);
            this.m_currentFrame = frame;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PopFrame() => this.m_currentFrame = this.m_frames.Pop();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_G"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public LuaValue Lookup(LuaTable _G, string id, out LuaTable table) {
            if (this.m_currentFrame.TryGet(new LuaString(id), out LuaValue value, out table)) {
                return value;
            } else {
                table = _G;
                return _G[id];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public LuaValue Define(string id, LuaValue value) { 
            this.m_currentFrame.TableEnv[id] = value;
            return value; 
        }

    }

}
