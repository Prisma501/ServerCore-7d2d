using System;
using System.Net;

namespace PrismaCore.Web.API
{
    public class CreateAdvClaims : WebAPI
    {
        public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnection _user, int _permissionLevel)
        {

            if (string.IsNullOrEmpty(_req.QueryString["command"]))
            {
                _resp.StatusCode = (int)HttpStatusCode.BadRequest;
                Web.SetResponseTextContent(_resp, "No command given");
                return;
            }

            WebCommandResult.ResultType responseType =
                _req.QueryString["raw"] != null ? WebCommandResult.ResultType.Raw :
                (_req.QueryString["simple"] != null ? WebCommandResult.ResultType.ResultOnly :
                    WebCommandResult.ResultType.Full);

            string commandline = _req.QueryString["command"];
            if (!commandline.StartsWith("mrr") && !commandline.StartsWith("ccc"))
            {
                _resp.StatusCode = (int)HttpStatusCode.BadRequest;
                Web.SetResponseTextContent(_resp, "Api can only be used for mrr and ccc ");
                return;
            }
            string commandPart = commandline.Split(' ')[0];
            string argumentsPart = commandline.Substring(Math.Min(commandline.Length, commandPart.Length + 1));

            IConsoleCommand command = SdtdConsole.Instance.GetCommand(commandline);

            if (command == null)
            {
                _resp.StatusCode = (int)HttpStatusCode.NotImplemented;
                Web.SetResponseTextContent(_resp, "Unknown command");
                return;
            }

            _resp.SendChunked = true;
            WebCommandResult wcr = new WebCommandResult(commandPart, argumentsPart, responseType, _resp);
            SdtdConsole.Instance.ExecuteAsync(commandline, wcr);
        }
    }
}