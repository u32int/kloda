namespace kloda;

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.Events.EventArgs.Player;

using MEC;
using Utf8Json;

public static class Webhook
{
	static HttpClient hc = new();
	public static Queue<DiscordMessage> messageQueue = new Queue<DiscordMessage>();

	static async Task HookDoPost(DiscordMessage msg)
	{
		if (!Kloda.instance.Config.DiscordWebhookEnable)
			return;

		Log.Info("Sending discord webhook message..");
		using HttpResponseMessage response = await hc.PostAsync(msg.WebhookUrl, msg.Content);
		if (!response.IsSuccessStatusCode)
		{
			var jsonResponse = await response.Content.ReadAsStringAsync();			
			Log.Error($"Failed to send discord message via webhook! Got response: {jsonResponse}");
		}
	}

	public static SeparateEmbedList G_EmbedList = new SeparateEmbedList();
	public static CombinedEmbedList G_CombinedList = new CombinedEmbedList();
	
	public static void QueueEmbed(DiscordEmbed embed)
	{
		if (!G_EmbedList.Add(embed))
		{
			StringContent sc = G_EmbedList.ClearIntoStringContent();
			messageQueue.Enqueue(new DiscordMessage(sc, Kloda.instance.Config.DiscordWebhookAdministrativeUrl));
			// The current embed still hasn't been sent, try adding it.
			if (!G_EmbedList.Add(embed))
			{
				Log.Error("Adding an embed to a newly cleared EmbedList failed, this is unexpected and likely a  bug. A message has been lost.");
			}
		}
	}

	/// Add a message to be sent as a combined embed
	public static void QueueCombined(string message)
	{
		if (!G_CombinedList.Add(message))
		{
			StringContent sc = G_CombinedList.ClearIntoStringContent();
			messageQueue.Enqueue(new DiscordMessage(sc, 
								Kloda.instance.Config.DiscordWebhookHurtNotificationsUrl));
			// The current message still hasn't been sent, try adding it.
			if (!G_CombinedList.Add(message))
			{
				Log.Error("Adding a message to a newly cleared CombinedList failed, this is unexpected and likely a bug. A message has been lost.");
			}
		}
	}

	// This is exists just avoid some typing at the call site
	public static void QueueCombinedTemplated(string message, 
			Player? playerA, Player? playerB = null, DamageHandler? dmg = null)
	{
		QueueCombined(Template.Replace(message, playerA, playerB, dmg));
	}

	// Runs in the background and hopefully delegates things to be actually sent..
	public static IEnumerator<float> SenderLoop()
	{
		// TODO: this entire function is a little silly, perhaps an async queue type exists
		// in csharp that would work better here? 
		// We definitely shouldn't be doing this based on a fixed timeout..
		for (;;) 
		{
			// Check if any of the queues have stale messages
			if (G_EmbedList.FirstTimestamp.HasValue && 
			    (DateTime.Now - G_EmbedList.FirstTimestamp.Value).TotalSeconds > 
			    					Kloda.instance.Config.DiscordWebhookQueueFlush)
			{
				Log.Info("Pushing stale embed list..");
				StringContent sc = G_EmbedList.ClearIntoStringContent();
				messageQueue.Enqueue(
						new DiscordMessage(sc, 
							Kloda.instance.Config.DiscordWebhookAdministrativeUrl));
			}

			if (G_CombinedList.FirstTimestamp.HasValue && 
			    (DateTime.Now - G_CombinedList.FirstTimestamp.Value).TotalSeconds > 
			    					Kloda.instance.Config.DiscordWebhookQueueFlush)
			{
				Log.Info("Pushing stale combined list..");
				StringContent sc = G_CombinedList.ClearIntoStringContent();
				messageQueue.Enqueue(
						new DiscordMessage(sc, 
							Kloda.instance.Config.DiscordWebhookHurtNotificationsUrl));
			}

			// Send the first message in queue
			if (messageQueue.Count != 0)
			{
				_ = HookDoPost(messageQueue.Dequeue());
			}

			// manual cooldown in-between requests 
			// (5 seconds by default, don't set it under 2 if you don't wanna get rate limited)
			yield return Timing.WaitForSeconds(Kloda.instance.Config.DiscordWebhookCooldown);
		}
	}
}
