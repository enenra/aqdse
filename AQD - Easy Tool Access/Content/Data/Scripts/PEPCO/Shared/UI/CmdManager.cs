using RichHudFramework.Internal;
using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VRage;
using VRage.Game;
using VRage.Game.Components;

namespace RichHudFramework.UI
{
    public interface ICommandGroup: IIndexedCollection<IChatCommand>
    {
        string Prefix { get; }

        bool TryAdd(string name, Action<string[]> callback = null, int argsRequired = 0);
        void AddCommands(CmdGroupInitializer newCommands);
    }

    public interface IChatCommand
    {
        event Action<string[]> CommandInvoked;

        string CmdName { get; }
        int ArgsRequired { get; }
    }

    public class CmdGroupInitializer : IReadOnlyList<MyTuple<string, Action<string[]>, int>>
    {
        public MyTuple<string, Action<string[]>, int> this[int index] => data[index];
        public int Count => data.Count;

        private readonly List<MyTuple<string, Action<string[]>, int>> data;

        public CmdGroupInitializer(int capacity = 0)
        {
            data = new List<MyTuple<string, Action<string[]>, int>>(capacity);
        }

        public void Add(string cmdName, Action<string[]> callback = null, int argsRequrired = 0)
        {
            data.Add(new MyTuple<string, Action<string[]>, int>(cmdName, callback, argsRequrired));
        }

        public IEnumerator<MyTuple<string, Action<string[]>, int>> GetEnumerator() =>
            data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            data.GetEnumerator();
    }

    /// <summary>
    /// Manages chat commands. Independent session component. Use only after load data.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 0)]
    public sealed class CmdManager : MySessionComponentBase
    {
        /// <summary>
        /// List of command groups registered;
        /// </summary>
        public static IReadOnlyList<ICommandGroup> CommandGroups => instance?.commandGroups;

        private static CmdManager instance;
        private readonly Regex cmdParser;
        private readonly List<CommandGroup> commandGroups;
        private readonly Dictionary<string, Command> commands;

        public CmdManager()
        {
            if (instance == null)
                instance = this;
            else
                throw new Exception("Only one instance of CmdManager can exist at any given time.");

            commandGroups = new List<CommandGroup>();
            commands = new Dictionary<string, Command>();
            cmdParser = new Regex(@"((\s*?[\s,;|]\s*?)((\w+)|("".+"")))+");
            RichHudCore.LateMessageEntered += MessageHandler;
        }

        protected override void UnloadData()
        {
            RichHudCore.LateMessageEntered -= MessageHandler;
            instance = null;
        }

        public static ICommandGroup GetOrCreateGroup(string prefix, CmdGroupInitializer groupInitializer = null)
        {
            prefix = prefix.ToLower();
            CommandGroup group = instance.commandGroups.Find(x => x.Prefix == prefix);

            if (group == null)
            {
                group = new CommandGroup(prefix);
                instance.commandGroups.Add(group);
                group.AddCommands(groupInitializer);
            }

            return group;
        }

        /// <summary>
        /// Recieves chat commands and attempts to execute them.
        /// </summary>
        private void MessageHandler(string message, ref bool sendToOthers)
        {
            message = message.ToLower();
            CommandGroup group = commandGroups.Find(x => message.StartsWith(x.Prefix));

            if (group != null)
            {
                sendToOthers = false;
                ExceptionHandler.Run(() => group.TryRunCommand(message));
            }
        }

        /// <summary>
        /// Parses list of arguments and their associated command name.
        /// </summary>
        public static bool TryParseCommand(string cmd, out string[] matches)
        {
            Match match = instance.cmdParser.Match(cmd);
            CaptureCollection captures = match.Groups[3].Captures;
            matches = new string[captures.Count];

            for (int n = 0; n < captures.Count; n++)
            {
                matches[n] = captures[n].Value;

                if (matches[n][0] == '"' && matches[n][matches[n].Length - 1] == '"')
                    matches[n] = matches[n].Substring(1, matches[n].Length - 2);
            }

            return matches.Length > 0;
        }

        private class CommandGroup : ICommandGroup
        {
            public IChatCommand this[int index] => commands[index];
            public int Count => commands.Count;
            public ICommandGroup Commands => this;
            public string Prefix { get; }

            private readonly List<Command> commands;

            public CommandGroup(string prefix)
            {
                commands = new List<Command>();
                this.Prefix = prefix;
            }

            public bool TryRunCommand(string message)
            {
                bool cmdFound = false, success = false;
                string[] matches;

                if (TryParseCommand(message, out matches))
                {
                    string cmdName = matches[0];
                    Command command;

                    if (instance.commands.TryGetValue($"{Prefix}.{cmdName}", out command))
                    {
                        string[] args = matches.GetSubarray(1);
                        cmdFound = true;

                        if (args.Length >= command.ArgsRequired)
                        {
                            command.InvokeCommand(args);
                            success = true;
                        }
                        else
                            ExceptionHandler.SendChatMessage($"Error: {cmdName} command requires at least {command.ArgsRequired} argument(s).");

                    }
                }

                if (!cmdFound)
                    ExceptionHandler.SendChatMessage("Command not recognised.");

                return success;
            }

            public bool TryAdd(string name, Action<string[]> callback = null, int argsRequired = 0)
            {
                name = name.ToLower();
                string key = $"{Prefix}.{name}";

                if (instance != null && !instance.commands.ContainsKey(key))
                {
                    Command command = new Command(name, argsRequired);
                    commands.Add(command);
                    instance.commands.Add(key, command);

                    if (callback != null)
                        command.CommandInvoked += callback;

                    return true;
                }
                else
                    return false;
            }

            public void AddCommands(CmdGroupInitializer newCommands)
            {
                for (int n = 0; n < newCommands.Count; n++)
                {
                    var cmd = newCommands[n];
                    TryAdd(cmd.Item1, cmd.Item2, cmd.Item3);
                }
            }
        }

        private class Command : IChatCommand
        {
            public event Action<string[]> CommandInvoked;
            public string CmdName { get; }
            public int ArgsRequired { get; }

            public Command(string cmdName, int argsRequired)
            {
                CmdName = cmdName.ToLower();
                ArgsRequired = argsRequired;
            }

            public void InvokeCommand(string[] args) =>
                CommandInvoked?.Invoke(args);
        }
    }
}