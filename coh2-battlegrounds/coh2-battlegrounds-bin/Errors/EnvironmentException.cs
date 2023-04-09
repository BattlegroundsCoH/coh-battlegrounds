using System;

namespace Battlegrounds.Errors; 
public class EnvironmentException : Exception {
    public EnvironmentException(string message) : base(message) { }
}
