namespace kloda;

using System;
using System.Threading.Tasks;

using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;

/// main plugin configuration, as defined by the Exiled "IConfig" thing (basicaly 
/// just requires IsEnabled and Debug to be here, these options end up in an end-user
/// editable file in a yaml-ish format)
public class Config : Exiled.API.Interfaces.IConfig
{
	// General
	public bool IsEnabled { get; set; } = true;
	public bool Debug { get; set; } = false;
	public ushort BroadcastDuration { get; set; } = 5;

	// Discord
	public string DiscordWebhookUrl { get; set; } = "";
	public bool DiscordWebhookEnable { get; set; } = false;
	public string DiscordNickname { get; set; } = "kloda";
	public string DiscordAvatarUrl { get; set; } = "https://cdn.discordapp.com/attachments/859050843029766177/1258498039132323981/log_n.png?ex=66884322&is=6686f1a2&hm=491ad78b6630aa38e38cd8ba9af22e9d4634bfcd840b0bccd453e87a7d3ebc69";
	public float DiscordWebhookCooldown { get; set; } = 5;
	public float DiscordWebhookQueueFlush { get; set; } = 10;
	public string EmbedJoinColor { get; set; } = "22c17f";
	public string EmbedBanColor { get; set; } = "d13934";
	public string EmbedMuteColor { get; set; } = "c68e2b";
	public string EmbedUnMuteColor { get; set; } = "c68e2b";
	public string EmbedKickColor { get; set; } = "eae738";

	// Damage 
	public bool IgnoreDamageAfterRoundEnd { get; set; } = true;
	public bool TeamDamageWebhookEnable { get; set; } = true;
	public string TeamDamageWebhookMsg { get; set; } = "**%Attacker%** damaged their teammate **%Victim%**";
	public string TeamDamageWebhookUrl { get; set; } = "";
	public bool NotifyAttacker { get; set; } = true;
	public string AttackerDamageMsg { get; set; } = "<color=red>Warning</color> You've attacked your teammate";
	public bool NotifyVictim { get; set; } = false;
	public string VictimDamageMsg { get; set; } = "<color=red>Warning</color> Attacked by teammate";

	// Death
	public bool IgnoreDeathAfterRoundEnd { get; set; } = true;
	public bool TeamKillWebhookEnable { get; set; } = true;
	public string TeamKillWebhookMsg { get; set; } = "**%Victim%** teamkilled by **%Attacker%**";
	public bool CuffedKillWebhookEnable { get; set; } = true;
	public string CuffedKillWebhookMsg { get; set; } = 
		"**%Victim%** was killed by **%Attacker%** using **%DamageType%**, while **%Victim%** was handcuffed";
	public bool SuicideWebhookEnable { get; set; } = true;
	public string SuicideWebhookMsg { get; set; } = "**%Player%** died by suicide using **%DamageType%** :((";

	// Bans
	public bool BanWebhookEnable { get; set; } = true;
	public string BanWebhookMsg { get; set; } = "**%Target%** was banned by **%Issuer%**. Reason: %Reason%. Duration: %Duration%. Expires: %ExpiryDate%";

	// Mutes
	public bool MuteWebhookEnable { get; set; } = true;
	public string MuteWebhookMsg { get; set; } = "**%Target%** was muted.";
	public bool UnMuteWebhookEnable { get; set; } = true;
	public string UnMuteWebhookMsg { get; set; } = "**%Target%** was unmuted.";

	// Kicking
	public bool KickWebhookEnable { get; set; } = true;
	public string KickWebhookMsg { get; set; } = "**%Target%** was kicked by **%Issuer%**. Reason: `%Reason%`.";

	// Player verify (server join)
	public bool JoinWebhookEnable { get; set; } = false;
	public string JoinWebhookMsg { get; set; } = "**%Player%** joined the server!";
} 

// main class of this entire object oriented endeavour apparently
public class Kloda: Plugin<Config>
{
	public static Kloda instance;

	public override string Name => "kloda";
	public override string Author => "u32int";
	public override Version Version => new Version(1, 0, 0);
	public override Version RequiredExiledVersion => new Version(8, 0, 0);

