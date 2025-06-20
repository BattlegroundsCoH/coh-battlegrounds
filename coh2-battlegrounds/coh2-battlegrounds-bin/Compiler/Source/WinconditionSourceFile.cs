﻿using Battlegrounds.Compiler.Wincondition.CoH2;

namespace Battlegrounds.Compiler.Source;

/// <summary>
/// A source file for the <see cref="CoH2WinconditionCompiler"/> containing a path and binary file contents.
/// </summary>
/// <param name="Path">The path for use in the wincondition (Not where the source is from).</param>
/// <param name="Contents">The byte contents loaded from the source.</param>
public record WinconditionSourceFile(string Path, byte[] Contents);
