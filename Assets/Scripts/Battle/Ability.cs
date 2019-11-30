using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents abilities that combatant's can use in a battle. See the GDD for
/// a specific definition of what an ability is.
/// </summary>
public class Ability
{
	public string name;
	public bool isCalm;
	public int calmReq, discordReq;
	public List<Effect> effects;
	public List<Aura> auras;

	public Ability(string name, bool isCalm, int calmReq, int discordReq, List<Effect> effects, List<Aura> auras)
	{
		this.name = name;
		this.isCalm = isCalm;
		this.calmReq = calmReq;
		this.discordReq = discordReq;
		this.effects = effects;
		this.auras = auras;

		// Abilities shouldn't have no effects or auras
		Debug.Assert(effects.Count > 0 || auras.Count > 0);
	}
}