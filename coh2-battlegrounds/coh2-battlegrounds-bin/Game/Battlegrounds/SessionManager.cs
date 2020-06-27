using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Compiler;

namespace Battlegrounds.Game.Battlegrounds {
    
    public enum SessionStatus {
        S_None = -2,
        S_Invalid = -1,
        S_Success,
        S_Compiling,
        S_FailedCompile,
        S_FiledPlay,
        S_Playing,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SessionManager {
    
        /// <summary>
        /// 
        /// </summary>
        public static Session ActiveSession { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static SessionStatus SessionStatus { get; private set; } = SessionStatus.S_None;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCompanyCompilerType"></typeparam>
        /// <param name="session"></param>
        /// <param name="statusChangedCallback"></param>
        public static async void PlaySession<TCompanyCompilerType>(Session session, Action<SessionStatus, Session> statusChangedCallback)
            where TCompanyCompilerType : CompanyCompiler {

            if (SessionStatus != SessionStatus.S_None) {
                return;
            }

            // Set active session
            ActiveSession = session;

            // Set session status
            SessionStatus = SessionStatus.S_Compiling;

            // Play session
            await Task.Run(() => {

                // Compile
                if (!CompileSession<TCompanyCompilerType>()) {
                    UpdateStatus(SessionStatus.S_FailedCompile, statusChangedCallback);
                }

                // Update status to playing
                UpdateStatus(SessionStatus.S_Playing, statusChangedCallback);

                // Launch
                CoH2Launcher.Launch();

            });

        }

        private static bool CompileSession<T>() where T : CompanyCompiler {

            // Create compiler
            SessionCompiler<T> compiler = new SessionCompiler<T>();

            // Try the following
            try {

                // Compile the session
                string luaSessionOutput = compiler.CompileSession(ActiveSession);

                // Write contents to session.scar
                File.WriteAllText("session.scar", luaSessionOutput);

            } catch {

                // Any error ==> return false
                return false;

            }

            // Return the result of the win condition compilation
            return WinconditionCompiler.CompileToSga("temp_build", "session.scar");

        }

        private static void UpdateStatus(SessionStatus status, Action<SessionStatus, Session> callback) {
            SessionStatus = status;
            callback?.Invoke(SessionStatus, ActiveSession);
        }

    }

}
