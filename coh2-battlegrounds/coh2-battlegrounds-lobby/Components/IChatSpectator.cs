using System.Windows.Media;

namespace Battlegrounds.Lobby.Components;

public interface IChatSpectator {
    
    void SystemMessage(string message, Color red);

}
