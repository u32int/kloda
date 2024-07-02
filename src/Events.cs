namespace kloda;

using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;

public class EventHandler
{
	public static void Verified(VerifiedEventArgs ev) 
	{
		if (Kloda.instance.Config.JoinWebhookEnable)
			Webhook.SendTemplated(Kloda.instance.Config.JoinWebhookMsg, playerA: ev.Player, playerB: null);
	}

	public static void Hurting(HurtingEventArgs ev)
	{
		if (Server.FriendlyFire) // TODO: is this necessary?
			return;

		if (ev.Attacker == null || ev.Player == null) // TODO: is this necessary?
			return;

		if (Kloda.instance.Config.IgnoreDamageAfterRoundEnd && Round.IsEnded)
			return;

		if (ev.Attacker.Role.Side != ev.Player.Role.Side || ev.Attacker == ev.Player)
			return;

		// Attacker/Victim broadcast notifications
		if (Kloda.instance.Config.NotifyAttacker)
		{
			ev.Attacker.ClearBroadcasts();
			ev.Attacker.Broadcast(
					Kloda.instance.Config.BroadcastDuration, 
					Template.Replace(Kloda.instance.Config.AttackerDamageMsg, ev.Player, ev.Attacker)
			);
		}

		if (Kloda.instance.Config.NotifyVictim)
		{
			ev.Player.ClearBroadcasts();
			ev.Player.Broadcast(
					Kloda.instance.Config.BroadcastDuration, 
					Template.Replace(Kloda.instance.Config.VictimDamageMsg, ev.Player, ev.Attacker)
			);
		}

		// hook the web (if the web is to be hooked, of course)
		if (Kloda.instance.Config.DamageWebhookEnable)
			Webhook.SendTemplated(Kloda.instance.Config.DamageWebhookMsg, ev.Player, ev.Attacker);
	}

	// O_o
	public static void Death(DyingEventArgs ev)
	{
		if (Kloda.instance.Config.IgnoreDeathAfterRoundEnd && Round.IsEnded)
			return;

		// Suicide
		if (ev.Attacker == ev.Player && Kloda.instance.Config.SuicideWebhookEnable)
		{
			Webhook.SendTemplated(Kloda.instance.Config.SuicideWebhookMsg, ev.Player, null, ev.DamageHandler);
			return;
		}

		// Killed while handcuffed
		if (ev.Player.IsCuffed && Kloda.instance.Config.CuffedKillWebhookEnable)
		{
			Webhook.SendTemplated(Kloda.instance.Config.CuffedKillWebhookMsg, 
					ev.Player, ev.Attacker, ev.DamageHandler);
			return;
		}

		// Teamkill
		if (ev.Player.Role.Side == ev.Attacker.Role.Side && Kloda.instance.Config.TeamkillWebhookEnable)
		{
			Webhook.SendTemplated(Kloda.instance.Config.TeamkillWebhookMsg, 
					ev.Player, ev.Attacker, ev.DamageHandler);
			return;
		}
	}

	public static void Banned(BannedEventArgs ev)
	{
		if (Kloda.instance.Config.BanWebhookEnable)
		{
			Webhook.SendTemplated(Kloda.instance.Config.BanWebhookMsg,
				              ev.Target, ev.Player);
		}
	}

	public static void Muted(IssuingMuteEventArgs ev)
	{
		if (Kloda.instance.Config.BanWebhookEnable)
		{
			Webhook.SendTemplated(Kloda.instance.Config.MuteWebhookMsg,
				              ev.Player);
		}
	}
}
