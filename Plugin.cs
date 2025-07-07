using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using System.ComponentModel;
using MEC;

namespace VoteKick
{
    public class Plugin : Plugin<Config, Translation>
    {
        public override string Name => "VoteKick";
        public override string Author => "Antoniofo";
        public override string Prefix => "votekick";
        public override Version Version => new Version(1, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 6, 1);

        public static Plugin Instance;

        public static int Votes;

        public static bool VoteInProgress;

        public static int VkLeft;

        public CoroutineHandle CoroutineHandle;

        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Instance = null;
            Timing.KillCoroutines(CoroutineHandle);
            base.OnDisabled();
        }

        public void OnRoundStart()
        {
            VkLeft = Instance.Config.VoteKickPerRound;
        }

        public static IEnumerator<float> VoteKick(int playerid)
        {
            Votes = 0;
            VoteInProgress = true;
            VkLeft--;
            var player = Player.Get(playerid);
            Map.Broadcast(
                new Exiled.API.Features.Broadcast(
                    Instance.Translation.Startbroadcast
                        .Replace("%timetovote%", Plugin.Instance.Config.TimerForVote.ToString())
                        .Replace("%playername%", player.DisplayNickname), Instance.Config.BroadcastDuration), true);
            for (int i = 0; i < Instance.Config.TimerForVote; i++)
            {
                if (Votes >= Player.List.Count / 2 + 1)
                    break;
                yield return Timing.WaitForSeconds(1f);
            }

            if (Votes >= Player.List.Count / 2 + 1)
            {
                Map.Broadcast(
                    new Exiled.API.Features.Broadcast(
                        Instance.Translation.VoteKicked.Replace("%playername%", player.DisplayNickname)), true);
                player.Ban(300, Plugin.Instance.Translation.VoteKickedReason);
            }
            else
            {
                Map.Broadcast(
                    new Exiled.API.Features.Broadcast(
                        Instance.Translation.NotVoteKicked.Replace("%playername%", player.DisplayNickname)), true);
            }
        }
    }

    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Number of player needed to start a votekick")]
        public int PlayerThreshold { get; set; } = 7;

        [Description("Time in second for how long a votekick should last")]
        public ushort TimerForVote { get; set; } = 60;

        public ushort BroadcastDuration { get; set; } = 10;
        public int VoteKickPerRound { get; set; } = 1;
        public bool AllowVoteAfterWarhead { get; set; } = false;
    }

    public class Translation : ITranslation
    {
#if FRENCH
        public string Startbroadcast { get; set; } =
            "A %timetovote% second votekick has started for <color=red>%playername%</color><br> Utilisé <color=green>.vk vote</color> pour voté ";

        public string VoteKicked { get; set; } = "<color=red>%playername%</color> a été voté et expulsé";
        public string VoteNeeded { get; set; } = "%votes%/%voteneeded% Vote nécessaire";
        public string NotVoteKicked { get; set; } = "Pas assez de vote pour expulser <color=red>%playername%</color>";

        public string VotekickStarted { get; set; } = "Vote commencé";

        public string NotEnoughPlayers { get; set; } = "Pas assez de joueurs sur le serveur";
        public string NotEnoughArgs { get; set; } = "Pas assez d'argument";
        public string EmptyArgs { get; set; } = "Argument est vide";
        public string ArgsNotId { get; set; } = "Argument est pas un Id";
        public string PlayerDesntExist { get; set; } = "Ce joueur n'existe pas";
        public string CanVoteKickAdmin { get; set; } = "Vous ne pouvez pas vote une admin";
        public string NoMoreVotekick { get; set; } = "Plus de vote pour ce round";
        public string VotekickStillRunning { get; set; } = "Un vote est toujours en cours";
        public string CanVoteAfterWarhead { get; set; } = "Vous ne pouvez pas voté après le détonation de la Warhead";
        public string NoVoteOrAlreadyVoted { get; set; } = "Il n'y a pas de vote ou vous avez déjà voté";

        public string VoteToKick { get; set; } = "Vous avez voté pour expluser";

        public string Usage { get; set; } = "Sous-commande invalide. Disponible: start, vote, list";

        public string VoteKickedReason { get; set; } = "Vous avez été voté et explusé";
#else
        public string Startbroadcast { get; set; } =
            "A %timetovote% second votekick has started for <color=red>%playername%</color><br> Use <color=green>.vk vote</color> to vote ";

        public string VoteKicked { get; set; } = "<color=red>%playername%</color> has been votekicked";
        public string VoteNeeded { get; set; } = "%votes%/%voteneeded% Vote needed";
        public string NotVoteKicked { get; set; } = "Not enough vote to kick <color=red>%playername%</color>";

        public string VotekickStarted { get; set; } = "VoteKick started";

        public string NotEnoughPlayers { get; set; } = "Not enough player on the server";
        public string NotEnoughArgs { get; set; } = "Not enough argument";
        public string EmptyArgs { get; set; } = "Argument is empty";
        public string ArgsNotId { get; set; } = "Argument is not an Id";
        public string PlayerDesntExist { get; set; } = "This player doesn't exist";
        public string CanVoteKickAdmin { get; set; } = "You can't votekick an admin";
        public string NoMoreVotekick { get; set; } = "No more votekick for this round";
        public string VotekickStillRunning { get; set; } = "A votekick is still running";
        public string CanVoteAfterWarhead { get; set; } = "You can't votekick after warhead is detonated";
        public string NoVoteOrAlreadyVoted { get; set; } = "No vote currently or already voted";

        public string VoteToKick { get; set; } = "You voted to kick";

        public string Usage { get; set; } = "Invalide subcommand. Available: start, vote, list";

        public string VoteKickedReason { get; set; } = "Your have been votekicked";
#endif
    }
}