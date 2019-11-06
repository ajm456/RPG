using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private BattleController battleController;
	[SerializeField] private UIController uiController;

	private CombatantController controller;
	private int calm, discord;

	private void Start() {
		controller = GetComponent<CombatantController>();

		calm = 0;
		discord = 0;
	}

	void Update() {
		// If it's our turn, let's take it
		if(battleController.State == BattleController.BattleState.PLAYERCHOICE) {
			if(Input.GetKeyDown(KeyCode.Space)) {
				// Determine player input
				int choiceId = uiController.GetChoice();

				// Request turn execution
				battleController.ExecuteTurn(controller, choiceId);
			}
		}
		// Did we win?
		else if(battleController.State == BattleController.BattleState.PLAYERWON) {
			Debug.Log("The player won! Good job :¬)");
		}
		// Did we... lose...? :(
		else if(battleController.State == BattleController.BattleState.ENEMYWON) {
			Debug.Log("The player lost :^(");
			Application.Quit(0);
		}
	}
}
