using System;

namespace Battlegrounds.Developer.Commands;

public class ReplCommand : Command {

    public ReplCommand() : base("repl", "The program will enter a repl mode and allow for various inputs.") { }

    public override void Execute(CommandArgumentList argumentList) {
        Console.Clear();
        Console.WriteLine("====== Entered REPL Mode ======");
        var defcolour = Console.ForegroundColor;
        while (true) {
            Console.WriteLine();
            Console.Write(">> ");

            Console.ForegroundColor = ConsoleColor.Green;
            string[] input = (Console.ReadLine() ?? string.Empty).Split(' ');
            Console.ForegroundColor = defcolour;

            if (input.Length is 1) {
                if (input[0] is "exit")
                    break;
                else if (input[0] is "clear") {
                    Console.Clear();
                    continue;
                }
            }

            // Parse
            Program.flags?.Parse(input);

        }
        Console.WriteLine("====== Exited REPL Mode ======");
    }

}
