using System;
using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using Server = Exiled.Events.Handlers.Server;
using PlayerHandler = Exiled.Events.Handlers.Player;

namespace SCP_575
{
	public class Plugin : Plugin<Config>
	{
		public override string Author { get; } = "Joker119 | Modified by LambdaGaming";
		public override string Name { get; } = "SCP-575 Lite";
		public override string Prefix { get; } = "575";
		public override Version Version { get; } = new Version( 4, 0, 1 );
		public override Version RequiredExiledVersion { get; } = new Version( 6, 0, 0 );

		public EventHandlers EventHandlers { get; private set; }
		public List<Player> StopRagdollList { get; } = new List<Player>();

		public override void OnEnabled()
		{
			EventHandlers = new EventHandlers( this );
			Server.WaitingForPlayers += EventHandlers.OnWaitingForPlayers;
			Server.RoundEnded += EventHandlers.OnRoundEnd;
			Server.RoundStarted += EventHandlers.OnRoundStart;
			PlayerHandler.TriggeringTesla += EventHandlers.OnTriggerTesla;
			PlayerHandler.SpawningRagdoll += EventHandlers.OnSpawningRagdoll;
			base.OnEnabled();
		}

		public override void OnDisabled()
		{
			foreach ( CoroutineHandle handle in EventHandlers.Coroutines )
				Timing.KillCoroutines( handle );
			EventHandlers.Coroutines.Clear();
			Server.WaitingForPlayers -= EventHandlers.OnWaitingForPlayers;
			Server.RoundEnded -= EventHandlers.OnRoundEnd;
			Server.RoundStarted -= EventHandlers.OnRoundStart;
			PlayerHandler.TriggeringTesla -= EventHandlers.OnTriggerTesla;
			PlayerHandler.SpawningRagdoll -= EventHandlers.OnSpawningRagdoll;
			EventHandlers = null;
			base.OnDisabled();
		}
	}
}