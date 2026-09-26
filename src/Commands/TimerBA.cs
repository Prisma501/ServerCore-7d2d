using System;
using System.Collections.Generic;
using System.Threading;

namespace PrismaCore.CustomCommands
{
    public class TimerBA : ConsoleCmdAbstract
    {
        private static Thread thCountdown;
        private static int minutes;
        private static string countdownMsg = string.Empty;
        private static string command = string.Empty;
        private static bool countdRunning = false;
        private static int delayedOnDay = 0;

        public override string getDescription()
        {
            return "Bloodboon aware generic timer to run commands.";
        }

        public override string getHelp()
        {
            return "Usage: timerba <minutes> <countdownMessage> <command(s)>\n" +
                   "timerba stop\n" +
                   "   Use {Minutes} in <countdownMessage> for showing the number of minutes left in the message.\n" +
                   "   For multiple commands use ; as seperator. For commands that have parameters with spaces enclose them in single quotes";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-timerba", "timerba" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1 && _params.Count != 3)
                {
                    SdtdConsole.Instance.Output("ERR: Invalid number of parameters");
                    return;
                }
                else if (_params.Count == 1)
                {
                    if (_params[0] == "stop")
                    {
                        if (!countdRunning)
                        {
                            SdtdConsole.Instance.Output("ERR: timer is not active.");
                        }
                        else
                        {
                            thCountdown.Abort();
                            countdRunning = false;
                            SdtdConsole.Instance.Output("[PrismaCore] timer was manually interrupted.");
                        }
                    }
                }
                else if (_params.Count == 3)
                {
                    if (countdRunning)
                    {
                        SdtdConsole.Instance.Output($"ERR: Timer is already running. Time left: {minutes} minutes");
                    }
                    else
                    {
                        if (!int.TryParse(_params[0], out minutes))
                        {
                            SdtdConsole.Instance.Output($"ERR: Invalid number of minutes specified: {minutes}");
                            return;
                        }
                        else
                        {
                            countdownMsg = _params[1];
                            command = _params[2];
                            Start();
                        }
                    }
                }
                else
                {
                    //wrong number of params
                    SdtdConsole.Instance.Output(string.Format("ERR: Invalid number of parameters specified: {0}", _params.Count));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in timerba.Run: {0}.", e));
            }
        }

        private static void Start()
        {
            thCountdown = new Thread(new ThreadStart(Countdown));
            thCountdown.IsBackground = true;
            thCountdown.Start();
        }

        private static void Countdown()
        {
            countdRunning = true;

            if (minutes > 0)
            {
                //check for bm tresholds
                int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));

                if (BMcycle > 0)
                {
                    delayedOnDay = 0;
                    if (MustDelay())
                    {
                        while (MustDelay())
                        {
                            Thread.Sleep(10000);
                        }
                    }
                }

                string t = countdownMsg;

                while (minutes != 1)
                {
                    t = t.Replace("{Minutes}", minutes.ToString());
                    GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, string.Format("{0}", t)), null, EMessageSender.None);
                    int ov = minutes;
                    minutes = minutes - 1;
                    t = t.Replace(ov.ToString(), minutes.ToString());
                    Thread.Sleep(60000);
                }

                t = t.Replace("{Minutes}", minutes.ToString());
                GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, string.Format("{0}", t)), null, EMessageSender.None);
                Thread.Sleep(60000);
            }

            if (command.Contains(";"))
            {
                //multiple commands
                string[] arrCommands = command.Split(';');
                CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                foreach (string s in arrCommands)
                {
                    string cmd = s;
                    cmd = cmd.Replace("'", "\"");
                    SdtdConsole.Instance.ExecuteAsync(cmd, iConsole);
                }
            }
            else
            {
                //just 1 command
                command = command.Replace("'", "\"");
                CmdClaimCommandResult iConsole = new CmdClaimCommandResult();
                SdtdConsole.Instance.ExecuteAsync(command, iConsole);
            }
        }

        private static bool MustDelay()
        {
            int days = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
            int hours = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);

            int remainder;

            int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));
            int BMrange = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonRange"));


            if (BMrange > 0)
            {
                //random bm handling
                //= days equal to Bloodmoonday? -> bloodmoonday :P

                int num = (int)SkyManager.dayCount;
                int bmDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);

                bool bmActive = (num == bmDay && SkyManager.TimeOfDay() >= 22f) || (num > 1 && num == bmDay + 1 && SkyManager.TimeOfDay() <= 4f);

                if (days == bmDay && hours >= PrismaCoreSettings.Instance.TimerBA_DelayBloodDayAfter)
                {
                    delayedOnDay = days;
                    return true;
                }

                if (bmDay == days - 1 && bmActive)
                {
                    delayedOnDay = days - 1;
                    return true;
                }

                if (bmDay > days && !bmActive && delayedOnDay == days - 1 && hours < PrismaCoreSettings.Instance.TimerBA_DelayAfterBloodmoonUntil)
                {
                    return true;
                }
            }
            else
            {
                int q = Math.DivRem(days, BMcycle, out remainder);

                if (days == 1)
                {
                    return false;
                }

                if (remainder == 0)
                {
                    //bloodday!! check for time that should delay shutdown
                    if (hours >= PrismaCoreSettings.Instance.TimerBA_DelayBloodDayAfter && BMrange == 0)
                    {
                        //its past delay time -> return true
                        return true;
                    }
                }
                else if (remainder == 1 && hours < 4 && BMrange == 0)
                {
                    //bloodmoon is ongoing -> do delay
                    return true;
                }
                else if (remainder == 1 && hours < PrismaCoreSettings.Instance.TimerBA_DelayAfterBloodmoonUntil && BMrange == 0)
                {
                    //bloodmoon over -> delay until time is reached
                    return true;
                }
            }

            return false;
        }
    }
}