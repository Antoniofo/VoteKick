using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;

namespace VoteKick
{    
    class List : ICommand, IUsageProvider
    {
        public string Command => "list";

        public string[] Aliases => new string[] { };

        public string Description => "List player that can be votekick";

        public string[] Usage => new string[] { ".votekick/.vk", "list" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "\n";
            foreach(var player in Player.List.Where(x => x.RemoteAdminAccess == false))
            {
                response += $"{player.Id} : {player.DisplayNickname}\n";
            }            
            return true;
        }
    }
}
