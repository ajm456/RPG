using System.Collections.Generic;
using UnityEngine;

public class HeroController : CombatantController
{
	public Color Color
	{
		get;
		set;
	}
	public int Calm
	{
		get;
		set;
	}
	public int Strife
	{
		get;
		set;
	}

	/// <summary>
	/// Is this hero the protagonist?
	/// 
	/// Required as the protagonist has some unique interactions with game
	/// mechanics.
	/// </summary>
	public bool IsProtag
	{
		get;
		private set;
	}

	public void Init(HeroData data, BattleController battleController, int battleID)
	{
		Name = data.name;
		// Special combat rules apply to the protag, so flag the controller
		IsProtag = Name.ToLower() == "jack";

		BattleID = battleID;
		Allegiance = CombatantAllegiance.PLAYER;
		Color = data.color;
		HP = data.hp;
		MaxHP = data.maxHp;
		Strength = data.strength;
		Agility = data.agility;
		CalmAbilities = data.calmAbilities;
		StrifeAbilities = data.strifeAbilities;
		Calm = data.calm;
		Strife = data.strife;
		this.battleController = battleController;
		ActiveAuraCasterPairs = new List<KeyValuePair<CombatantController, AuraData>>();
	}
}
