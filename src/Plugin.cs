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
	public bool EnableDiscordWebhook { get; set; } = false;

	// Damage 
	public bool IgnoreDamageAfterRoundEnd { get; set; } = true;
	public bool DamageWebhookEnable { get; set; } = true;
	public string DamageWebhookMsg { get; set; } = "[%Time%] %Attacker% damaged their teammate %Victim%";
	public bool NotifyAttacker { get; set; } = true;
	public string AttackerDamageMsg { get; set; } = "<color=red>Warning</color> You've attacked your teammate";
	public bool NotifyVictim { get; set; } = false;
	public string VictimDamageMsg { get; set; } = "<color=red>Warning</color> Attacked by teammate";

	// Death
	public bool IgnoreDeathAfterRoundEnd { get; set; } = true;
	public bool TeamkillWebhookEnable { get; set; } = true;
	public string TeamkillWebhookMsg { get; set; } = "[%Time%] %Victim% teamkilled by %Attacker%";
	public bool CuffedKillWebhookEnable { get; set; } = true;
	public string CuffedKillWebhookMsg { get; set; } = 
		"[%Time%] %Victim% was killed by %Attacker% using %DamageType%, while %Victim% was handcuffed";
	public bool SuicideWebhookEnable { get; set; } = true;
	public string SuicideWebhookMsg { get; set; } = "[%Time%] %Player% died by suicide using %DamageType% :((";

	// Bans
	public bool BanWebhookEnable { get; set; } = true;
	public string BanWebhookMsg { get; set; } = "[%Time%] %Target% was banned by %Issuer%.";

	// Mutes
	public bool MuteWebhookEnable { get; set; } = true;
	public string MuteWebhookMsg { get; set; } = "[%Time%] %Target% was muted.";

	// Player verify (server join)
	public bool JoinWebhookEnable { get; set; } = true;
	public string JoinWebhookMsg { get; set; } = "[%Time%] %Player% joined the server!";
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

		base.OnDisabled();
		Log.Info("o7");
	}

	// Changes the loaded config webhook message templates to ones that make more
	// sense programatically but are inconvenient for end users :]
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

		Log.Info("config normalized");
	}
}
