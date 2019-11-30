using UnityEngine;
using System.Collections.Generic;

public abstract class CombatantController : MonoBehaviour
{
	/// <summary>
	/// Defines the current animation state of a combatant i.e. idle animation,
	/// dead, moving towards an enemy, attack animation, etc.
	/// </summary>
	public enum AnimationState
	{
		IDLE,
		ATTACKING,
		DEAD,
	}



	/* MEMBERS */

	/// <summary>
	/// Reference to the BattleController object for this battle.
	/// </summary>
	protected BattleController battleController;
	// TODO: Is this really necessary? It's kind of grim to have a reference to
	// the entire battle controller, considering it has a reference to this.

	
	/* Combatant properties */

	/// <summary>
	/// The character name of this combatant.
	/// </summary>
	public string Name
	{
		get;
		set;
	}
	/// <summary>
	/// The current HP of this combatant.
	/// </summary>
	public int HP
	{
		get;
		set;
	}
	/// <summary>
	/// The maximum possible HP in battle for this combatant.
	/// </summary>
	public int MaxHP
	{
		get;
		set;
	}
	/// <summary>
	/// The strength of this combatant.
	/// </summary>
	public int Strength
	{
		get;
		set;
	}
	/// <summary>
	/// The agility of this combatant.
	/// </summary>
	public int Agility
	{
		get;
		set;
	}
	/// <summary>
	/// All Calm abilities this combatant can use.
	/// </summary>
	public List<Ability> CalmAbilities
	{
		get;
		set;
	}
	/// <summary>
	/// All Discord abilities this combatant can use.
	/// </summary>
	public List<Ability> DiscordAbilities
	{
		get;
		set;
	}
	/// <summary>
	/// A list of all currently active auras on this combatant.
	/// </summary>
	public List<Aura> ActiveAuras
	{
		get;
		set;
	}


	/// <summary>
	/// The current animation state of this combatant.
	/// </summary>
	public AnimationState AnimState
	{
		get;
		set;
	}



	/* METHODS */

	void Update()
	{
		// If we're dead, switch to the dead animation
		if (HP <= 0)
		{
			AnimState = AnimationState.DEAD;
		}
	}


	/// <summary>
	/// Adds an aura to this combatant's list of currently active auras.
	/// </summary>
	/// <param name="aura">The Aura object being added to this combatant's aura list.</param>
	public void AddAura(Aura aura)
	{
		ActiveAuras.Add(aura);
	}


	/// <summary>
	/// Resolves each active aura currently affecting this combatant.
	/// </summary>
	public void ResolveAuras()
	{
		foreach (Aura aura in ActiveAuras)
		{
			aura.ResolveAura();
		}
	}
}
