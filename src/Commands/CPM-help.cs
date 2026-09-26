using System.Collections.Generic;
using System.Text;

namespace ServerCore.CustomCommands
{
    public class ConsoleCmdPrismaCoreHelp : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[]
            {
                "pc-help"
            };
        }

        public override string getDescription()
        {
            return "Help on console and specific Prisma Core Mod commands";
        }

        public override string getHelp()
        {
            return "Type \"pc-help\" for an overview of PrismaCore Mod commands ";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (_params.Count == 0)
            {
                SdtdConsole.Instance.Output("*** Generic Console Help ***");
                SdtdConsole.Instance.Output(
                    "To get further help on a specific topic or command type (without the brackets)");
                SdtdConsole.Instance.Output("    help <topic / command>");
                SdtdConsole.Instance.Output(string.Empty);
                SdtdConsole.Instance.Output("*** List of PrismaCore Mod Commands ***");
                for (int i = 0; i < SdtdConsole.Instance.GetCommands().Count; i++)
                {
                    IConsoleCommand consoleCommand = SdtdConsole.Instance.GetCommands()[i];
                    if (consoleCommand.GetCommands()[0].StartsWith("pc-"))
                    {
                        foreach (string value in consoleCommand.GetCommands())
                        {
                            stringBuilder.Append(" ");
                            stringBuilder.Append(value);
                        }

                        stringBuilder.Append(" => ");
                        stringBuilder.Append(consoleCommand.GetDescription());
                        SdtdConsole.Instance.Output(stringBuilder.ToString());
                        stringBuilder.Length = 0;
                    }
                }
            }
        }
    }
}