using System;
using System.IO;

using Battlegrounds.Compiler.Source;
using Battlegrounds.Game.Match;

namespace Battlegrounds.Compiler.Wincondition.CoH3;

/// <summary>
/// 
/// </summary>
public class CoH3WinconditionCompiler : IWinconditionCompiler {

    private readonly string workDirectory;
    private readonly LocaleCompiler localeCompiler;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="workDirectory"></param>
    /// <param name="localeCompiler"></param>
    public CoH3WinconditionCompiler(string workDirectory, LocaleCompiler localeCompiler) {
        this.workDirectory = workDirectory;
        this.localeCompiler = localeCompiler;
    }

    /// <inheritdoc/>
    public bool CompileToSga(string sessionFile, ISession session, IWinconditionSourceProvider source, params WinconditionSourceFile[] includeFiles) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public string GetArchivePath() {
        string dirpath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\Company of Heroes 3\\mods\\extension\\subscriptions\\battlegrounds";
        if (!Directory.Exists(dirpath)) {
            Directory.CreateDirectory(dirpath);
        }
        return dirpath + "\\coh3_battlegrounds_wincondition.sga";
    }

}
