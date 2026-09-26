using ServerCore.FileCache;
using ServerCore.Web.Handlers;
using Platform.EOS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ServerCore.Web
{
    public class Web : IConsoleServer
    {
        private const int GUEST_PERMISSION_LEVEL = 2000;
        public static int handlingCount;
        public static int currentHandlers;
        public static long totalHandlingTime = 0;
        private readonly HttpListener _listener = new HttpListener();
        private readonly string dataFolder;
        private readonly Dictionary<string, PathHandler> handlers = new CaseInsensitiveStringDictionary<PathHandler>();
        private readonly bool useStaticCache;

        public ConnectionHandler connectionHandler;

        public Web()
        {
            try
            {
                int webPort = GamePrefs.GetInt(EnumUtils.Parse<EnumGamePrefs>("WebDashboardPort"));
                if (webPort < 1 || webPort > 65533)
                {
                    Log.Out("ClaimCreator not started (WebDashboardPort not within 1-65533)");
                    return;
                }

                if (!Directory.Exists(ServerCore.API.modPath + "/ClaimCreator"))
                {
                    Log.Out("ClaimCreator not started (folder \"ClaimCreator\" not found in mod folder)");
                    return;
                }

                // TODO: Read from config
                useStaticCache = false;

                dataFolder = ServerCore.API.modPath + "/ClaimCreator";

                if (!HttpListener.IsSupported)
                {
                    Log.Out("ClaimCreator not started (needs Windows XP SP2, Server 2003 or later or Mono)");
                    return;
                }

                handlers.Add(
                    "/index.htm",
                    new SimpleRedirectHandler("/static/index.html"));
                handlers.Add(
                    "/favicon.ico",
                    new SimpleRedirectHandler("/static/favicon.ico"));
                handlers.Add(
                    "/session/",
                    new SessionHandler(
                        "/session/",
                        dataFolder,
                        this)
                );
                handlers.Add(
                    "/userstatus",
                    new UserStatusHandler()
                );
                if (useStaticCache)
                {
                    handlers.Add(
                        "/static/",
                        new StaticHandler(
                            "/static/",
                            dataFolder,
                            new SimpleCache(),
                            false)
                    );
                }
                else
                {
                    handlers.Add(
                        "/static/",
                        new StaticHandler(
                            "/static/",
                            dataFolder,
                            new DirectAccess(),
                            false)
                    );
                }

                handlers.Add(
                    "/map/",
                    new StaticHandler(
                        "/map/",
                        GameIO.GetSaveGameDir() + "/PrismaCoreMap",
                        MapRendering.MapRendering.GetTileCache(),
                        false,
                        "ClaimCreator.map")
                );

                handlers.Add(
                    "/api/",
                    new ApiHandler("/api/")
                );

                connectionHandler = new ConnectionHandler();

                _listener.Prefixes.Add(string.Format("http://*:{0}/", ServerCoreSettings.Instance.WebUI_Port));
                _listener.Start();

                SdtdConsole.Instance.RegisterServer(this);

                _listener.BeginGetContext(HandleRequest, _listener);

                Log.Out("[PrismaCore] Started ClaimCreator on " + (ServerCoreSettings.Instance.WebUI_Port));
            }
            catch (Exception e)
            {
                Log.Out("[PrismaCore] Error in Web.ctor: " + e);
            }
        }

        public void Disconnect()
        {
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (Exception e)
            {
                Log.Out("[PrismaCore] Error in Web.Disconnect: " + e);
            }
        }

        public void SendLine(string _line)
        {
            connectionHandler.SendLine(_line);
        }

        public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
        {
            // Do nothing, handled by LogBuffer internally
        }

        public static bool isSslRedirected(HttpListenerRequest _req)
        {
            string proto = _req.Headers["X-Forwarded-Proto"];
            if (!string.IsNullOrEmpty(proto))
            {
                return proto.Equals("https", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private readonly Version HttpProtocolVersion = new Version(1, 1);

#if ENABLE_PROFILER
		private readonly CustomSampler authSampler = CustomSampler.Create ("Auth");
		private readonly CustomSampler handlerSampler = CustomSampler.Create ("Handler");
#endif

        private void HandleRequest(IAsyncResult _result)
        {
            if (!_listener.IsListening)
            {
                return;
            }

            Interlocked.Increment(ref handlingCount);
            Interlocked.Increment(ref currentHandlers);

#if ENABLE_PROFILER
			Profiler.BeginThreadProfiling ("AllocsMods", "WebRequest");
			HttpListenerContext ctx = _listener.EndGetContext (_result);
			try {
#else
            HttpListenerContext ctx = _listener.EndGetContext(_result);
            _listener.BeginGetContext(HandleRequest, _listener);
#endif
            try
            {
                HttpListenerRequest request = ctx.Request;
                HttpListenerResponse response = ctx.Response;
                response.SendChunked = false;

                response.ProtocolVersion = HttpProtocolVersion;

                WebConnection conn;
#if ENABLE_PROFILER
				authSampler.Begin ();
#endif
                int permissionLevel = DoAuthentication(request, out conn);
#if ENABLE_PROFILER
				authSampler.End ();
#endif



                if (conn != null)
                {
                    Cookie cookie = new Cookie("sidprismacore", conn.SessionID, "/");
                    cookie.Expired = false;
                    cookie.HttpOnly = true;
                    cookie.Secure = false;
                    response.AppendCookie(cookie);
                }

                // No game yet -> fail request
                if (GameManager.Instance.World == null)
                {
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    return;
                }

                if (request.Url.AbsolutePath.Length < 2)
                {
                    handlers["/index.htm"].HandleRequest(request, response, conn, permissionLevel);
                    return;
                }
                else
                {
                    foreach (KeyValuePair<string, PathHandler> kvp in handlers)
                    {
                        if (request.Url.AbsolutePath.StartsWith(kvp.Key))
                        {
                            if (!kvp.Value.IsAuthorizedForHandler(conn, permissionLevel))
                            {
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                if (conn != null)
                                {
                                }
                            }
                            else
                            {
#if ENABLE_PROFILER
								handlerSampler.Begin ();
# endif                         
                                response.AddHeader("Access-Control-Allow-Origin", "*");
                                kvp.Value.HandleRequest(request, response, conn, permissionLevel);
#if ENABLE_PROFILER
								handlerSampler.End ();
#endif
                            }

                            return;
                        }
                    }
                }

                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            catch (IOException e)
            {
                if (e.InnerException is SocketException)
                {
                    Log.Out("[PrismaCore] Error in Web.HandleRequest(): Remote host closed connection: " +
                             e.InnerException.Message);
                }
                else
                {
                    Log.Out("Error (IO) in Web.HandleRequest(): " + e);
                }
            }
            catch (Exception e)
            {
                Log.Out("[PrismaCore] Error in Web.HandleRequest(): " + e);
            }
            finally
            {
                if (ctx != null && !ctx.Response.SendChunked)
                {
                    ctx.Response.Close();
                }

               Interlocked.Decrement(ref currentHandlers);
            }
#if ENABLE_PROFILER
			} finally {
				_listener.BeginGetContext (HandleRequest, _listener);
				Profiler.EndThreadProfiling ();
			}
#endif
        }

        private int DoAuthentication(HttpListenerRequest _req, out WebConnection _con)
        {
            _con = null;

            string sessionId = null;
            if (_req.Cookies["sidprismacore"] != null)
            {
                sessionId = _req.Cookies["sidprismacore"].Value;
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                WebConnection con = connectionHandler.IsLoggedIn(sessionId, _req.RemoteEndPoint.Address);
                if (con != null)
                {
                    _con = con;

                    Dictionary<PlatformUserIdentifierAbs, AdminUsers.UserPermission> admins = GameManager.Instance.adminTools.Users.GetUsers();

                    if (admins.ContainsKey(_con.UserId))
                    {
                        return GameManager.Instance.adminTools.Users.GetUserPermissionLevel(_con.UserId);
                    }
                    else
                    {
                        string eos = Database.Instance.GetEOSIdBySteamId(_con.UserId.ToString());

                        if (string.IsNullOrEmpty(eos))
                        {
                            return 1000;
                        }

                        PlatformUserIdentifierAbs EOS_id = new UserIdentifierEos(eos.Replace("EOS_", string.Empty));
                        return GameManager.Instance.adminTools.Users.GetUserPermissionLevel(EOS_id);
                    }
                }
            }

            if (_req.QueryString["apiuser"] != null && _req.QueryString["password"] != null)
            {
                WebPermissions.AdminToken admin = WebPermissions.Instance.GetWebAdmin(_req.QueryString["apiuser"],
                    _req.QueryString["password"]);
                if (admin != null)
                {
                    return admin.permissionLevel;
                }

                Log.Warning("Invalid password used from " + _req.RemoteEndPoint);
            }

            if (_req.Url.AbsolutePath.StartsWith("/session/verify", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ulong id = OpenID.Validate(_req);
                    if (id > 0)
                    {
                        WebConnection con = connectionHandler.LogIn(id, _req.RemoteEndPoint.Address);
                        _con = con;

                        Dictionary<PlatformUserIdentifierAbs, AdminUsers.UserPermission> admins = GameManager.Instance.adminTools.Users.GetUsers();

                        if (admins.ContainsKey(_con.UserId))
                        {
                            int level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(con.UserId);

                            Log.Out("[PrismaCore] Steam OpenID login from {0} with ID {1}, permission level {2}",
                            _req.RemoteEndPoint.ToString(), con.UserId, level);

                            return level;
                        }
                        else
                        {
                            int level = 1000;

                            string eos = Database.Instance.GetEOSIdBySteamId(con.UserId.ToString());

                            if (!string.IsNullOrEmpty(eos))
                            {
                                PlatformUserIdentifierAbs EOS_id = new UserIdentifierEos(eos.Replace("EOS_", string.Empty));
                                level = GameManager.Instance.adminTools.Users.GetUserPermissionLevel(EOS_id);
                            }

                            Log.Out("[PrismaCore] Steam OpenID login from {0} with ID {1}, permission level {2} (checked by eosId, if you are using eosId in serveradmin.xml, you MUST have logged in once on the server for permissions to be applied!)",
                            _req.RemoteEndPoint.ToString(), con.UserId, level);

                            return level;
                        }
                    }

                    Log.Out("[PrismaCore] Steam OpenID login failed from {0}", _req.RemoteEndPoint.ToString());
                }
                catch (Exception e)
                {
                    Log.Error("[PrismaCore] Error validating login:");
                    Log.Exception(e);
                }
            }

            return GUEST_PERMISSION_LEVEL;
        }

        public static void SetResponseTextContent(HttpListenerResponse _resp, string _text)
        {
            byte[] buf = Encoding.UTF8.GetBytes(_text);
            _resp.ContentLength64 = buf.Length;
            _resp.ContentType = "text/html";
            _resp.ContentEncoding = Encoding.UTF8;
            _resp.OutputStream.Write(buf, 0, buf.Length);
        }
    }
}