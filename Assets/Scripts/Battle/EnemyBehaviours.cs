using System.Collections.Generic;
using System;

public enum BehaviourIndex
{
	DO_NOTHING = 0,
	ATTACK_RANDOM = 1,
	RANDOM_ABILITY = 2
}

public static class EnemyBehaviours
{
	public static void DoNothing(BattleController battleController, CombatantController source)
	{
		battleController.PassTurn();
	}

	public static void AttackRandom(BattleController battleController, CombatantController source)
	{
		battleController.ExecuteTurnWithAttack(source.BattleID, UnityEngine.Random.Range(0, battleController.GetNumHeroes()));
	}

	public static void RandomAbility(BattleController battleController, CombatantController source)
	{
		// Use a random strife ability on a random player character
		AbilityData ability = source.StrifeAbilities[UnityEngine.Random.Range(0, source.StrifeAbilities.Count)];
		battleController.ExecuteTurnWithAbilityOnRandomTarget(ability);
	}

	public static Dictionary<BehaviourIndex, Action<BattleController, CombatantController>> behaviourDict = new Dictionary<BehaviourIndex, Action<BattleController, CombatantController>>() {
		{ BehaviourIndex.DO_NOTHING, DoNothing },
		{ BehaviourIndex.ATTACK_RANDOM, AttackRandom },
		{ BehaviourIndex.RANDOM_ABILITY, RandomAbility }
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