using System.Collections.Generic;

namespace ServerCore.CustomCommands
{
    public class WaypointControl : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new[] { "pc-wpc", "wpc" };
        }

        public override string getDescription()
        {
            return "Manage PrismaCore waypoints";
        }

        public override string getHelp()
        {
            return "Add/Remove/List waypoints\n" +
                "Usage:\n" +
                "   wpc add <name> <x> <y> <z>\n" +
                "   wpc remove <name>\n" +
                "   wpc list";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (_params.Count >= 1)
            {
                switch (_params[0].ToLower())
                {
                    case "add":
                        ExecuteAdd(_params);
                        break;
                    case "remove":
                        ExecuteRemove(_params);
                        break;
                    case "list":
                        ExecuteList();
                        break;
                    default:
                        SdtdConsole.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
                        return;
                }
            }
            else
            {
                SdtdConsole.Instance.Output("No sub command given.");
            }
        }

        private void ExecuteAdd(List<string> _params)
        {
            if (_params.Count != 5)
            {
                SdtdConsole.Instance.Output("Wrong number of arguments, expected 5, found " + _params.Count + ".");
                return;
            }

            if (string.IsNullOrEmpty(_params[1]))
            {
                SdtdConsole.Instance.Output("Argument 'name' is empty.");
                return;
            }

            if (!int.TryParse(_params[2], out int x))
            {
                SdtdConsole.Instance.Output($"x parameter invalid. Not an integer.");
                return;
            }
            if (!int.TryParse(_params[3], out int y))
            {
                SdtdConsole.Instance.Output($"y parameter invalid. Not an integer.");
                return;
            }
            if (!int.TryParse(_params[4], out int z))
            {
                SdtdConsole.Instance.Output($"z parameter invalid. Not an integer.");
                return;
            }

            bool succes = Database.Instance.SaveDbWaypoint(_params[1], x, y, z);

            if (succes)
            {
                SdtdConsole.Instance.Output(string.Format("Waypoint with name={0} added with coordinates of X:{1} Y:{2} Z:{3}.", _params[1], x, y, z));
            }
            else
            {
                SdtdConsole.Instance.Output(string.Format("Waypoint with name={0} could not be added to waypoints. Make sure you dont use existing waypointnames.", _params[1]));
            }
        }

        private void ExecuteRemove(List<string> _params)
        {
            if (_params.Count != 2)
            {
                SdtdConsole.Instance.Output("Wrong number of arguments, expected 2, found " + _params.Count + ".");
                return;
            }

            if (string.IsNullOrEmpty(_params[1]))
            {
                SdtdConsole.Instance.Output("Argument 'name' is empty.");
                return;
            }

            bool removed = Database.Instance.DeleteDbWaypoint((_params[1]));
            if (removed)
            {
                SdtdConsole.Instance.Output(string.Format("Waypoint with name={0} has been removed from waypoints.", _params[1]));

            }
            else SdtdConsole.Instance.Output(string.Format("Waypoint with name={0} could not be removed from waypoints.", _params[1]));
        }

        private void ExecuteList()
        {
            SdtdConsole.Instance.Output("Defined waypoints:");
            SdtdConsole.Instance.Output("  Name: Coordinates");

            var result = Database.Instance.ListDbWaypoints();

            foreach (DbWaypoint wp in result)
            {
                string wpcoord = string.Format("X={0} Y={1} Z={2}", wp.X, wp.Y, wp.Z);
                SdtdConsole.Instance.Output(string.Format("  {0}: {1}", wp.Id, wpcoord));
            }
        }
    }
}
