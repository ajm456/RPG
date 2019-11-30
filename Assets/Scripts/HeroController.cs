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
	public int PartyOrder
	{
		get;
		set;
	}

	public void Init(HeroData data, BattleController battleController)
	{
		Name = data.name;
		Color = data.color;
		HP = data.hp;
		MaxHP = data.maxHp;
		Strength = data.strength;
		CalmAbilities = data.calmAbilities;
		DiscordAbilities = data.discordAbilities;
		Calm = data.calm;
		Discord = data.discord;
		this.battleController = battleController;
		AnimState = AnimationState.IDLE;
		ActiveAuras = new List<Aura>();
	}

	private Ability DetermineAbility()
	{
		return DiscordAbilities[0];
	}

	private CombatantController DetermineTarget()
	{
		return battleController.EnemyCombatants[0];
	}
}
