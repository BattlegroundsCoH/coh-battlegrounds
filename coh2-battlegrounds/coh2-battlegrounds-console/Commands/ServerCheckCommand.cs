using Battlegrounds.Networking;

using System;
using System.Diagnostics;

namespace Battlegrounds.Developer.Commands;

public class ServerCheckCommand : Command {

    public ServerCheckCommand() : base("server-check", "Will send a ping request to the server and time the response time.") { }

    public override void Execute(CommandArgumentList argumentList) {
        var watch = Stopwatch.StartNew();
        var response = NetworkInterface.RemoteReleaseEndpoint.IsConnectable();
        watch.Stop();
        Console.WriteLine($"Server response: {response} - ping: {watch.ElapsedMilliseconds}ms");
    }

}
