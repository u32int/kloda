namespace kloda;

using System;
using System.Text;
using System.Net;
using System.Net.Http;
using Utf8Json;

public class DiscordEmbed 
{
	public string Content { get; set; }
	public int Color { get; set; }
	public string Footer { get; set; }
	public bool FooterEnable { get; set; }

	public DiscordEmbed(string content, string colorHex = "e6e6e6", bool footerTimestamp = true)
	{
		this.Content = content;	
		this.SetColorHex(colorHex);
		if (footerTimestamp)
		{
			this.Footer = DateTime.Now.ToString();
			this.FooterEnable = true;
		}
		else
		{
			this.Footer = "";
			this.FooterEnable = false;
		}
	}

	public void SetColorHex(string hexColor)
	{
		this.Color = Convert.ToInt32(hexColor, 16);
	}

	public int TextLength()
	{
		return this.Content.Length + this.Footer.Length;
	}

	public object IntoObject() 
	{
		// this is slightly spaghetti-ish but I don't know how to add fields to an existing "object"
		// thing, sorry
		if (this.FooterEnable)
		{
			return new {
				description = this.Content,
				color = this.Color,
				footer = new {
					text = this.Footer,
				}
			};
		} else 
		{
			return new {
				description = this.Content,
				color = this.Color,
			};
		}
	}

	public StringContent IntoStringContent()
	{
                return new StringContent(
			Encoding.UTF8.GetString(
				Utf8Json.JsonSerializer.Serialize<object>(new { 
					username = "kloda", 
					embeds = new List<object>() { this.IntoObject() },
				})
			), 
			Encoding.UTF8, "application/json");
	}
}

/// A list of [DiscordEmbed]s, very roughly keeping track of whether or not the max
/// character count and embed count have been exceeded. If so, Add() will return false
/// and it is up to the caller to potentially call ClearIntoStringContent 
/// which does what is says, combining all the list elements into a single json StringContent
/// ready to be sent as a discord webhook.
public class EmbedList 
{
	public List<object> Embeds { get; set; } = new List<object>();
	public int TotalLength { get; set; } = 0;
	public DateTime? FirstTimestamp { get; set; } = null;

	public bool Add(DiscordEmbed embed)
	{
		int embedTextLen = embed.TextLength();

		// discord limitations
		if (this.Embeds.Count >= 10 || this.TotalLength + embedTextLen >= 5_500)
			return false;

		this.Embeds.Add(embed.IntoObject());
		this.TotalLength += embedTextLen;
		if (Embeds.Count == 1)
			this.FirstTimestamp = DateTime.Now;

		return true;
	}

	public StringContent ClearIntoStringContent()
	{
		StringContent sc = new StringContent(
			Encoding.UTF8.GetString(
				Utf8Json.JsonSerializer.Serialize<object>(new { 
					username = "kloda", 
					embeds = this.Embeds,
				})
			), 
			Encoding.UTF8, "application/json"
		);


		this.Embeds.Clear();
		this.TotalLength = 0;
		this.FirstTimestamp = null;
		return sc;
	}
}

/// Similar to EmbedList but for lower priority messages like team damage etc.
/// Combines messages (roughly up to discord character limit) into a single
/// large embed.
public class CombinedList
{
	public string LogText { get; set; } = "";
	public DateTime? FirstTimestamp { get; set; } = null;

	public bool Add(string message)
	{
		if (LogText.Length + message.Length >= 5_500)
			return false;
		
		if (LogText.Length == 0)
			this.FirstTimestamp = DateTime.Now;
		LogText += "\n" + message;
		return true;
	}

	public StringContent ClearIntoStringContent()
	{
		DiscordEmbed embed = new DiscordEmbed(content: this.LogText, footerTimestamp: false);
		embed.Footer = $"{this.FirstTimestamp} To {DateTime.Now}";
		this.FirstTimestamp = null;
		return embed.IntoStringContent();
	}
}