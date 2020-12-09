using System;

namespace Battlegrounds.Online.Debug {
    public class PermissionDeniedException : Exception {
        public const string HOST_ONLY = "Only the host can execute this method.";
        public PermissionDeniedException(string reason) : base(reason) { }
    }
}
