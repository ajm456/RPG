using UnityEngine;
using System.Collections.Generic;

public abstract class CombatantController : MonoBehaviour
{
	public enum CombatantState
	{
		IDLE,
		ATTACKING,
		DEAD,
	}

	protected BattleController battleController;

	/* MEMBERS */

	public string Name {
		get;
		set;
	}

	public int HP {
		get;
		set;
	}
	public int MaxHP {
		get;
		set;
	}
	public List<Ability> CalmAbilities {
		get;
		set;
	}
	public List<Ability> DiscordAbilities {
		get;
		set;
	}

	public CombatantState State {
		get;
		set;
	}

	void Update() {
		if(HP <= 0) {
			State = CombatantState.DEAD;
		}
	}

	public abstract void PollForTurn();
}
