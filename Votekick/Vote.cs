using System;
using CommandSystem;
using Exiled.API.Features;

namespace VoteKick
{
    class Vote : ICommand, IUsageProvider
    {
        public string Command => "vote";

        public string[] Aliases => new string[] { };

        public string Description => "Vote to a kick the player";

        public string[] Usage => new string[] { ".votekick/.vk", "vote"};

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Plugin.VoteInProgress && (bool)Player.Get(sender).SessionVariables["votekick_voted"] == false)
            {
                Plugin.Votes++;
                Player.Get(sender).SessionVariables["votekick_voted"] = true;
                Map.Broadcast(new Exiled.API.Features.Broadcast(Plugin.Instance.Translation.VoteNeeded.Replace("%votes%",Plugin.Votes.ToString()).Replace("%voteneeded%",(Player.List.Count / 2 + 1).ToString()),5));
                response = Plugin.Instance.Translation.VoteToKick;
                return true;
            }
            response = Plugin.Instance.Translation.NoVoteOrAlreadyVoted;
            return false;
        }
    }
}
