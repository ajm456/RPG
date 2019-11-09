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





	/* MEMBERS */

	// BattleController's state
	public BattleState State {
		get;
		set;
	}

	// Player and enemy CombatantController refs
	[SerializeField] private List<CombatantController> playerCombatants, enemyCombatants;

	// Points to the index of the combatant in playerCombantants or enemyCombatants whose turn it is
	private float combatantTurnIndex;





	/* OVERRIDES */

	void Start() {
		LoadAbilityData();
		InitTurnOrder();
	}





	/* METHODS */

	public void ExecuteTurn(CombatantController source, int choiceId, string targetName) {
		// Sanity check the correct combatant is taking their turn
		Debug.Assert(State == BattleState.PLAYERCHOICE && playerCombatants.Exists(comb => comb.Name == source.Name)
				|| State == BattleState.ENEMYCHOICE && enemyCombatants.Exists(comb => comb.Name == targetName));

		// Sanity check that the given key has an entry in the dictionary
		Debug.Assert(playerTurnChoices.ContainsKey(choiceId));
		if(!playerTurnChoices.ContainsKey(choiceId)) {
			Debug.Log("Trying to execute ability " + choiceId + " but not found in dictionary!");
			return;
		}

		// Find the ability
		Ability ability = playerTurnChoices[choiceId];
		Debug.Log("Executing ability from " + source.Name + ": " + ability.ToString());
		// Find the target
		CombatantController target = enemyCombatants.Find(comb => comb.Name == targetName);
		Debug.Assert(target != null);
		// Execute it
		ExecuteAbility(ability, source, target);

		// Modify our current battle state
		Transition();
	}

	private void InitTurnOrder() {
		// Start with player turn, first character
		State = BattleState.PLAYERCHOICE;
		combatantTurnIndex = 0;
	}

	private void LoadAbilityData() {
		// Load from JSON files
		Dictionary<string, Ability> abilities = JsonParser.LoadAllAbilities();

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

	private void ExecuteAbility(Ability ability, CombatantController source, CombatantController target) {
		// Damage/heal them
		target.CurrHP += Random.Range(ability.hpAdjMin, ability.hpAdjMax+1);
		// Adjust source resource values
		source.CurrCalm += ability.calmAdj;
		source.CurrDiscord += ability.discordAdj;

		// Clamp resource values
		target.CurrHP = Mathf.Max(0, target.CurrHP);
		source.CurrCalm = Mathf.Max(0, source.CurrCalm);
		source.CurrDiscord = Mathf.Max(0, source.CurrDiscord);
	}

	private void Transition() {
		// DEBUG - for now, keep state in player's turn
		State = BattleState.PLAYERCHOICE;
	}
}
