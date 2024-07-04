namespace kloda;

using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.Events.EventArgs.Player;

public class Template
{
	// generic template replacement extracted to reduce code duplication
	public static string Replace(string msg, Player? playerA, Player? playerB = null, DamageHandler? dmg = null)
	{
		string templated = msg
				.Replace("%Time%", DateTime.Now.ToString());

		if (playerA is Player pA)
		{
			templated = templated
			  .Replace("%PlayerA_Nick%", pA.Nickname)
			  .Replace("%PlayerA_ID%", pA.RawUserId)
			  .Replace("%PlayerA_Role%", pA.Role.Type.ToString());
		}

		if (playerB is Player pB)
		{
			templated = templated
			  .Replace("%PlayerB_Nick%", pB.Nickname)
			  .Replace("%PlayerB_ID%", pB.RawUserId)
			  .Replace("%PlayerB_Role%", pB.Role.Type.ToString());
		}

		if (dmg is DamageHandler dh)
		{
			templated = templated
				.Replace("%DamageType%", dmg.Type.ToString());
		}

		return templated;
	}
}
