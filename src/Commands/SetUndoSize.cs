using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class SetUndoSize : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Set the size of history on bundo";
        }

        public override string getHelp()
        {
            return "Set the size of history on bundo" +
            "Usage:\n" +
            "   1. setbundosize <size> \n" +
            "   2. setbundosize\n" +
            "1. Sets the bundo History Size\n" +
            "2. Gets the bundo History Size\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-setbundosize", "setbundosize" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 0)
                {
                    SdtdConsole.Instance.Output("BUndo History Size is " + PrismaCoreSettings.Instance.Bundo_HistorySize);
                    return;
                }

                int size = int.MinValue;

                int.TryParse(_params[0], out size);

                if (size == int.MinValue || size <= 0)
                {
                    SdtdConsole.Instance.Output("ERR: Invalid bundo history size. It must be greater than 0.");
                    return;
                }
                PrismaCoreSettings.Instance.Bundo_HistorySize = size;
                PrismaCoreSettings.Instance.Save();
                SdtdConsole.Instance.Output("BUndo History Size set to " + _params[0]);
            }
            catch (Exception e)
            {
                Log.Out("Error in SetUndoSize: " + e);
            }
        }
    }
}
