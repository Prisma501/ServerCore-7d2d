using System;
using System.Collections.Generic;

namespace PrismaCore.CustomCommands
{
    public class GetBikeCommand : ConsoleCmdAbstract
    {
        public override string getDescription()
        {
            return "Get lost or stuck minibike to player";
        }

        public override string getHelp()
        {
            return "Get minibike Usage:\n   getbike <steam id/player name/entity id>\n";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-getbike", "getbike" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    SdtdConsole.Instance.Output("Usage: getbike <entityid|playername|steamid>");
                }
                else if (_params.Count == 1)
                {
                    ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0], true, false);
                    if (clientInfo == null)
                    {
                        SdtdConsole.Instance.Output("ERR: Playername or entity/steamid id not found.");
                    }
                    else
                    {
                        if (_senderInfo.RemoteClientInfo == null || _senderInfo.RemoteClientInfo.PlatformId == null)
                        {
                            _senderInfo.RemoteClientInfo = clientInfo;
                        }
                        if (_senderInfo.RemoteClientInfo == clientInfo || GameManager.Instance.adminTools.Users.HasEntry(_senderInfo.RemoteClientInfo))
                        {
                            Getbike.Findbikes(clientInfo);
                        }
                        else
                        {
                            SdtdConsole.Instance.Output("ERR: You are not the owner or admin.");
                        }
                    }
                }
                else if (_params.Count == 0)
                {
                    SdtdConsole.Instance.Output("Usage: getbike <entityid|playername|steamid>");
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in getbike Command: " + e);
            }
        }
    }
}
