using System;
using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class AnnounceNightTime : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Turn NightTime announcement on/off";
        }
        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. an true/false\n" +
                   "  2. an warnhours <hours>\n" +
                   "  3. an announcer <name>\n" +
                   "  4. an nighttimetext <string>\n" +
                   "  5. an blooddaytext <string>\n" +
                   "  6. an blooddaytomorrowtext <string>\n" +
                   "  7. an counterdaytext <string>\n" +
                   "  8. an\n" +
                   "1. Turn AnnounceNightTime ON (true)/OFF(false)\n" +
                   "2. Set the number of <hours> before 22:00 warning\n" +
                   "3. Set the name of the announcer\n" +
                   "4. Set the text for nighttime warning. {hours} will be replaced by the warnhours setting\n" +
                   "5. Set the text for announcing bloodmoon is tonight\n" +
                   "6. Set the text for announcing bloodmoon is tomorrow\n" +
                   "7. Set the text for normal bloodmoon counter text. {daysleft} will be replaced by remaining days\n" +
                   "8. List active AnnounceNightTime settings\n" +
                   "All text and announcer name support (nested) color coding. The value of <string> has to be wrapped in \"double quotes\".\n" +
                   "Put text between [colorCode]text here[-] for giving it a color.";
        }
        public override string[] getCommands()
        {
            return new[] { "pc-announcenighttime", "announcenighttime", "an" };
        }
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 1)
                {
                    if (!bool.TryParse(_params[0], out bool enabled))
                    {
                        SdtdConsole.Instance.Output(string.Format("ERR: The parameter is not true/false."));
                        return;
                    }

                    ServerCoreSettings.Instance.NighttimeAnnouncer_Enabled = enabled;
                    ServerCoreSettings.Instance.Save();

                    if (enabled)
                        SdtdConsole.Instance.Output(string.Format("NightTimeAnnouncement has been turned ON."));
                    else
                        SdtdConsole.Instance.Output(string.Format("NightTimeAnnouncement has been turned OFF."));
                }
                else if (_params.Count == 2)
                {
                    if (_params[0].Trim().ToLower() == "warnhours")
                    {
                        if (!int.TryParse(_params[1], out int hours))
                        {
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for warnhours is not a valid integer."));
                            return;
                        }

                        ServerCoreSettings.Instance.NighttimeAnnouncer_Warnhours = hours;
                        ServerCoreSettings.Instance.Save();

                        SdtdConsole.Instance.Output(string.Format("WarnHours has been set to {0} hours", hours));
                    }
                    else if (_params[0].Trim().ToLower() == "announcer")
                    {
                        string announcer = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for announcer is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            announcer = _params[1].Trim();
                            ServerCoreStrings.Instance.NighttimeAnnouncer_AnnouncerName = announcer;
                            ServerCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("Announcer name has been set to \"{0}\"", announcer));
                        }
                    }
                    else if (_params[0].Trim().ToLower() == "nighttimetext")
                    {
                        string text = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for nighttimetext is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            text = _params[1].Trim();
                            ServerCoreStrings.Instance.NighttimeAnnouncer_NightTimeText = text;
                            ServerCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("Text for NightTime has been set to \"{0}\"", text));
                        }
                    }
                    else if (_params[0].Trim().ToLower() == "blooddaytext")
                    {
                        string text = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for blooddaytext is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            text = _params[1].Trim();
                            ServerCoreStrings.Instance.NighttimeAnnouncer_BloodDayText = text;
                            ServerCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("Text for BloodDay has been set to \"{0}\"", text));
                        }
                    }
                    else if (_params[0].Trim().ToLower() == "blooddaytomorrowtext")
                    {
                        string text = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for blooddaytomorrowtext is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            text = _params[1].Trim();
                            ServerCoreStrings.Instance.NighttimeAnnouncer_BloodDayTomorrowText = text;
                            ServerCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("Text for BloodDayTomorrow has been set to \"{0}\"", text));
                        }
                    }
                    else if (_params[0].Trim().ToLower() == "counterdaytext")
                    {
                        string text = string.Empty;
                        if (string.IsNullOrEmpty(_params[1]))
                            SdtdConsole.Instance.Output(string.Format("ERR: The parameter for counterdaytext is not valid. Parameter: {0}", _params[1]));
                        else
                        {
                            text = _params[1].Trim();
                            ServerCoreStrings.Instance.NighttimeAnnouncer_CounterText = text;
                            ServerCoreStrings.Instance.Save();

                            SdtdConsole.Instance.Output(string.Format("Text for BloodDayCounter has been set to \"{0}\"", text));
                        }
                    }
                    else
                        SdtdConsole.Instance.Output("ERR: Subcommand not recognized: " + _params[0]);
                }
                else
                {
                    SdtdConsole.Instance.Output(string.Format("AnnounceNightTime Enabled: {0}", ServerCoreSettings.Instance.NighttimeAnnouncer_Enabled));
                    SdtdConsole.Instance.Output(string.Format("Warn {0} hours before 22:00", ServerCoreSettings.Instance.NighttimeAnnouncer_Warnhours));
                    SdtdConsole.Instance.Output(string.Format("Announcer name: {0}", ServerCoreStrings.Instance.NighttimeAnnouncer_AnnouncerName));
                    SdtdConsole.Instance.Output(string.Format("NightTime text: {0}", ServerCoreStrings.Instance.NighttimeAnnouncer_NightTimeText));
                    SdtdConsole.Instance.Output(string.Format("BloodDay text: {0}", ServerCoreStrings.Instance.NighttimeAnnouncer_BloodDayText));
                    SdtdConsole.Instance.Output(string.Format("BloodDay Tomorrow text: {0}", ServerCoreStrings.Instance.NighttimeAnnouncer_BloodDayTomorrowText));
                    SdtdConsole.Instance.Output(string.Format("CounterDay text: {0}", ServerCoreStrings.Instance.NighttimeAnnouncer_CounterText));
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in AnnounceNightTime.Run: {0}.", e));
            }
        }
    }
}
