using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
	/* ENUMS AND STRUCTS */

	// State enum
	public enum BattleState
	{
		PLAYERCHOICE,
		ENEMYCHOICE,
		PLAYERWON,
		ENEMYWON,
	}




	/* STATIC MEMBERS */
	public static Dictionary<int, Ability> playerTurnChoices;
	public static Dictionary<string, Ability> abilities;





	/* MEMBERS */

	// BattleController's state
	public BattleState State {
		get;
		set;
	}

	// Player and enemy CombatantController refs
	[SerializeField] private CombatantController[] combatantsArray;
	private List<CombatantController> combatants;





	/* OVERRIDES */

	void Start() {
		State = BattleState.PLAYERCHOICE;
		combatants = new List<CombatantController>(combatantsArray);
		abilities = JsonParser.LoadAllAbilities();

		// Build the dictionary of player turn choices
		playerTurnChoices = new Dictionary<int, Ability>();
		// Bottom-left: Discord-spending attack
		playerTurnChoices.Add(0, abilities["Rend"]);
		// Bottom-right: Calm-building buff
		playerTurnChoices.Add(1, abilities["Reflect"]);
		// Top-left: Normal attack
		playerTurnChoices.Add(2, abilities["Attack"]);
		// Top-right: Calm-spending heal
		playerTurnChoices.Add(3, abilities["Relaxer"]);
	}





	/* METHODS */

	public void ExecuteTurn(CombatantController source, int choiceId, string targetName) {
		// Sanity check the correct combatant is taking their turn
		Debug.Assert(State == BattleState.PLAYERCHOICE && source.Name == "player"
				|| State == BattleState.ENEMYCHOICE && source.Name == "frog");

		// Sanity check that the given key has an entry in the dictionary
		Debug.Assert(playerTurnChoices.ContainsKey(choiceId));
		if(!playerTurnChoices.ContainsKey(choiceId)) {
			return;
		}


		// Apply the ability to its target
		Ability ability = playerTurnChoices[choiceId];
		Debug.Log("Executing ability from " + source.Name + ": " + ability.ToString());
		// Find the target
		CombatantController target = combatants.Find(comb => comb.Name == targetName);
		Debug.Assert(target != null);
		// Damage/heal them
		target.CurrHP += Random.Range(ability.hpAdjMin, ability.hpAdjMax+1);
		// Adjust source resource values
		source.CurrCalm += ability.calmAdj;
		source.CurrDiscord += ability.discordAdj;


		// Modify our current battle state
		Transition();
		// DEBUG - For now, immediately return to player turn
		Transition();
	}

	private void Transition() {
		switch(State) {
			case BattleState.PLAYERCHOICE:
				State = BattleState.ENEMYCHOICE;
				break;

			case BattleState.ENEMYCHOICE:
				State = BattleState.PLAYERCHOICE;
				break;
		}
	}
}
