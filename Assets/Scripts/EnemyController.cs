﻿using System;

public class EnemyController : CombatantController
{
	private Action<BattleController, CombatantController> behaviour;

	public void Init(EnemyData data, BattleController battleController) {
		Name = data.name;
		HP = data.maxHp;
		MaxHP = data.maxHp;
		Strength = data.strength;
		behaviour = EnemyBehaviours.GetBehaviour(data.behaviourIndex);
		CalmAbilities = data.calmAbilities;
		DiscordAbilities = data.discordAbilities;
		this.battleController = battleController;
		State = CombatantState.IDLE;
	}

	public override void PollForTurn() {
		// Use the enemy behaviour on a random target
		behaviour(battleController, this);
	}
}
