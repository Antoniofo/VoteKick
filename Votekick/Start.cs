using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using MEC;

namespace VoteKick
{    
    class Start : ICommand, IUsageProvider
    {
        public string Command => "start";

        public string[] Aliases => new string[] { };

        public string Description => "start a vote kick";

        public string[] Usage => new string[] { ".votekick/.vk", "start", "playerID" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {            
            //foreach(var str in arguments.Array)
            //{
            //    Log.Info($"arg: {str}\n");
            //}
            if (Server.PlayerCount < Plugin.Instance.Config.PlayerThreshold)
            {
                response = "Not enough player on the server";
                return false;
            }            
            if(arguments.Array.Count() < 3)
            {
                response = "Not enough argument";
                return false;
            }
            if(arguments.Array[2] == null || arguments.Array[2].Equals(""))
            {
                response = "Argument empty";
                return false;
            }
            if(!int.TryParse(arguments.Array[2], out int pid))
            {
                response = "Argument not an ID";
                return false;
            }
            if (Player.Get(pid) == null)
            {
                response = "This player doesn't exist";
                return false;
            }
            if (Player.Get(pid).RemoteAdminAccess)
            {
                response = "You can't votekick an admin";
                return false;
            }         
            if(Plugin.VkLeft <= 0)
            {
                response = "No more votekick for this round";
                return false;
            }
            if (Plugin.Instance.CoroutineHandle.IsRunning)
            {
                response = "A votekick is still running";
                return false;
            }
            foreach (var ply in Player.List)
            {
                //Log.Info($"ply: {ply.DisplayNickname}");
                if (ply.SessionVariables.ContainsKey("votekick_voted"))
                {
                    ply.SessionVariables["votekick_voted"] = false;
                }
                else
                {
                    ply.SessionVariables.Add("votekick_voted", false);
                }
                
            }
            Server.ExecuteCommand($"@{Player.Get(sender).DisplayNickname} has started VoteKick");
            Plugin.Instance.CoroutineHandle = Timing.RunCoroutine(Plugin.VoteKick(pid));
            response = "VoteKick started";
            return true;
        }
    }
}
