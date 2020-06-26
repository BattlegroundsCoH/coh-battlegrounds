using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Util;

namespace Battlegrounds.Compiler {
    
    /// <summary>
    /// 
    /// </summary>
    public class SessionCompiler {

        /// <summary>
        /// 
        /// </summary>
        public SessionCompiler() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public virtual string CompileSession(Session session) {

            TxtBuilder lua = new TxtBuilder();



            return lua.GetContent();

        }

    }

}
