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
	public List<HeroController> HeroCombatants {
		get;
		private set;
	}
	public List<EnemyController> EnemyCombatants {
		get;
		private set;
	}
	public int HeroTurnIndex {
		get;
		private set;
	}
	
	public int EnemyTurnIndex {
		get;
		private set;
	}




	/* OVERRIDES */

	void Awake() {
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
		Debug.Log("PLAYER TURN");
		Debug.Log("Index: " + HeroTurnIndex);
	}

	void Update() {
		if(State == BattleState.PLAYERCHOICE) {
			// Start polling for the player hero's turn
			HeroCombatants[HeroTurnIndex].PollForTurn();
		} else if(State == BattleState.ENEMYCHOICE) {
			EnemyCombatants[EnemyTurnIndex].PollForTurn();
		}
	}





	/* METHODS */

	public void ExecuteTurn(CombatantController source, Ability ability, CombatantController target) {
		// Try and execute the ability
		ExecuteAbility(ability, source, target);

		// Modify our current battle state
		Transition();
	}

	public void ExecuteTurn() {
		// No turn taken, just modify battle state
		Transition();
	}

	private void InitEncounterData() {
		data = EncounterDataStaticContainer.GetData();
	}
	
	private void SetupHeroes() {
		// Deserialize the heroes we need
		List<HeroData> heroes = JsonParser.LoadHeroes(data.heroNames);

		// Initialise the combatants list
		HeroCombatants = new List<HeroController>();

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
		EnemyCombatants = new List<EnemyController>();

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
		HeroTurnIndex = 0;
		EnemyTurnIndex = 0;
	}

	private void ExecuteAbility(Ability ability, CombatantController source, CombatantController target) {
		// Damage/heal them
		int damage = Random.Range(ability.hpAdjMin, ability.hpAdjMax+1);
		target.HP += damage;
		target.HP = Mathf.Max(0, target.HP);
		Debug.Log(source.Name + " did " + (-damage) + " damage to " + target.Name);
	}

	private void Transition() {
		switch(State) {
			case BattleState.PLAYERCHOICE:
				State = BattleState.ENEMYCHOICE;
				EnemyTurnIndex = 0;
				Debug.Log("ENEMY TURN");
				Debug.Log("Index: " + EnemyTurnIndex);
				break;
			case BattleState.ENEMYCHOICE:
				State = BattleState.PLAYERCHOICE;
				HeroTurnIndex = (HeroTurnIndex + 1) % HeroCombatants.Count;
				Debug.Log("PLAYER TURN");
				Debug.Log("Index: " + HeroTurnIndex);
				break;
			default:
				Debug.Log("Unexpected BattleState!");
				Debug.Break();
				break;
		}
	}
}
