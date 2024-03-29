﻿using UnityEngine;
using System.Collections.Generic;

public abstract class CombatantController : MonoBehaviour
{
	/// <summary>
	/// Defines whether this combatant is player- or enemy-allied.
	/// </summary>
	public enum CombatantAllegiance
	{
		PLAYER,
		ENEMY,
	}


	/* CONSTANTS */

	/// <summary>
	/// The base percentage chance an effect has to crit.
	/// </summary>
	private static readonly float BASE_CRIT_CHANCE = 3f;

	/// <summary>
	/// The amount a combatant's agility is scaled by before being added to the
	/// crit chance of an effect.
	/// </summary>
	private static readonly float CRIT_AGIL_SCALING = 0.3f;




	/* MEMBERS */

	/// <summary>
	/// Reference to the BattleController object for this battle.
	/// </summary>
	protected BattleController battleController;
	// TODO: Is this really necessary? It's kind of grim to have a reference to
	// the entire battle controller, considering it has a reference to this.

	protected Animator animator;

	
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
	/// A unique-to-this-battle identifier for this combatant.
	/// </summary>
	public int BattleID
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
	/// All Strife abilities this combatant can use.
	/// </summary>
	public List<AbilityData> StrifeAbilities
	{
		get;
		set;
	}
	public List<KeyValuePair<CombatantController, AuraData>> ActiveAuraCasterPairs
	{
		get;
		set;
	}



	/* METHODS */


	private void Start()
	{
		animator = GetComponent<Animator>();
	}


	/// <summary>
	/// Adds an aura to this combatant's list of currently active auras.
	/// </summary>
	/// <param name="aura">The Aura object being added to this combatant's aura list.</param>
	/// <param name="caster">The CombatantController object applying the aura.</param>
	public void AddAura(AuraData aura, CombatantController caster)
	{
		Debug.Log("Adding aura " + aura.name + " to combatant " + Name + "[" + BattleID + "]");
		ActiveAuraCasterPairs.Add(new KeyValuePair<CombatantController, AuraData>(caster, aura));
	}


	/// <summary>
	/// Resolves each active aura currently affecting this combatant.
	/// </summary>
	public void ResolveAuras()
	{
		Debug.Log("Resolving combatant " + Name + "[" + BattleID + "]'s auras");

		// Remove all expired auras
		for (var i = ActiveAuraCasterPairs.Count - 1; i >= 0; --i)
		{
			if (ActiveAuraCasterPairs[i].Value.effectTurnIndex >= ActiveAuraCasterPairs[i].Value.effects.Count)
			{
				Debug.Log("Removing expired aura " + ActiveAuraCasterPairs[i].Value.name + " from combatant " + Name + "[" + BattleID + "]");

				ActiveAuraCasterPairs.RemoveAt(i);
			}
		}

		// Apply this turn's effect for each aura
		foreach (var pair in ActiveAuraCasterPairs)
		{
			EffectData effect = pair.Value.effects[pair.Value.effectTurnIndex];
			Debug.Log("From aura " + pair.Value.name + "...");
			// TODO: Floating text for resolving auras
			string floatingText = "";
			ApplyEffect(effect, pair.Key, ref floatingText);
			pair.Value.effectTurnIndex += 1;
		}
	}

	public void ApplyEffect(EffectData effect, CombatantController source, ref string floatingText)
	{
		if (effect.IsEmpty())
		{
			Debug.Log("Empty effect - skipping");
			return;
		}

		Debug.Log("Applying effect " + effect.name + " to combatant " + Name + "[" + BattleID + "]");

		string statStr = effect.stat.ToLowerInvariant();
		if (statStr == "hp")
		{
			// Calculate the magnitude of this effect
			int magnitude = effect.amount;

			// Scale it with strength
			if (magnitude > 0)
			{
				magnitude = (int)(magnitude + (source.Strength * effect.strengthScaling));
			}
			else if (magnitude < 0)
			{
				magnitude = (int)(magnitude - (source.Strength * effect.strengthScaling));
			}

			// Calculate if it crit or not
			if (effect.canCrit)
			{
				// Crit chance is a 3% base plus an amount based on agility
				float critChance = BASE_CRIT_CHANCE + (CRIT_AGIL_SCALING * source.Agility);
				critChance /= 100.0f;

				// Roll and see if this effect is critting
				if (Random.value >= 1.0f - critChance)
				{
					Debug.Log(source.Name + "'s " + effect.name + " effect crit!");
					magnitude *= 2;
				}
			}

			// Set the floating combat text string with the damage
			floatingText = Mathf.Abs(magnitude).ToString();
			
			// Apply the effect
			HP = Mathf.Max(HP + magnitude, 0);
			if (magnitude <= 0)
			{
				Debug.Log(Name + "[" + BattleID + "] took " + (-magnitude) + " damage from " + effect.name);
			}
			else
			{
				Debug.Log(Name + "[" + BattleID + "] was healed by " + magnitude + " from " + effect.name);
			}
			Debug.Log("It now has HP " + HP + "/" + MaxHP);
		}
		else if (statStr == "agility")
		{
		 	int magnitude = effect.amount;

			// Calculate if it crit or not
			if (effect.canCrit)
			{
				// Crit chance is a 3% base plus an amount based on agility
				float critChance = BASE_CRIT_CHANCE + (CRIT_AGIL_SCALING * source.Agility);
				critChance /= 100.0f;

				// Roll and see if this effect is critting
				if (Random.value >= 1.0f - critChance)
				{
					Debug.Log(source.Name + "'s " + effect.name + " effect crit!");
					magnitude *= 2;
				}
			}

			// Cannot have less than 1 agility
		 	Agility = Mathf.Max(Agility + magnitude, 1);

			// Since agility has changed, turn order may have too
		 	battleController.RefreshTurnOrder();
		}
		
		else
		{
			Debug.Log("Unsupported effect type received!");
			Debug.Break();
		}
	}

	public void SetAnimBool(string name, bool value)
	{
		animator.SetBool(name, value);
	}

	/// <summary>
	/// Get the duration in seconds of the animation associated with this
	/// combatant with the given name, if one exists.
	/// </summary>
	/// <param name="name">The name of the animation being searched for.</param>
	/// <returns>The duration in seconds of the given animation.</returns>
	public float GetAnimDuration(string name)
	{
		AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip clip in clips)
		{
			if (clip.name == name)
			{
				return clip.length;
			}
		}

		Debug.Log("Could not find animation clip with name " + name);
		Debug.Break();
		return -1f;
	}
}
