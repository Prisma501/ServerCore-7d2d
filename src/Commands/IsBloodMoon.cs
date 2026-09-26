using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class IsBloodMoon : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-isbloodmoon", "isbloodmoon" };
        }

        public override string getDescription()
        {
            return "Returns bool that defines if bloodmoon is active or not(true/false)";
        }

        public override string getHelp()
        {
            return "Returns bool that defines if bloodmoon is active or not(true/false)\n" +
                "Usage:\n" +
                "   isbloodmoon";

        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            int bmDay = GameStats.GetInt(EnumUtils.Parse<EnumGameStats>("BloodMoonDay", false));
            bool bm =GameUtils.IsBloodMoonTime(GameManager.Instance.World.worldTime, new ValueTuple<int, int>(GameManager.Instance.World.DuskHour, GameManager.Instance.World.DawnHour),bmDay);

            SdtdConsole.Instance.Output($"{bm}");
            //Log.Out($"[PrismaCore] IsBloodMoon: {bm}");

        }
    }
}
