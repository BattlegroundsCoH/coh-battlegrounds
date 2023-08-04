using System;

using Battlegrounds.Errors.Common;

namespace Battlegrounds.Gfx.Writers;

internal class IllegalFormatException : BattlegroundsException {
    public IllegalFormatException() {
    }

    public IllegalFormatException(string? message) : base(message) {
    }

    public IllegalFormatException(string? message, Exception? innerException) : base(message, innerException) {
    }

}
