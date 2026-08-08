using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Battlegrounds.ViewModels.LobbyHelpers;

namespace Battlegrounds.Converters;

/// <summary>
/// Resolves a <see cref="ChatTone"/> to a themed brush.
/// </summary>
/// <remarks>
/// Every tone maps onto a role that already exists — no chat-specific brushes were added, since
/// a new brush belongs to a new role, not to a view wanting a particular shade.
/// </remarks>
public sealed class ChatToneBrushConverter : IValueConverter {

    private static readonly Dictionary<ChatTone, string> ToneBrushKeys = new() {
        [ChatTone.ChannelAll] = "Brush.Accent.Gold",
        [ChatTone.ChannelTeam] = "Brush.Status.Info",
        [ChatTone.Self] = "Brush.Status.Info",
        [ChatTone.Ally] = "Brush.Status.Caution",
        [ChatTone.Enemy] = "Brush.Status.Danger",
        [ChatTone.SystemInfo] = "Brush.Text.Muted",
        [ChatTone.SystemWarning] = "Brush.Status.Caution",
        [ChatTone.SystemError] = "Brush.Status.DangerText"
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not ChatTone tone || !ToneBrushKeys.TryGetValue(tone, out var key)) {
            return DependencyProperty.UnsetValue;
        }
        return Application.Current?.TryFindResource(key) as Brush ?? (object)DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

}
