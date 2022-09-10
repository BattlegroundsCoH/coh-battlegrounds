using System;
using System.Collections.Generic;

using Battlegrounds.Functional;

namespace coh2_battlegrounds_console;

public abstract class Argument {
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public abstract string ArgType { get; }
    public abstract object? GetDefault();
}

public class Argument<T> : Argument {
    public override string ArgType => typeof(T).Name;
    public T? DefaultValue { get; }
    public Argument(string name, string description, T? defaultValue) {
        this.Name = name;
        this.Description = description;
        this.DefaultValue = defaultValue;
    }
    public override object? GetDefault() => this.DefaultValue;
}

public class CommandArgumentList {
    private readonly Dictionary<string, object?> m_args;
    public CommandArgumentList(Dictionary<string, object?> args) {
        this.m_args = args;
    }
    public T GetValue<T>(string arg)
        => this.m_args.TryGetValue(arg, out object? argval) ? (argval is T t ? t : throw new Exception("Invalid type argument.")) : throw new Exception("Invalid argument name.");
    public T GetValue<T>(Argument arg) {
        if (arg is Argument<T>) {
            if (this.m_args.TryGetValue(arg.Name, out object? v) && v is T argv) {
                return argv;
            }
            throw new Exception("Invalid argument name.");
        }
        throw new Exception("Invalid argument type");
    }
    public T GetValue<T>(Argument<T> arg) => this.GetValue<T>(arg as Argument);
}

public abstract class Command {

    public string Name { get; }

    public string Description { get; }

    public Argument[] Arguments { get; init; }

    public Command(string name, string desc, params Argument[] arguments) {
        this.Name = name;
        this.Description = desc;
        this.Arguments = arguments;
    }

    public Argument? GetArgument(string name) {
        int i = this.Arguments.IndexOf(x => x.Name == name);
        if (i != -1)
            return this.Arguments[i];
        else
            return null;
    }

    public abstract void Execute(CommandArgumentList argumentList);

}
