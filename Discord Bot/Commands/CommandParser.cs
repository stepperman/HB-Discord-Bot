using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class CommandPart
    {
        public string Value { get; }
        public int Index { get; }

        internal CommandPart(string value, int index)
        {
            Value = value;
            Index = index;
        }
    }

    public static class CommandParser
    {

        public static bool ParseArgs(string input, out CommandPart[] args)
        {
            string ignored;
            return Parse(input, out ignored, out args);
        }

        private static bool Parse(string input, out string command, out CommandPart[] args, bool ParseCommand = false)
        {
            int startPos = 0;
            int endPos = 0;
            int inputLength = input.Length;
            List<CommandPart> argList = new List<CommandPart>();

            command = null;
            args = null;

            if (input == "")
                return false;

            bool parseCommand = ParseCommand;

            while(endPos < inputLength)
            {
                char currentChar = input[endPos++];

                switch(parseCommand)
                {
                    case true:
                        if (currentChar == ' ' || endPos == inputLength)
                        {
                            int length = (currentChar == ' ' ? endPos - 1 : endPos) - startPos;
                            string temp = input.Substring(startPos, length);
                             if (temp != "")
                            {
                                command = temp;
                                parseCommand = false;
                            }

                            startPos = endPos;
                        }
                        break;
                    case false:
                        if(currentChar == ' ' || endPos == inputLength)
                        {
                            int length = (currentChar == ' ' ? endPos - 1 : endPos) - startPos;
                            string temp = input.Substring(startPos, length);

                            if (temp != "")
                                argList.Add(new CommandPart(temp, startPos));
                            startPos = endPos;

                        }
                        break;
                }

            }

            if (parseCommand && (command == null || command == String.Empty))
                return false;

            args = argList.ToArray();
            return true;
        }
    }
}
