using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Loader;
using MEC;
using Respawning;

namespace SCP_575
{
	public class EventHandlers
	{
		private readonly Plugin _plugin;
		public EventHandlers( Plugin plugin ) => _plugin = plugin;

		public bool TeslasDisabled = false;
		public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

		public IEnumerator<float> RunBlackoutTimer()
		{
			yield return Timing.WaitForSeconds( _plugin.Config.InitialDelay );

			for (; ; )
			{
				RespawnEffectsController.PlayCassieAnnouncement( _plugin.Config.CassieMessageStart, false, false );

				if ( _plugin.Config.DisableTeslas )
					_plugin.EventHandlers.TeslasDisabled = true;

				float blackoutDur = _plugin.Config.DurationMax;
				if ( _plugin.Config.RandomEvents )
					blackoutDur = ( float ) Loader.Random.NextDouble() * ( _plugin.Config.DurationMax - _plugin.Config.DurationMin ) + _plugin.Config.DurationMin;
				if ( _plugin.Config.EnableKeter )
					_plugin.EventHandlers.Coroutines.Add( Timing.RunCoroutine( Keter( blackoutDur ), "keter" ) );

				Map.TurnOffAllLights( blackoutDur, _plugin.Config.OnlyHeavy ? ZoneType.HeavyContainment : ZoneType.Unspecified );
				if ( _plugin.Config.Voice )
					RespawnEffectsController.PlayCassieAnnouncement( _plugin.Config.CassieKeter, false, false );
				yield return Timing.WaitForSeconds( blackoutDur );
				RespawnEffectsController.PlayCassieAnnouncement( _plugin.Config.CassieMessageEnd, false, false );
				Timing.KillCoroutines( "keter" );
				_plugin.EventHandlers.TeslasDisabled = false;
				if ( _plugin.Config.RandomEvents )
					yield return Timing.WaitForSeconds( Loader.Random.Next( _plugin.Config.DelayMin, _plugin.Config.DelayMax ) );
				else
					yield return Timing.WaitForSeconds( _plugin.Config.InitialDelay );
			}
		}

		public IEnumerator<float> Keter( float dur )
		{
			do
			{
				foreach ( Player player in Player.List )
				{
					if ( player.CurrentRoom.AreLightsOff && player.IsHuman && !player.HasFlashlightModuleEnabled && ( !( player.CurrentItem is Flashlight flashlight ) || !flashlight.IsEmittingLight ) )
					{
						player.Hurt( _plugin.Config.KeterDamage, _plugin.Config.KilledBy );
						player.Broadcast( _plugin.Config.KeterBroadcast );
					}
					yield return Timing.WaitForSeconds( 5f );
				}
			} while ( ( dur -= 5f ) > 5f );
		}

		public void OnSpawningRagdoll( SpawningRagdollEventArgs ev )
		{
			if ( !_plugin.StopRagdollList.Contains( ev.Player ) )
				return;

			ev.IsAllowed = false;
			_plugin.StopRagdollList.Remove( ev.Player );
		}

		public void OnRoundStart()
		{
			if ( Loader.Random.Next( 100 ) < _plugin.Config.SpawnChance )
				Coroutines.Add( Timing.RunCoroutine( RunBlackoutTimer() ) );
		}

		public void OnRoundEnd( RoundEndedEventArgs ev )
		{
			foreach ( CoroutineHandle handle in Coroutines )
				Timing.KillCoroutines( handle );
			TeslasDisabled = false;
			Coroutines.Clear();
		}

		// This shouldn't be necessary, but incase someone force-restarts a round, ig.
		public void OnWaitingForPlayers()
		{
			foreach ( CoroutineHandle handle in Coroutines )
				Timing.KillCoroutines( handle );
			TeslasDisabled = false;
			Coroutines.Clear();
		}

		public void OnTriggerTesla( TriggeringTeslaEventArgs ev )
		{
			if ( TeslasDisabled )
				ev.IsAllowed = false;
		}
	}
}
