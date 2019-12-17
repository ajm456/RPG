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
	public int Discord
	{
		get;
		set;
	}

	public void Init(HeroData data, BattleController battleController)
	{
		Name = data.name;
		Allegiance = CombatantAllegiance.PLAYER;
		Color = data.color;
		HP = data.hp;
		MaxHP = data.maxHp;
		Strength = data.strength;
		Agility = data.agility;
		CalmAbilities = data.calmAbilities;
		DiscordAbilities = data.discordAbilities;
		Calm = data.calm;
		Discord = data.discord;
		this.battleController = battleController;
		AnimState = AnimationState.IDLE;
		ActiveAuras = new List<AuraData>();
	}
}
