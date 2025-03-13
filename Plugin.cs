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
        public override Version Version => new Version(1, 0, 1);

        public static Plugin Instance;

        public static int Votes;

        public static bool VoteInProgress;

        public static int VkLeft;

        public CoroutineHandle CoroutineHandle;

        public override void OnEnabled()
        {
            base.OnEnabled();
            Instance = this;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Instance = null;
            Timing.KillCoroutines(CoroutineHandle);
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
                player.Ban(300, "Your have been votekicked");
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
    }

    public class Translation : ITranslation
    {
        public string Startbroadcast { get; set; } =
            "A %timetovote% second votekick has started for <color=red>%playername%</color><br> Use <color=green>.vk vote</color> to vote ";

        public string VoteKicked { get; set; } = "<color=red>%playername%</color> has been votekicked";
        public string VoteNeeded { get; set; } = "%votes%/%voteneeded% Vote needed";
        public string NotVoteKicked { get; set; } = "Not enough vote to kick <color=red>%playername%</color>";
    }
}