	public override void OnEnabled()
	{        
		instance = this;

		Exiled.Events.Handlers.Player.Verified += EventHandler.Verified;
		Exiled.Events.Handlers.Player.Dying += EventHandler.Death;
		Exiled.Events.Handlers.Player.Hurting += EventHandler.Hurting;
		Exiled.Events.Handlers.Player.Banned += EventHandler.Banned;
		Exiled.Events.Handlers.Player.IssuingMute += EventHandler.Muted;
		Exiled.Events.Handlers.Player.RevokingMute += EventHandler.MuteRevoked;
		Exiled.Events.Handlers.Player.Kicking += EventHandler.Kick;

		base.OnEnabled();

		NormalizeConfigMessages();

		Timing.RunCoroutine(Webhook.SenderLoop());
		Log.Info("init complete");
	}

	public override void OnDisabled()
	{
		Exiled.Events.Handlers.Player.Verified -= EventHandler.Verified;
		Exiled.Events.Handlers.Player.Dying -= EventHandler.Death;
		Exiled.Events.Handlers.Player.Hurting -= EventHandler.Hurting;
		Exiled.Events.Handlers.Player.Banned -= EventHandler.Banned;
		Exiled.Events.Handlers.Player.IssuingMute -= EventHandler.Muted;
		Exiled.Events.Handlers.Player.RevokingMute -= EventHandler.MuteRevoked;
		Exiled.Events.Handlers.Player.Kicking -= EventHandler.Kick;

		base.OnDisabled();
		Log.Info("o7");
	}

	// Changes the loaded config webhook message templates to ones that make more
	// sense programatically but are inconvenient for end users :]
	// Note: this is only called once, at plugin load.
	// ex:
	// 	In the PlayerJoinMsg option: %Nick% -> %PlayerA_Nick%
	void NormalizeConfigMessages()
	{
		// TODO: replace kill stuff you know what to do 

		Func<string, string> GenericReplace = 
			msg => msg
				.Replace("%Player%", "%PlayerA_Nick%")
				.Replace("%ID%", "%PlayerA_ID%")
				.Replace("%Role%", "%PlayerA_Role%");

		this.Config.JoinWebhookMsg = GenericReplace(this.Config.JoinWebhookMsg);
		this.Config.SuicideWebhookMsg = GenericReplace(this.Config.SuicideWebhookMsg);

		Func<string, string> VictimAttackerReplace = 
			msg => msg
				.Replace("%Victim%", "%PlayerA_Nick%")
				.Replace("%Victim_ID%", "%PlayerA_ID%")
				.Replace("%Victim_Role%", "%PlayerA_Role%")
				.Replace("%Attacker%", "%PlayerB_Nick%")
				.Replace("%Attacker_ID%", "%PlayerB_ID%")
				.Replace("%Attacker_Role%", "%PlayerB_Role%");

		this.Config.AttackerDamageMsg = VictimAttackerReplace(this.Config.AttackerDamageMsg);
		this.Config.VictimDamageMsg = VictimAttackerReplace(this.Config.VictimDamageMsg);
		this.Config.TeamDamageWebhookMsg = VictimAttackerReplace(this.Config.TeamDamageWebhookMsg);
		this.Config.TeamKillWebhookMsg = VictimAttackerReplace(this.Config.TeamKillWebhookMsg);
		this.Config.CuffedKillWebhookMsg = VictimAttackerReplace(this.Config.CuffedKillWebhookMsg);

		Func<string, string> TargetIssuerReplace = 
			msg => msg
				.Replace("%Target%", "%PlayerA_Nick%")
				.Replace("%Target_ID%", "%PlayerA_ID%")
				.Replace("%Target_Role%", "%PlayerA_Role%")
				.Replace("%Issuer%", "%PlayerB_Nick%")
				.Replace("%Issuer_ID%", "%PlayerB_ID%")
				.Replace("%Issuer_Role%", "%PlayerB_Role%");

		this.Config.BanWebhookMsg = TargetIssuerReplace(this.Config.BanWebhookMsg);
		this.Config.MuteWebhookMsg = TargetIssuerReplace(this.Config.MuteWebhookMsg);
		this.Config.KickWebhookMsg = TargetIssuerReplace(this.Config.KickWebhookMsg);

		Log.Info("config normalized");
	}
}
