using System.Collections.Generic;
using System;

public enum BehaviourIndex
{
	DO_NOTHING = 0,
	RANDOM_ABILITY = 1
}

public static class EnemyBehaviours
{
	public static void DoNothing(BattleController battleController, CombatantController source)
	{
		battleController.PassTurn();
	}

	public static void RandomAbility(BattleController battleController, CombatantController source)
	{
		// Use a random discord ability on a random player character
		AbilityData ability = source.StrifeAbilities[UnityEngine.Random.Range(0, source.StrifeAbilities.Count)];
		battleController.ExecuteTurnWithAbilityOnRandomTarget(ability);
	}

	public static Dictionary<BehaviourIndex, Action<BattleController, CombatantController>> behaviourDict = new Dictionary<BehaviourIndex, Action<BattleController, CombatantController>>() {
		{ BehaviourIndex.RANDOM_ABILITY, RandomAbility },
		{ BehaviourIndex.DO_NOTHING, DoNothing }
	};


	public static Action<BattleController, CombatantController> GetBehaviour(BehaviourIndex index)
	{
		return behaviourDict[index];
	}

	public static Action<BattleController, CombatantController> GetBehaviour(int index)
	{
		return behaviourDict[(BehaviourIndex)index];
	}
}