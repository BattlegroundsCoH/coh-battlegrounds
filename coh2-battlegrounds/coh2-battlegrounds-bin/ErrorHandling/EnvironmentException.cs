using System;

namespace Battlegrounds.ErrorHandling {

    public class EnvironmentException : Exception {
        public EnvironmentException(string message) : base(message) { }
    }

}
