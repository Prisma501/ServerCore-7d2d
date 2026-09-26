using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace ServerCore.CustomCommands
{
    public class ResetRegionsNow : ConsoleCmdAbstract
    {
        private static Thread thKillServer;

        public override string getDescription()
        {
            return "Reset marked resetregions.";
        }

        public override string getHelp()
        {
            return "Usage: resetregions [kicklockreboot]";
        }

        public override string[] getCommands()
        {
            return new[] { "pc-resetregions", "resetregions" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            GameManager.Instance.StartCoroutine(this.execute(_params, _senderInfo));
        }

        public IEnumerator execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                SdtdConsole.Instance.Output("[PrismaCore] Regions marked for reset are going to be reset now!!!");

                if (_params.Count == 1 && _params[0].Trim().ToLower().Equals("kicklockreboot"))
                {
                    KickAll();

                    Reset.resetActive = true;

                    RegionReset.RR();

                    thKillServer = new Thread(new ThreadStart(KillServer));
                    thKillServer.IsBackground = true;
                    thKillServer.Start();
                    yield break;
                }

                RegionReset.RR();

                yield break;

            }
            catch (Exception e)
            {
                Log.Out(string.Format("[PrismaCore] Error in ResetRegionsNow.Run: {0}.", e));
            }
        }

        private void KickAll()
        {
            ReadOnlyCollection<ClientInfo> list = ConnectionManager.Instance.Clients.List;
            for (int i = 0; i < list.Count; i++)
            {
                ClientInfo clientInfo = list[i];
                GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.ManualKick, 0, default(DateTime), ServerCoreStrings.Instance.ResetPrefabs_KickMessage));
            }

            int fs = 0;
            while (ConnectionManager.Instance.ClientCount() > 0)
            {
                Thread.Sleep(1000);
                fs += 1;
                if (fs == 5)
                {
                    break;
                }
            }
        }
        private static void KillServer()
        {
            Application.Quit();

            Thread.Sleep(30000);

            Process proc = Process.GetCurrentProcess();
            if (proc != null)
            {
                proc.Kill();
            }
        }
    }
}