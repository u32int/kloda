namespace kloda;

using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Enums;
using PlayerRoles;
using System;

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
		if (!Server.FriendlyFire)
			return;

		if (ev.Attacker == null || ev.Player == null)
			return;

		if (Kloda.instance.Config.IgnoreDamageAfterRoundEnd && Round.IsEnded)
			return;

		if (ev.Attacker.Role.Side != ev.Player.Role.Side || ev.Attacker == ev.Player)
			return;

		// At this point this has to be team damage

		if (Kloda.instance.Config.TeamHarmRoleWhiteList.Contains(ev.Attacker.Role)
		    && Kloda.instance.Config.TeamHarmRoleWhiteList.Contains(ev.Player.Role))
		{
			return;
		}

		// Attacker/Victim broadcast notifications
		if (Kloda.instance.Config.NotifyAttacker)
		{
			ev.Attacker.ClearBroadcasts();
			ev.Attacker.ShowHint(Template.Replace(
						Kloda.instance.Config.AttackerDamageMsg, ev.Player, ev.Attacker), 
					     Kloda.instance.Config.BroadcastDuration);
		}

		if (Kloda.instance.Config.NotifyVictim)
		{
			ev.Player.ClearBroadcasts();
			ev.Player.ShowHint(Template.Replace(Kloda.instance.Config.VictimDamageMsg, ev.Player, ev.Attacker),
					Kloda.instance.Config.BroadcastDuration);
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
		if (ev.Attacker == ev.Player)
		{
			if (Kloda.instance.Config.SuicideWebhookEnable)
			{
				Webhook.QueueCombinedTemplated(Kloda.instance.Config.SuicideWebhookMsg, 
						       	       ev.Player, null, ev.DamageHandler);
			}
			return;
		}

		// Killed while handcuffed
		if (ev.Player.IsCuffed && ev.Player.Role.Side != ev.Attacker.Role.Side)
		{
			if (Kloda.instance.Config.CuffedKillWebhookEnable)
			{
				Webhook.QueueCombinedTemplated(Kloda.instance.Config.CuffedKillWebhookMsg, 
						       	       ev.Player, ev.Attacker, ev.DamageHandler);
			}
			return;
		}

		// Teamkill whitelist check
		if (Kloda.instance.Config.TeamHarmRoleWhiteList.Contains(ev.Attacker.Role)
		    && Kloda.instance.Config.TeamHarmRoleWhiteList.Contains(ev.Player.Role))
		{
			return;
		}

		// Teamkill
		if (ev.Player.Role.Side == ev.Attacker.Role.Side && Kloda.instance.Config.TeamKillWebhookEnable)
		{
			Webhook.QueueCombinedTemplated(Kloda.instance.Config.TeamKillWebhookMsg, 
						       ev.Player, ev.Attacker, ev.DamageHandler);
		}
	}

	public static void Banned(BannedEventArgs ev)
	{
		if (Kloda.instance.Config.BanWebhookEnable && ev.Target != null)
		{
			var duration = TimeSpan.FromTicks(ev.Details.Expires - ev.Details.IssuanceTime);

			DateTime issuanceTime = new DateTime(ev.Details.IssuanceTime);
			DateTime expiryDate = new DateTime(ev.Details.Expires);
			try {
				if (Kloda.instance.Config.TimeZoneCode == "Local")
				{
					issuanceTime = issuanceTime.ToLocalTime();
					expiryDate = expiryDate.ToLocalTime();
				} 
				else 
				{
					TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(Kloda.instance.Config.TimeZoneCode);
					issuanceTime = TimeZoneInfo.ConvertTimeFromUtc(issuanceTime, tzi);
					expiryDate = TimeZoneInfo.ConvertTimeFromUtc(expiryDate, tzi);
				}
			}
			catch (TimeZoneNotFoundException)
			{
				Log.Error($"Error while converting time to time zone '{Kloda.instance.Config.TimeZoneCode}': time zone doesn't exist. Falling back to UTC");
			}

			string WebhookMsg = Kloda.instance.Config.BanWebhookMsg
				.Replace("%IssuanceTime%", issuanceTime.ToString("yyyy-MM-dd HH:mm:ss"))
				.Replace("%ExpiryDate%", expiryDate.ToString("yyyy-MM-dd HH:mm:ss"))
				.Replace("%Duration%", $"{duration.ToString("%d")} Days {duration.ToString("%h")} Hours {duration.ToString("%m")} Minutes {duration.ToString("%s")} Seconds")
				.Replace("%Reason%", ev.Details.Reason);

			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(WebhookMsg, ev.Target, ev.Player),
				colorHex: Kloda.instance.Config.EmbedBanColor
			));
		}
	}

	public static void Muted(IssuingMuteEventArgs ev)
	{
		if (Kloda.instance.Config.MuteWebhookEnable && ev.IsAllowed)
		{
			string intercomRepl;
			if (Kloda.instance.Config.IntercomCheckReturnsBool)
			{
				intercomRepl = ev.IsIntercom.ToString();
			}
			else
			{
				intercomRepl = ev.IsIntercom ? "ICOM" : "";
			}

			string WebhookMsg = Kloda.instance.Config.MuteWebhookMsg
				.Replace("%IsIntercom%", intercomRepl);

			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(WebhookMsg, ev.Player),
				colorHex: Kloda.instance.Config.EmbedMuteColor
			));
		}
	}

	public static void MuteRevoked(RevokingMuteEventArgs ev)
	{
		if (Kloda.instance.Config.UnMuteWebhookEnable && ev.IsAllowed)
		{
			string intercomRepl;
			if (Kloda.instance.Config.IntercomCheckReturnsBool)
			{
				intercomRepl = ev.IsIntercom.ToString();
			}
			else
			{
				intercomRepl = ev.IsIntercom ? "ICOM" : "";
			}

			string WebhookMsg = Kloda.instance.Config.UnMuteWebhookMsg
				.Replace("%IsIntercom%", intercomRepl);

			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(WebhookMsg, ev.Player),
				colorHex: Kloda.instance.Config.EmbedUnMuteColor
			));
		}
	}

	public static void Kick(KickingEventArgs ev)
	{
		if (Kloda.instance.Config.KickWebhookEnable && ev.IsAllowed)
		{
			string WebhookMsg = Kloda.instance.Config.KickWebhookMsg
				.Replace("%Reason%", ev.Reason);

			Webhook.QueueEmbed(new DiscordEmbed(
				content: Template.Replace(WebhookMsg, ev.Target, ev.Player),
				colorHex: Kloda.instance.Config.EmbedKickColor
			));
		}
	}
}
