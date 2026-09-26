using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrismaCore
{
    public class CmdClaimCommandResult : IConsoleConnection
    {

        public CmdClaimCommandResult() { }

        public void SendLines(List<string> _output)
        {

        }

        public void SendLine(string _text)
        {
            //throw new NotImplementedException ();
        }

        public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
        {
            // Do nothing, handled by LogBuffer internally
        }

        public void EnableLogLevel(UnityEngine.LogType _type, bool _enable)
        {
            //throw new NotImplementedException ();
        }

        public string GetDescription()
        {
            return "Fire cmds asynchronously from non-main game thread";
        }
    }
}

