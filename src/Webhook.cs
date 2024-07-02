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
	public static Queue<string> messageQueue = new Queue<string>();

	static async Task HookDoPost(StringContent jsonContent)
	{
		if (!Kloda.instance.Config.EnableDiscordWebhook)
			return;

		Log.Info("sending discord webhook message");
		using HttpResponseMessage response = await hc.PostAsync(Kloda.instance.Config.DiscordWebhookUrl, jsonContent);
		if (!response.IsSuccessStatusCode)
		{
			var jsonResponse = await response.Content.ReadAsStringAsync();			
			Log.Error($"Failed to send discord message via webhook! Got response: {jsonResponse}");
		}
	}

	// Send webhook message with the msg string verbatim, without template replecement etc
	public static void SendRaw(string msg)
	{
		// subject to change, hence the function wrapper in the first place
		messageQueue.Enqueue(msg);
	}

	public static void SendTemplated(string msg, Player? playerA, Player? playerB = null, DamageHandler? dmg = null)
	{
		SendRaw(Template.Replace(msg, playerA, playerB, dmg));
	}

	// Runs in the background and hopefully delegates things to be actually sent..
	public static IEnumerator<float> SenderLoop()
	{
		// TODO: this entire function is a little silly, perhaps an async queue type exists
		// in csharp that would work better here? 
		// We definitely shouldn't be doing this based on a fixed timeout..
		for (;;) 
		{
			if (messageQueue.Count != 0)
			{
				var msg = messageQueue.Dequeue();

                        	StringContent content = new StringContent(
					Encoding.UTF8.GetString(
						Utf8Json.JsonSerializer.Serialize<object>(new { 
							username = "kloda", 
							content = msg,
						})
					), 
					Encoding.UTF8, "application/json");

				_ = HookDoPost(content);
			}

			// cooldown in-between requests
			yield return Timing.WaitForSeconds(2);
		}
	}
}
