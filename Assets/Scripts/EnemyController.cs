using System;
using UnityEngine;

public class EnemyController : CombatantController
{
	private Action<BattleController, CombatantController> behaviour;

	public void Init(EnemyData data, BattleController battleController) {
		Name = data.name;
		HP = data.maxHp;
		behaviour = EnemyBehaviours.GetBehaviour(data.behaviourIndex);
		Abilities = data.abilities;
		this.battleController = battleController;
		State = CombatantState.IDLE;
	}

	public override void PollForTurn() {
		// Use the enemy behaviour on a random target
		behaviour(battleController, this);
	}
}
