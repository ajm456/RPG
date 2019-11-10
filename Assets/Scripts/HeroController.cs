using TMPro;
using UnityEngine;

public class HeroController : CombatantController
{
	[SerializeField] private TextMeshProUGUI hpText, calmText, discordText;

	public int Calm {
		get;
		set;
	}
	public int Discord {
		get;
		set;
	}
	public int PartyOrder {
		get;
		set;
	}

	public void Init(HeroData data, BattleController battleController) {
		Name = data.name;
		HP = data.hp;
		MaxHP = data.maxHp;
		Abilities = data.abilities;
		Calm = data.calm;
		Discord = data.discord;
		this.battleController = battleController;
		State = CombatantState.IDLE;
	}

	void Update() {
		if(State != CombatantState.DEAD) {
			hpText.SetText(HP.ToString());
			calmText.SetText(Calm.ToString());
			discordText.SetText(Discord.ToString());
		}

		if(HP <= 0) {
			State = CombatantState.DEAD;
			Canvas canvasChild = GetComponentInChildren<Canvas>();
			canvasChild.enabled = false;
		}
	}

	// Discern which ability the player is using, ensure they have sufficient
	// resources, etc.
	public override void PollForTurn() {
		if(Input.GetKeyDown(KeyCode.Space)) {
			// Work out which ability the player is using
			Ability ability = DetermineAbility();

			// Determine player target
			CombatantController target = DetermineTarget();

			// Execute turn
			battleController.ExecuteTurn(this, ability, target);
			// Adjust source resource values
			Calm += ability.calmAdj;
			Discord += ability.discordAdj;
			Calm = Mathf.Clamp(Calm, 0, 100);
			Discord = Mathf.Clamp(Discord, 0, 100);

			// Clamp resource values
			Calm = Mathf.Max(0, Calm);
			Discord = Mathf.Max(0, Discord);
		}
	}

	private Ability DetermineAbility() {
		return Abilities[0];
	}
	
	private CombatantController DetermineTarget() {
		return battleController.EnemyCombatants[0];
	}
}
