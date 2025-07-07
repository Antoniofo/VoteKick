using System;
using System.Linq;
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
            if (Server.PlayerCount < Plugin.Instance.Config.PlayerThreshold)
            {
                response = Plugin.Instance.Translation.NotEnoughPlayers;
                return false;
            }

            if (arguments.Array.Count() < 3)
            {
                response = Plugin.Instance.Translation.NotEnoughArgs;
                return false;
            }

            if (arguments.Array[2] == null || arguments.Array[2].Equals(""))
            {
                response = Plugin.Instance.Translation.EmptyArgs;
                return false;
            }

            if (!int.TryParse(arguments.Array[2], out int pid))
            {
                response = Plugin.Instance.Translation.ArgsNotId;
                return false;
            }

            if (Player.Get(pid) == null)
            {
                response = Plugin.Instance.Translation.PlayerDesntExist;
                return false;
            }

            if (Player.Get(pid).RemoteAdminAccess)
            {
                response = Plugin.Instance.Translation.CanVoteKickAdmin;
                return false;
            }

            if (Plugin.VkLeft <= 0)
            {
                response = Plugin.Instance.Translation.NoMoreVotekick;
                return false;
            }

            if (Plugin.Instance.CoroutineHandle.IsRunning)
            {
                response = Plugin.Instance.Translation.VotekickStillRunning;
                return false;
            }

            if (Plugin.Instance.Config.AllowVoteAfterWarhead && Warhead.IsDetonated)
            {
                response = Plugin.Instance.Translation.CanVoteAfterWarhead;
                return false;
            }

            foreach (var ply in Player.List)
            {
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
            response = Plugin.Instance.Translation.VotekickStarted;
            return true;
        }
    }
}