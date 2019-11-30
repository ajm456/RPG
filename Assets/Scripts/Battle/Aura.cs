using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an aura which affects a combatant. See the GDD for a specific
/// definition of what an aura is.
/// </summary>
public class Aura
{
	/* MEMBERS */

	/// <summary>
	/// The combatant who is being affected by this aura.
	/// </summary>
	private CombatantController host;

	/// <summary>
	/// An ordered list of effects to apply for each turn that this aura is
	/// applied.
	/// </summary>
	private List<Effect> effectList;

	/// <summary>
	/// Indicates which effect in <see cref="effectList"/> is to be applied
	/// when <see cref="ResolveAura"/> is next called.
	/// </summary>
	private int turnCounter;

	
	/* METHODS */

	/// <summary>
	/// Creates an Aura object with a given combatant as its bearer.
	/// </summary>
	/// <param name="host">The combatant affected by this aura.</param>
	/// <param name="effectList">The list of effects to apply throughout this aura's duration.</param>
	public Aura(CombatantController host, List<Effect> effectList)
	{
		this.host = host;
		this.effectList = effectList;
		turnCounter = 0;
	}

	
	/// <summary>
	/// Applies the aura's effect for this turn.
	/// </summary>
	public void ResolveAura()
	{
		// If we've reached the end of the effect list, this aura should have
		// been removed from the combatant's auras list
		Debug.Assert(turnCounter < effectList.Count);

		// Apply the effect
		effectList[turnCounter].DoEffect(host);

		// Increment our turn counter
		turnCounter++;
	}
}
