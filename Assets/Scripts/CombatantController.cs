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


	/// <summary>
	/// Defines whether this combatant is player- or enemy-allied.
	/// </summary>
	public enum CombatantAllegiance
	{
		PLAYER,
		ENEMY,
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
	/// The allegiance of this combatant i.e. player or enemy.
	/// </summary>
	public CombatantAllegiance Allegiance
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
	public List<AbilityData> CalmAbilities
	{
		get;
		set;
	}
	/// <summary>
	/// All Discord abilities this combatant can use.
	/// </summary>
	public List<AbilityData> DiscordAbilities
	{
		get;
		set;
	}
	public List<KeyValuePair<CombatantController, AuraData>> ActiveAuraCasterPairs
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
	/// <param name="caster">The CombatantController object applying the aura.</param>
	public void AddAura(AuraData aura, CombatantController caster)
	{
		ActiveAuraCasterPairs.Add(new KeyValuePair<CombatantController, AuraData>(caster, aura));
	}


	/// <summary>
	/// Resolves each active aura currently affecting this combatant.
	/// </summary>
	public void ResolveAuras()
	{
		Debug.Log("Resolving combatant " + Name + "'s auras");

		// Remove all expired auras
		for (var i = ActiveAuraCasterPairs.Count - 1; i >= 0; --i)
		{
			if (ActiveAuraCasterPairs[i].Value.effectTurnIndex >= ActiveAuraCasterPairs[i].Value.effects.Count)
			{
				Debug.Log("Removing expired aura " + ActiveAuraCasterPairs[i].Value.name + " from combatant " + Name);

				ActiveAuraCasterPairs.RemoveAt(i);
			}
		}

		// Apply this turn's effect for each aura
		foreach (var pair in ActiveAuraCasterPairs)
		{
			EffectData effect = pair.Value.effects[pair.Value.effectTurnIndex];
			ApplyEffect(effect, pair.Key);
			pair.Value.effectTurnIndex += 1;
		}
	}

	public void ApplyEffect(EffectData effect, CombatantController source)
	{
		Debug.Log("Applying effect " + effect.name + " to combatant " + Name);

		string statStr = effect.stat.ToLowerInvariant();
		if (statStr == "hp")
		{
			// Calculate the magnitude of this effect
			int magnitude = effect.amount;

			// Scale it with strength
			if (magnitude > 0)
				magnitude = (int)(magnitude + (source.Strength * effect.strengthScaling));
			else if (magnitude < 0)
				magnitude = (int)(magnitude - (source.Strength * effect.strengthScaling));

			// Calculate if it crit or not
			if (effect.canCrit)
			{
				// Crit chance is a 3% base plus an amount based on agility
				float critChance = 3.0f + 0.3f*source.Agility;
				critChance /= 100.0f;

				// Roll and see if this effect is critting
				if (Random.value >= 1.0f - critChance)
				{
					Debug.Log(source.Name + "'s " + effect.name + " effect crit!");
					magnitude *= 2;
				}
			}
			
			// Apply the effect
			HP += magnitude;
		}
		else
		{
			Debug.Log("Unsupported effect type received!");
			Debug.Break();
		}
	}
}
