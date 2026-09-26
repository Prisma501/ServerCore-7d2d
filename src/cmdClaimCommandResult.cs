using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServerCore
{
    public class CmdClaimCommandResult : IConsoleConnection
    {

        public CmdClaimCommandResult() { }

        public void SendLines(List<string> _output)
        {

        }

        public void SendLine(string _text)
        {
        }

        public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
        {
            // Do nothing, handled by LogBuffer internally
        }

        public void EnableLogLevel(UnityEngine.LogType _type, bool _enable)
        {
        }

        public string GetDescription()
        {
            return "Fire cmds asynchronously from non-main game thread";
        }
    }
}

