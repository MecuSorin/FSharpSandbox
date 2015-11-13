using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSL_Sprache
{
    public class Parser : Command
    {
        public static readonly Parser<IEnumerable<Command>> Get = ListNotEmpty(CommandRepeat.GetCommandParser(), '[', ']', ';').End();
    }


    public abstract class Command
    {
        protected static Parser<T> EnumeratedButNotLast<T>(Parser<T> enumeratedItem, char delimiter)
        {
            return from item in enumeratedItem
                   from spaces in Parse.WhiteSpace.Many()
                   from _ in Parse.Char(delimiter)
                   select item;
        }

        private static IEnumerable<T> ComposeList<T>(IOption<IEnumerable<T>> items, T lastItem)
        {
            List<T> result;
            if (items.IsDefined && !items.IsEmpty)
                result = new List<T>(items.Get());
            else
                result = new List<T>();
            result.Add(lastItem);
            return result;
        }

        protected static Parser<IEnumerable<T>> NotEmptyEnumeration<T>(Parser<T> parser, char delimiter)
        {
            return from item in EnumeratedButNotLast(parser, delimiter).Many().Optional()
                   from spaces in Parse.WhiteSpace.Many()
                   from last in parser
                   select ComposeList(item, last);
        }

        protected static Parser<T> Brackets<T>(Parser<T> contentParser, char start, char end)
        {
            return from open in Parse.Char(start)
                   from space1 in Parse.WhiteSpace.Many()
                   from content in contentParser
                   from space2 in Parse.WhiteSpace.Many()
                   from close in Parse.Char(end)
                   select content;
        }

        protected static Parser<IOption<IEnumerable<T>>> ListThatCanBeEmpty<T>(Parser<T> parser, char start, char end, char delimiter )
        {
            return Brackets(NotEmptyEnumeration(parser, delimiter).Optional(), start, end);
        }

        protected static Parser<IEnumerable<T>> ListNotEmpty<T>(Parser<T> parser, char start, char end, char delimiter)
        {
            return Brackets(NotEmptyEnumeration(parser, delimiter), start, end);
        }
    }

    class CommandForward : Command
    {
        public double Distance { get; set; }

        public static readonly Parser<Command> Parser =
            from cmd in Parse.IgnoreCase("forward").Token()
            from distance in Parse.Decimal
            select new CommandForward { Distance = double.Parse(distance) };
    }

    class CommandTurn : Command
    {
        public double AngleInDegrees { get; set; }

        public static readonly Parser<Command> Parser =
            from cmd in Parse.IgnoreCase("turn").Token()
            from angle in Parse.Decimal
            select new CommandTurn { AngleInDegrees = double.Parse(angle) };
    }

    class CommandMomentum : Command
    {
        public double Ponder { get; set; }
        public static readonly Parser<Command> Parser =
            from cmd in Parse.IgnoreCase("momentum").Token()
            from ponder in Parse.Decimal
            select new CommandMomentum { Ponder = double.Parse(ponder) };
    }

    class CommandGo : Command
    {
        public static readonly Parser<Command> Parser =
               from cmd in Parse.IgnoreCase("go").Token()
               select new CommandGo();
    }

    class CommandRepeat : Command
    {
        private readonly static List<Parser<Command>> _commandParsers = new List<Parser<Command>>();
        public static Parser<Command> GetCommandParser()
        {
            var result = _commandParsers[0];
            foreach (var command in _commandParsers.Skip(1))
                result = result.Or(command);
            return result;
        }

        public static readonly Parser<Command> Parser =
           from cmd in Parse.IgnoreCase("repeat").Token()
           from a in Brackets(
               from repetitions in Parse.Number.Token()
               from comma in Parse.Char(',').Token()
               from commands in ListThatCanBeEmpty(GetCommandParser(), '[', ']', ';')
               select new CommandRepeat(int.Parse(repetitions), commands), '(', ')')
           select a;

        static CommandRepeat()
        {
            _commandParsers.Add(CommandForward.Parser);
            _commandParsers.Add(CommandTurn.Parser);
            _commandParsers.Add(CommandMomentum.Parser);
            _commandParsers.Add(CommandGo.Parser);
            _commandParsers.Add(CommandRepeat.Parser);  

        }
        public CommandRepeat(int numberOfTimes, IOption<IEnumerable<Command>> commandsToRepeat)
        {
            if (numberOfTimes < 1)
                throw new ArgumentOutOfRangeException("Number of repeats must be positive");
            NumberOfTimes = numberOfTimes;
            if (commandsToRepeat.IsDefined && !commandsToRepeat.IsEmpty)
                CommandsToRepeat = new List<Command>(commandsToRepeat.Get());
            else
                CommandsToRepeat = new Command[0];
        }

        public int NumberOfTimes { get; private set; }
        public IEnumerable<Command> CommandsToRepeat { get; private set; }
    }
}
