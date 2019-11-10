using System.Collections.Generic;
using UnityEngine;

public class BattleController : MonoBehaviour
{
	/* ENUMS AND STRUCTS */

	// State enum
	public enum BattleState
	{
		INIT,
		PLAYERCHOICE,
		ENEMYCHOICE,
		PLAYERWON,
		ENEMYWON,
	};





	/* CONSTANTS */

	// The spawn vectors for the Heroes in three-Hero encounters
	private static readonly Vector2[] HERO_SPAWN_POSITIONS = new Vector2[] {
		new Vector2(-0.66f, 0.35f),
		new Vector2(-0.49f, -0.15f),
		new Vector2(-0.87f, -0.59f)
	};

	// The spawn vectors for the enemies in three-enemy encounters
	private static readonly Vector2[] ENEMY_SPAWN_POSITIONS = new Vector2[] {
		new Vector2(0.7f, 0.6f),
		new Vector2(0.85f, 0f),
		new Vector2(1f, -0.6f)
	};
	
	
	
	
	/* MEMBERS */

	[SerializeField] private GameObject heroPrefab, enemyPrefab;

	// BattleController's state
	public BattleState State {
		get;
		set;
	}

	// This encounter's data
	private EncounterData data;

	// Player and enemy CombatantController refs
	public List<CombatantController> HeroCombatants {
		get;
		private set;
	}
	public List<CombatantController> EnemyCombatants {
		get;
		private set;
	}

	// Points to the index of the combatant in playerCombantants or enemyCombatants whose turn it is
	private int combatantTurnIndex;





	/* OVERRIDES */

	void Start() {
		State = BattleState.INIT;

		// DEBUG - Set encounter data from inside scene
		List<string> heroNames = new List<string>() { "jack", "marl", "elise" };
		List<string> enemyNames = new List<string>() { "frog" };
		EncounterDataStaticContainer.SetData(new EncounterData(heroNames, enemyNames));
		// Grab the encounter data (hopefully it was set before loading this scene)
		InitEncounterData();

		// Deserialize Hero/Enemy data and initialise objects
		SetupHeroes();
		SetupEnemies();

		// Discern the turn order
		InitTurnOrder();

		State = BattleState.PLAYERCHOICE;
	}

	void Update() {
		if(State == BattleState.PLAYERCHOICE) {
			// Start polling for the player hero's turn
			HeroCombatants[combatantTurnIndex].PollForTurn();
		} else if(State == BattleState.ENEMYCHOICE) {
			EnemyCombatants[combatantTurnIndex].PollForTurn();
		}
	}





	/* METHODS */

	public void ExecuteTurn(CombatantController source, Ability ability, CombatantController target) {
		Debug.Log(source.ToString() + " is using " + ability.ToString() + " on " + target.ToString() + "!");
		// Try and execute the ability
		ExecuteAbility(ability, source, target);

		// Modify our current battle state
		Transition();
	}

	private void InitEncounterData() {
		data = EncounterDataStaticContainer.GetData();
	}
	
	private void SetupHeroes() {
		// Deserialize the heroes we need
		List<HeroData> heroes = JsonParser.LoadHeroes(data.heroNames);

		// Initialise the combatants list
		HeroCombatants = new List<CombatantController>();

		// Instantiate game objects for the heroes
		for(var i = 0; i < heroes.Count; ++i) {
			GameObject newHero = Instantiate(heroPrefab);
			newHero.transform.localPosition = HERO_SPAWN_POSITIONS[i];
			HeroController heroController = newHero.GetComponent<HeroController>();
			heroController.Init(heroes[i], this);
			heroController.PartyOrder = i;
			HeroCombatants.Add(heroController);
		}
	}

	private void SetupEnemies() {
		// Deserialize the enemies we need
		List<EnemyData> enemies = JsonParser.LoadEnemies(data.enemyNames);

		// Initialise the combatants list
		EnemyCombatants = new List<CombatantController>();

		// Instantiate game objects for the enemies
		for(var i = 0; i < enemies.Count; ++i) {
			GameObject newEnemy = Instantiate(enemyPrefab);
			newEnemy.transform.localPosition = ENEMY_SPAWN_POSITIONS[i];
			EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
			enemyController.Init(enemies[i], this);
			EnemyCombatants.Add(enemyController);
		}
	}

	private void InitTurnOrder() {
		// Start with player turn, first hero
		combatantTurnIndex = 0;
	}

	private void ExecuteAbility(Ability ability, CombatantController source, CombatantController target) {
		// Damage/heal them
		target.HP += Random.Range(ability.hpAdjMin, ability.hpAdjMax+1);
		target.HP = Mathf.Max(0, target.HP);
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
