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

	public void Init(HeroData data, BattleController battleController, int battleID)
	{
		Name = data.name;
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
		AnimState = AnimationState.IDLE;
		ActiveAuraCasterPairs = new List<KeyValuePair<CombatantController, AuraData>>();
	}
}
