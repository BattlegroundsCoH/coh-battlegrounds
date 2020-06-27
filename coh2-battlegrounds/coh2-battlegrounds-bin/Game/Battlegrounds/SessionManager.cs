using System;
using System.IO;
using System.Threading.Tasks;
using Battlegrounds.Compiler;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Status code for a <see cref="Session"/> being run in the <see cref="SessionManager"/>.
    /// </summary>
    public enum SessionStatus {

        /// <summary>
        /// Invalid session (Session was null)
        /// </summary>
        S_Invalid = -1,

        /// <summary>
        /// Session was completed without errors
        /// </summary>
        S_Success,

        /// <summary>
        /// The session is currently compiling
        /// </summary>
        S_Compiling,

        /// <summary>
        /// The session failed to compile
        /// </summary>
        S_FailedCompile,

        /// <summary>
        /// The session failed to play
        /// </summary>
        S_FailedPlay,

        /// <summary>
        /// The game was not launched
        /// </summary>
        S_GameNotLaunched,

        /// <summary>
        /// The game was played without causing a crash
        /// </summary>
        S_GamePlayedWithoutCrash,

        /// <summary>
        /// The played match is currently being analyzed
        /// </summary>
        S_Analyzing,

        /// <summary>
        /// The analysis of the match failed
        /// </summary>
        S_AnalysisFailed,

        /// <summary>
        /// The playback file was found
        /// </summary>
        S_NoPlayback,

        /// <summary>
        /// The session is currently being played.
        /// </summary>
        S_Playing,

    }

    /// <summary>
    /// Static management class for playing a <see cref="Session"/> in the proper order.
    /// </summary>
    public static class SessionManager {

        /// <summary>
        /// The instance of the currently active <see cref="Session"/>.
        /// </summary>
        public static Session ActiveSession { get; private set; }

        /// <summary>
        /// The current status of the <see cref="SessionManager"/>.
        /// </summary>
        public static SessionStatus SessionStatus { get; private set; } = SessionStatus.S_Success;

        /// <summary>
        /// Plays a <see cref="Session"/> in a controlled flow of execution wherein callbacks are used to report on the status of the <see cref="Session"/>. Once done, a <see cref="GameMatch"/> instance can be retrieved to see match results.
        /// </summary>
        /// <typeparam name="TSessionCompilerType">The session compiler type</typeparam>
        /// <typeparam name="TCompanyCompilerType">The company compiler type</typeparam>
        /// <param name="session">The <see cref="Session"/> instance to play and analyze.</param>
        /// <param name="statusChangedCallback">Callback for whenever the status of the play session has changed</param>
        /// <param name="matchAnalyzedCallback">Callback called only when the match has been played and analyzed</param>
        public static async void PlaySession<TSessionCompilerType, TCompanyCompilerType>(Session session, Action<SessionStatus, Session> statusChangedCallback, Action<GameMatch> matchAnalyzedCallback)
            where TSessionCompilerType : SessionCompiler<TCompanyCompilerType>
            where TCompanyCompilerType : CompanyCompiler {

            // Make sure no session is active
            if (SessionStatus == SessionStatus.S_Playing || SessionStatus == SessionStatus.S_Analyzing || 
                SessionStatus == SessionStatus.S_Compiling || SessionStatus == SessionStatus.S_GamePlayedWithoutCrash) {
                return;
            }

            // Make sure we've been given a valid session
            if (session == null) {
                UpdateStatus(SessionStatus.S_Invalid, statusChangedCallback);
                return;
            }

            // Set active session
            ActiveSession = session;

            // Set session status
            UpdateStatus(SessionStatus.S_Compiling, statusChangedCallback);

            // Play session
            await Task.Run(() => {

                // Compile
                if (!CompileSession<TSessionCompilerType, TCompanyCompilerType>()) {
                    UpdateStatus(SessionStatus.S_FailedCompile, statusChangedCallback);
                    return;
                }

                // Launch the game
                if (!CoH2Launcher.Launch()) {
                    UpdateStatus(SessionStatus.S_GameNotLaunched, statusChangedCallback);
                    return;
                }

                // Update status to playing
                UpdateStatus(SessionStatus.S_Playing, statusChangedCallback);

                // Watch the CoH2 process (wait for it to exit)
                int p = CoH2Launcher.WatchProcess();

                // Did it fail?
                if (p == 1) {
                    UpdateStatus(SessionStatus.S_GameNotLaunched, statusChangedCallback);
                    return;
                }

                // Let the callback know the game was played without a crash
                UpdateStatus(SessionStatus.S_GamePlayedWithoutCrash, statusChangedCallback);

                // Make sure the .rec file exists and is valid
                if (!HasPlayback()) {
                    UpdateStatus(SessionStatus.S_NoPlayback, statusChangedCallback);
                }

            });

            // Update status
            UpdateStatus(SessionStatus.S_Analyzing, statusChangedCallback);

            // Create the match object
            GameMatch match = new GameMatch(ActiveSession);

            // Load the match
            if (!match.LoadMatch($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec")) {
                UpdateStatus(SessionStatus.S_AnalysisFailed, statusChangedCallback);
            }

            // Evaluate the result
            match.EvaluateResult();

            // Invoke the analyzed callback.
            matchAnalyzedCallback?.Invoke(match);

            // Reset status
            UpdateStatus(SessionStatus.S_Success, statusChangedCallback);

        }

        private static bool CompileSession<TSessionCompilerType, TCompanyCompilerType>()
            where TSessionCompilerType : SessionCompiler<TCompanyCompilerType>
            where TCompanyCompilerType : CompanyCompiler {

            // Create compiler
            TSessionCompilerType compiler = Activator.CreateInstance<TSessionCompilerType>();

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

        private static bool HasPlayback() 
            => File.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec");

        private static void UpdateStatus(SessionStatus status, Action<SessionStatus, Session> callback) {
            SessionStatus = status;
            callback?.Invoke(SessionStatus, ActiveSession);
        }

    }

}
