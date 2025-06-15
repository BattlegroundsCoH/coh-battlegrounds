using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Developer;

public class Flags {

    private readonly Dictionary<string, Command> m_commands = new();

    private static string[] Sanitize(string[] input) {
        StringBuilder sb = new StringBuilder();
        List<string> res = new();
        bool isInString = false;
        for (int i = 0; i < input.Length; i++) {
            if (isInString) {
                sb.Append(input[i]);
                if (input[i].EndsWith('\"')) {
                    res.Add(sb.ToString().Trim('\"'));
                    isInString = false;
                } else {
                    sb.Append(' ');
                }
            } else {
                if (input[i].StartsWith('\"')) {
                    sb.Append(input[i]);
                    sb.Append(' ');
                    isInString = true;
                } else {
                    res.Add(input[i]);
                }
            }
        }
        return res.ToArray();
    }

    public void Parse(params string[] args) {

        if (args.Length is 0) {
            this.HelpAll();
            return;
        }

        // Sanitise
        args = Sanitize(args);

        Dictionary<string, object?> cmdargs = new();
        string? current = null;

        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "&") {
                if (!string.IsNullOrEmpty(current)) {
                    this.Dispatch(current, cmdargs);
                }
                current = null;
                cmdargs.Clear();
            } else if (string.IsNullOrEmpty(current)) {
                current = args[i];
                if (!this.m_commands.ContainsKey(current)) {
                    Console.WriteLine("Unknown command: " + current);
                    Console.WriteLine();
                    this.HelpAll();
                    return;
                }
            } else {
                if (this.m_commands[current].GetArgument(args[i]) is Argument arg) {
                    cmdargs[arg.Name] = this.ParseArg(arg.ArgType, i + 1 < args.Length ? args[i + 1] : null, arg.GetDefault());
                    i++; // Skip next parameter
                } else {
                    if (this.m_commands[current].Arguments.Length == 1) {
                        var defarg = this.m_commands[current].Arguments[0];
                        if (defarg.ArgType == "Boolean") {
                            this.HelpCmd(this.m_commands[current]);
                            return;
                        }
                        cmdargs[defarg.Name] = this.ParseArg(defarg.ArgType, args[i], defarg.GetDefault());
                    } else {
                        this.HelpCmd(this.m_commands[current]);
                        return;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(current))
            this.Dispatch(current, cmdargs);

    }

    private object ParseArg(string argty, string? next, object? def) => argty switch {
        "String" => next ?? throw new Exception(),
        "Boolean" => !(def is bool b ? b : throw new Exception()),
        "Int32" => int.TryParse(next, out int i) ? i : throw new Exception(),
        _ => throw new NotSupportedException()
    };

    private void HelpAll() {
        Console.WriteLine("Battlegrounds-Console usage:");
        Console.WriteLine();
        foreach (var cmd in this.m_commands) {
            this.HelpCmd(cmd.Value);
        }
    }

    private void HelpCmd(Command cmd) {
        Console.WriteLine($"{cmd.Name}: {cmd.Description}");
        for (int i = 0; i < cmd.Arguments.Length; i++) {
            StringBuilder line = new();
            line.Append($"\t{cmd.Arguments[i].Name}: {cmd.Arguments[i].Description}");
            if (cmd.Arguments[i].ArgType != "Boolean") {
                line.Append($" (Default = '{cmd.Arguments[i].GetDefault()}')");
            }
            Console.WriteLine(line);
        }
        Console.WriteLine();
    }

    private void Dispatch(string cmd, Dictionary<string, object?> args) {
        var cmdobj = this.m_commands[cmd];
        foreach (var arg in cmdobj.Arguments) { 
            if (!args.ContainsKey(arg.Name)) {
                args[arg.Name] = arg.GetDefault();
            }
        }
        cmdobj.Execute(new(args));
    }

    public void RegisterCommand<T>() where T : Command
        => this.RegisterCommand(Activator.CreateInstance<T>());

    public void RegisterCommand(Command cmd)
        => this.m_commands[cmd.Name] = cmd;

}
