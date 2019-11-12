using System.Collections.Generic;
using System;

public enum BehaviourIndex
{
	RANDOM_ABILITY = 0
}

public static class EnemyBehaviours
{
	public static void RandomAbility(BattleController battleController, CombatantController source) {
		// Use a random discord ability on a random player character
		List<HeroController> playerCharacters = battleController.HeroCombatants;
		CombatantController target = playerCharacters[UnityEngine.Random.Range(0, playerCharacters.Count)];
		Ability ability = source.DiscordAbilities[UnityEngine.Random.Range(0, source.DiscordAbilities.Count)];
		battleController.ExecuteTurn(source, ability, target);
	}

	public static Dictionary<BehaviourIndex, Action<BattleController, CombatantController>> behaviourDict = new Dictionary<BehaviourIndex, Action<BattleController, CombatantController>>() {
		{ BehaviourIndex.RANDOM_ABILITY, RandomAbility }
	};


	public static Action<BattleController, CombatantController> GetBehaviour(BehaviourIndex index) {
		return behaviourDict[index];
	}

	public static Action<BattleController, CombatantController> GetBehaviour(int index) {
		return behaviourDict[(BehaviourIndex)index];
	}
}