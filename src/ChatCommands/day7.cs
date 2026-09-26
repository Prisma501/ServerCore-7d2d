using System;
using System.Collections.Generic;

namespace PrismaCore
{
    class day7
    {
        public static bool Exec(ClientInfo _cInfo)
        {
            try
            {
                int AdminLvL = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_cInfo);

                if (AdminLvL <= PrismaCoreSettings.Instance.ChatCommandPermissions_day7)
                {
                    int days = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
                    int hours = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
                    int minutes = GameUtils.WorldTimeToMinutes(GameManager.Instance.World.worldTime);

                    int BMcycle = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonFrequency"));
                    int BMrange = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("BloodMoonRange"));

                    if (BMcycle > 0)
                    {
                        int remainder;
                        int q = Math.DivRem(days, BMcycle, out remainder);
                        string PMmsg = "";
                        if (remainder == 0)
                        {
                            //if remainder=0 but hours < 4 report 22:00
                            if (hours < 4) PMmsg = PrismaCoreStrings.Instance.Day7_BloodmoonWarningB4Four;
                            else PMmsg = PrismaCoreStrings.Instance.Day7_BloodmoonWarningAfterFour;
                        }
                        else if (remainder == 1 && hours < 4) PMmsg = PrismaCoreStrings.Instance.Day7_BloodmoonWarningDuring;
                        else
                        {
                            int daysleft = BMcycle - remainder;
                            int nexthorde = (q * BMcycle) + remainder + daysleft;
                            string strDay = "";
                            if (daysleft == 1) strDay = PrismaCoreStrings.Instance.Day7_Day;
                            else strDay = PrismaCoreStrings.Instance.Day7_Days;
                            PMmsg = PrismaCoreStrings.Instance.Day7_BloodmoonWarningDaysleft;
                            // "Bloodmoon is in {daysLeft} {textDay} !  ( Day {nextBloodmoonDay} )"
                            PMmsg = PMmsg.Replace("{daysLeft}", daysleft.ToString());
                            PMmsg = PMmsg.Replace("{textDay}", strDay);
                            PMmsg = PMmsg.Replace("{nextBloodmoonDay}", nexthorde.ToString());
                        }

                        if (BMrange > 0)
                        {
                            PMmsg = PrismaCoreStrings.Instance.Day7_Random;
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        }
                        else
                        {
                            _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, PMmsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        }
                    }

                    int enemycount = 0;

                    try
                    {
                        List<Entity> entities = GameManager.Instance.World.Entities.list;
                        for (int i = 0; i < entities.Count; i++)
                        {
                            Entity entity = entities[i];

                            if (entity is EntityEnemy && entity.IsAlive())
                            {
                                enemycount++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Exception(e);
                    }

                    World w = GameManager.Instance.World;
                    int players = w.Players.dict.Count;

                    string fps = GameManager.Instance.fps.Counter.ToCultureInvariantString("F1");

                    string StatMsg = PrismaCoreStrings.Instance.Day7_Stats.Replace("{players}", players.ToString());
                    StatMsg = StatMsg.Replace("{zombies}", enemycount.ToString());

                    string FpsMsg = PrismaCoreStrings.Instance.Day7_Fps.Replace("{fps}", fps);

                    if (!StatMsg.ToLower().Equals("off"))
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "* " + StatMsg, null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));

                    if (!FpsMsg.ToLower().Equals("off"))
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, "* " + FpsMsg, null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
                else
                {
                    string errMsg = PrismaCoreStrings.Instance.ChatCommandPermissions_NotAllowedMessage;
                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>().Setup(EChatType.Whisper, -1, Utils.CreateGameMessage(PrismaCoreStrings.Instance.ServerChatName, errMsg), null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                }
            }
            catch { return false; }
            return true;
        }
    }
}
