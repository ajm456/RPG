using System;
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

	// All the state-modifying data of a turn
	public class TurnData
	{
		public Aura[] auras;

		public TurnData(Aura[] auras) {
			this.auras = auras;
		}

		public override string ToString() {
			return String.Join<Aura>(" , ", auras);
		}
	}

	// Effects (auras)
	public class Aura
	{
		public string targetName;
		public int hpMod;

		public Aura(string targetName, int hpMod) {
			this.targetName = targetName;
			this.hpMod = hpMod;
		}

		public override string ToString() {
			return "[targetName: " + targetName + ", hpMod: " + hpMod.ToString() + "]";
		}
	}




	/* STATIC MEMBERS */
	public static Dictionary<int, TurnData> playerTurnChoices;





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

		// Build the dictionary of player turn choices
		playerTurnChoices = new Dictionary<int, TurnData>();
		// ID 2: Discord-spending attack
		{
			Aura aura = new Aura("frog", -20);
			Aura[] auras = new Aura[] { aura };
			playerTurnChoices.Add(2, new TurnData(auras));
		}
		// ID 3: Calm-spending heal
		{
			Aura aura = new Aura("player", 10);
			Aura[] auras = new Aura[] { aura };
			playerTurnChoices.Add(3, new TurnData(auras));
		}
		
	}





	/* METHODS */

	public void ExecuteTurn(CombatantController combatant, int choiceId) {
		// Sanity check the correct combatant is taking their turn
		Debug.Assert(State == BattleState.PLAYERCHOICE && combatant.Name == "player"
				|| State == BattleState.ENEMYCHOICE && combatant.Name == "frog");

		// Sanity check that the given key has an entry in the dictionary
		Debug.Assert(playerTurnChoices.ContainsKey(choiceId));
		if(!playerTurnChoices.ContainsKey(choiceId)) {
			return;
		}


		// Apply each aura to its target
		TurnData data = playerTurnChoices[choiceId];
		Debug.Log("Executing turn from " + combatant.Name + ": " + data.ToString());
		foreach(Aura aura in data.auras) {
			CombatantController target = combatants.Find(comb => comb.Name == aura.targetName);
			Debug.Assert(target != null);
			target.CurrHP += aura.hpMod;
		}

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
