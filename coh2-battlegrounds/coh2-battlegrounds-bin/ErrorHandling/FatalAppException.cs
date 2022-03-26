using System;
using System.Diagnostics;

namespace Battlegrounds.ErrorHandling;

/// <summary>
/// 
/// </summary>
public class FatalAppException : Exception {

    /// <summary>
    /// 
    /// </summary>
    public FatalAppException() : base("") {
        Trace.WriteLine("Fatal app error detected and application will now close.", nameof(FatalAppException));
        Environment.Exit(0);
    }

}
