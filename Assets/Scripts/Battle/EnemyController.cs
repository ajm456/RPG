using System;
using System.Collections.Generic;

public class EnemyController : CombatantController
{
	private Action<BattleController, CombatantController> behaviour;

	public void Init(EnemyData data, BattleController battleController, int battleID)
	{
		Name = data.name;
		BattleID = battleID;
		Allegiance = CombatantAllegiance.ENEMY;
		HP = data.maxHp;
		MaxHP = data.maxHp;
		Strength = data.strength;
		Agility = data.agility;
		behaviour = EnemyBehaviours.GetBehaviour(data.behaviourIndex);
		CalmAbilities = data.calmAbilities;
		StrifeAbilities = data.strifeAbilities;
		this.battleController = battleController;
		AnimState = AnimationState.IDLE;
		ActiveAuraCasterPairs = new List<KeyValuePair<CombatantController, AuraData>>();
	}

	public void DoTurn()
	{
		// Use the enemy behaviour on a random target
		behaviour(battleController, this);
	}
}
