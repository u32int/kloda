namespace kloda;

using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Player;

public class EventHandler
{
	public static void Verified(VerifiedEventArgs ev) 
	{
		if (!Kloda.instance.Config.JoinWebhookEnable)
			return;
		
		Webhook.QueueEmbed(new DiscordEmbed(
			content: Template.Replace(Kloda.instance.Config.JoinWebhookMsg, playerA: ev.Player, playerB: null),
			colorHex: Kloda.instance.Config.EmbedJoinColor
		));
	}

	public static void Hurting(HurtingEventArgs ev)
	{
		if (!Server.FriendlyFire) // TODO: is this necessary?
			return;

		if (ev.Attacker == null || ev.Player == null)
			return;

		if (Kloda.instance.Config.IgnoreDamageAfterRoundEnd && Round.IsEnded)
			return;

		if (ev.Attacker.Role.Side != ev.Player.Role.Side || ev.Attacker == ev.Player)
			return;

		// At this point this has to be team damage

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

		if (Kloda.instance.Config.TeamDamageWebhookEnable)
			Webhook.QueueCombinedTemplated(Kloda.instance.Config.TeamDamageWebhookMsg, ev.Player, ev.Attacker);
	}

	public static void Death(DyingEventArgs ev)
	{
		if (Kloda.instance.Config.IgnoreDeathAfterRoundEnd && Round.IsEnded)
			return;

		// Most likely death caused by something that is not a player; i.e. fall damage or tesla.
		if (ev.Attacker == null)
			return;

		// Suicide
		if (ev.Attacker == ev.Player && Kloda.instance.Config.SuicideWebhookEnable)
		{
			Webhook.QueueCombinedTemplated(Kloda.instance.Config.SuicideWebhookMsg, 
						       ev.Player, null, ev.DamageHandler);
			return;
		}

		// Killed while handcuffed
		if (ev.Player.IsCuffed && Kloda.instance.Config.CuffedKillWebhookEnable)
		{
			Webhook.QueueCombinedTemplated(Kloda.instance.Config.CuffedKillWebhookMsg, 
						       ev.Player, ev.Attacker, ev.DamageHandler);
			return;
		}

		// Teamkill
		if (ev.Player.Role.Side == ev.Attacker.Role.Side && Kloda.instance.Config.TeamKillWebhookEnable)
		{
			Webhook.QueueCombinedTemplated(Kloda.instance.Config.TeamKillWebhookMsg, 
						       ev.Player, ev.Attacker, ev.DamageHandler);
			return;
		}
	}

	public static void Banned(BannedEventArgs ev)
	{
		if (Kloda.instance.Config.BanWebhookEnable)
		{
			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(Kloda.instance.Config.BanWebhookMsg, ev.Target, ev.Player),
				colorHex: Kloda.instance.Config.EmbedBanColor
			));
		}
	}

	public static void Muted(IssuingMuteEventArgs ev)
	{
		if (Kloda.instance.Config.BanWebhookEnable)
		{
			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(Kloda.instance.Config.MuteWebhookMsg, ev.Player),
				colorHex: Kloda.instance.Config.EmbedMuteColor
			));
		}
	}

	public static void Kick(KickingEventArgs ev)
	{
		if (Kloda.instance.Config.KickWebhookEnable)
		{
			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(Kloda.instance.Config.KickWebhookMsg, ev.Target, ev.Player),
				colorHex: Kloda.instance.Config.EmbedKickColor
			));
		}
	}
}
