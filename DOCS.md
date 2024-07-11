## kloda config documentation
Config options along with default values
```
kloda:
  is_enabled: true
  debug: false
  broadcast_duration: 5
  time_zone_code: 'Local'
  discord_administrative_webhook_url: ''
  discord_administrative_nickname: 'kloda'
  discord_administrative_avatar_url: 'https://cdn.discordapp.com/attachments/859050843029766177/1258498039132323981/log_n.png?ex=66884322&is=6686f1a2&hm=491ad78b6630aa38e38cd8ba9af22e9d4634bfcd840b0bccd453e87a7d3ebc69'
  discord_administrative_footer_suffix: ''
  discord_hurt_notifications_webhook_url: ''
  discord_hurt_notifications_nickname: 'kloda'
  discord_hurt_notifications_avatar_url: 'https://cdn.discordapp.com/attachments/859050843029766177/1258498039132323981/log_n.png?ex=66884322&is=6686f1a2&hm=491ad78b6630aa38e38cd8ba9af22e9d4634bfcd840b0bccd453e87a7d3ebc69'
  discord_hurt_notifications_footer_suffix: ''
  discord_webhook_enable: false
  discord_webhook_cooldown: 5
  discord_webhook_queue_flush: 10
  embed_join_color: '22c17f'
  embed_ban_color: 'd13934'
  embed_mute_color: 'c68e2b'
  embed_un_mute_color: 'c68e2b'
  embed_kick_color: 'eae738'
  team_harm_role_white_list:
  - ClassD
  ignore_damage_after_round_end: true
  team_damage_webhook_enable: true
  team_damage_webhook_msg: '**%Attacker%** damaged their teammate **%Victim%**'
  notify_attacker: true
  attacker_damage_msg: '<color=red>Warning</color> You''ve attacked your teammate'
  notify_victim: false
  victim_damage_msg: '<color=red>Warning</color> Attacked by teammate'
  ignore_death_after_round_end: true
  team_kill_webhook_enable: true
  team_kill_webhook_msg: '**%Victim%** teamkilled by **%Attacker%**'
  cuffed_kill_webhook_enable: true
  cuffed_kill_webhook_msg: '**%Victim%** was killed by **%Attacker%** using **%DamageType%**, while **%Victim%** was handcuffed'
  suicide_webhook_enable: true
  suicide_webhook_msg: '**%Player%** died by suicide using **%DamageType%** :(('
  ban_webhook_enable: true
  ban_webhook_msg: '**%Target%** was banned by **%Issuer%**. Reason: %Reason%. Duration: %Duration%. Expires: %ExpiryDate%'
  mute_webhook_enable: true
  mute_webhook_msg: '**%Target%** was muted.'
  un_mute_webhook_enable: true
  un_mute_webhook_msg: '**%Target%** was unmuted.'
  intercom_check_returns_bool: true
  kick_webhook_enable: true
  kick_webhook_msg: '**%Target%** was kicked by **%Issuer%**. Reason: `%Reason%`.'
  join_webhook_enable: false
  join_webhook_msg: '**%Player%** joined the server!'
```

## Option explanations
##### BroadcastDuration
Sets the duration of the broadcast displayed as notifications to the attacker/victim of team damage

##### TimeZoneCode
c# TimeZoneInfo time zone code; eg. "Central Standard Time".
Use `Local` for server local time.

#### DiscordWebhooks
[Webhooks](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) urls.
##### DiscordWebhookAdministrativeUrl
Webhook used for adminstrative messages; i.e. bans/kicks/mutes

##### DiscordWebhookHurtNotificationsUrl
Webhook used for information about player-hurting events: teamkills/teamdamage/suicides etc.

##### DiscordWebhookCooldown
Cooldown between webhook messages sent to discord. Don't set it under two or you might run into rate-limits.

##### DiscordWebhookQueueFlush
(In seconds) After this time is exceeded, messages waiting in queue to be sent are considered stale and pushed regardless
of whether or not the current message could fit more embeds.

##### Embed\*Color
Sets the discord embed bar color.

##### AttackerDamageMsg/VictimDamageMsg NotifyAttacker/NotifyVictim
These options control the in-game broadcast notifications.

## Templates
In most of the `_msg` you can use string templates aka this `%Value%` syntax.
For all of these you can use `%DateTime%` to get the timestamp of the event.

**Important**
You can suffix any of `Player` `Victim` `Attacker` `Target` `Issuer` with `_SteamId` or `_Role` to get the player's steamid64 and role respectively.

### Available template arguments
##### team\_damage team\_kill cuffed\_kill
```
%Victim% 
%Attacker% 
%DamageType%
```

##### suicide 
```
%Player% 
%DamageType%
```

##### ban
```
%Target%
%Issuer%
%Reason%
%IssuanceTime%
%ExpiryDate%"
%Duration%"
%Reason%"
```

##### kick
```
%Target%
%Issuer%
%Reason%
```

##### mute
```
%Target%
%IsIntercom%
```
