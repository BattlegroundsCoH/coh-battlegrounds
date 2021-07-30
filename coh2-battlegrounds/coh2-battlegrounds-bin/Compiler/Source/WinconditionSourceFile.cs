namespace Battlegrounds.Compiler.Source {

    /// <summary>
    /// A source file for the <see cref="WinconditionCompiler"/> containing a path and binary file contents.
    /// </summary>
    /// <param name="path">The path for use in the wincondition (Not where the source is from).</param>
    /// <param name="contents">The byte contents loaded from the source.</param>
    public record WinconditionSourceFile(string path, byte[] contents);

}
