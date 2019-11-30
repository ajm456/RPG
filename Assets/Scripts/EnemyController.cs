using System;
using System.Collections.Generic;

public class EnemyController : CombatantController
{
	private Action<BattleController, CombatantController> behaviour;

	public void Init(EnemyData data, BattleController battleController)
	{
		Name = data.name;
		HP = data.maxHp;
		MaxHP = data.maxHp;
		Strength = data.strength;
		Agility = data.agility;
		behaviour = EnemyBehaviours.GetBehaviour(data.behaviourIndex);
		CalmAbilities = data.calmAbilities;
		DiscordAbilities = data.discordAbilities;
		this.battleController = battleController;
		AnimState = AnimationState.IDLE;
		ActiveAuras = new List<Aura>();
	}

	public void DoTurn()
	{
		// Use the enemy behaviour on a random target
		behaviour(battleController, this);
	}
}
