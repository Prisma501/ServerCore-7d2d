using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace PrismaCore.Web
{
    public class WebConnection : ConsoleConnectionAbstract
    {
        private readonly DateTime login;
       
        private DateTime lastAction;
        private readonly string conDescription;

        public WebConnection(string _sessionId, IPAddress _endpoint, PlatformUserIdentifierAbs _userId)
        {
            SessionID = _sessionId;
            Endpoint = _endpoint;
            UserId = _userId;
            login = DateTime.Now;
            lastAction = login;
            conDescription = "WebPanel from " + Endpoint;
        }

        public string SessionID { get; private set; }

        public IPAddress Endpoint { get; private set; }

        public PlatformUserIdentifierAbs UserId { get; }

        public TimeSpan Age => DateTime.Now - lastAction;

        public static bool CanViewAllPlayers(int _permissionLevel)
        {
            return WebPermissions.Instance.ModuleAllowedWithLevel("webapi.viewallplayers", _permissionLevel);
        }

        public static bool CanViewAllClaims(int _permissionLevel)
        {
            return WebPermissions.Instance.ModuleAllowedWithLevel("webapi.viewallclaims", _permissionLevel);
        }

        public void UpdateUsage()
        {
            lastAction = DateTime.Now;
        }

        public override void SendLine(string _text)
        {
        }

        public override void SendLines(List<string> _output)
        {
        }
        public override void SendLog(string _formattedMsg, string _plainMsg, string _trace, LogType _type, DateTime _timestamp, long _uptime)
        {
            // Do nothing, handled by LogBuffer
        }

        public override string GetDescription()
        {
            return conDescription;
        }
    }
}