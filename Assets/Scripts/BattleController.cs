using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Manages the overall state and actions in a battle.
/// </summary>
public class BattleController : MonoBehaviour
{
	/* ENUMS AND STRUCTS */

	/// <summary>
	/// Represents the current battle state.
	/// </summary>
	public enum BattleState
	{
		INIT,
		PLAYERCHOICE,
		ENEMYCHOICE,
		PLAYERWON,
		ENEMYWON,
	};





	/* CONSTANTS */

	/// <summary>
	/// The spawn vectors for the heroes if there are 3 heroes combatants.
	/// </summary>
	private static readonly Vector2[] HERO_SPAWN_POSITIONS = new Vector2[] {
		new Vector2(-0.66f, 0.35f),
		new Vector2(-0.49f, -0.15f),
		new Vector2(-0.87f, -0.59f)
	};

	/// <summary>
	/// The spawn vectors for enemies if there are 3 enemy combatants.
	/// </summary>
	private static readonly Vector2[] ENEMY_SPAWN_POSITIONS = new Vector2[] {
		new Vector2(0.7f, 0.6f),
		new Vector2(0.85f, 0f),
		new Vector2(1f, -0.6f)
	};




	/* MEMBERS */

	[SerializeField] private GameObject heroPrefab, enemyPrefab;

	/// <summary>
	/// Current state of the battle.
	/// </summary>
	public BattleState State
	{
		get;
		set;
	}

	/// <summary>
	/// All the data required to initialise this battle (e.g. hero combatants,
	/// enemy combatants...). This must be set before loading this scene!
	/// </summary>
	private EncounterData data;

	/// <summary>
	/// Holds a ref to each hero combatant's controller in this battle.
	/// </summary>
	public List<HeroController> HeroCombatants
	{
		get;
		private set;
	}

	/// <summary>
	/// Holds a ref to each enemy combatant's controller in this battle.
	/// </summary>
	public List<EnemyController> EnemyCombatants
	{
		get;
		private set;
	}

	/// <summary>
	/// The index of the hero whose turn it is currently.
	/// </summary>
	public int HeroTurnIndex
	{
		get;
		private set;
	}

	/// <summary>
	/// The index of the enemy whose turn it is currently.
	/// </summary>
	public int EnemyTurnIndex
	{
		get;
		private set;
	}




	/* OVERRIDES */

	void Awake()
	{
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

		Transition();
	}





	/* METHODS */

	/// <summary>
	/// Executes a turn from a give combatant by attacking a given target.
	/// </summary>
	/// <param name="source">The combatant taking the turn.</param>
	/// <param name="target">The combatant being attacked.</param>
	public void ExecuteTurnWithAttack(CombatantController source, CombatantController target)
	{
		// Check that it really is the turn of the combatant executing this turn
		if (!VerifyCombatantTurn(source))
		{
			Debug.Log("Someone tried to execute a turn when it wasn't their turn!");
			Debug.Break();
			return;
		}

		// Execute attack
		DoAttack(source, target);

		// Modify our current battle state
		Transition();
	}

	/// <summary>
	/// Executes a turn from a given combatant with a given ability on a given
	/// target combatant.
	/// </summary>
	/// <param name="ability">The ability the source combatant is using.</param>
	/// <param name="source">The combatant taking the turn.</param>
	/// <param name="target">The combatant receiving the ability.</param>
	public void ExecuteTurnWithAbility(AbilityData ability, CombatantController source, CombatantController target)
	{
		// Check that it really is the turn of the combatant executing this turn
		if (!VerifyCombatantTurn(source))
		{
			Debug.Log("Someone tried to execute a turn when it wasn't their turn!");
			Debug.Break();
			return;
		}

		// Try and execute the ability
		DoAbility(ability, source, target);

		// Modify our current battle state
		Transition();
	}

	public void PassTurn()
	{
		Debug.Log("Turn passed!");

		// No turn taken, just modify battle state
		Transition();
	}




	private void InitEncounterData()
	{
		data = EncounterDataStaticContainer.GetData();
	}

	private void SetupHeroes()
	{
		// Deserialize the heroes we need
		List<HeroData> heroes = JsonParser.LoadHeroes(data.heroNames);

		// Initialise the combatants list
		HeroCombatants = new List<HeroController>();

		// Instantiate game objects for the heroes
		for (var i = 0; i < heroes.Count; ++i)
		{
			GameObject newHero = Instantiate(heroPrefab);
			newHero.transform.localPosition = HERO_SPAWN_POSITIONS[i];
			HeroController heroController = newHero.GetComponent<HeroController>();
			heroController.Init(heroes[i], this);
			HeroCombatants.Add(heroController);
		}
	}

	private void SetupEnemies()
	{
		// Deserialize the enemies we need
		List<EnemyData> enemies = JsonParser.LoadEnemies(data.enemyNames);

		// Initialise the combatants list
		EnemyCombatants = new List<EnemyController>();

		// Instantiate game objects for the enemies
		for (var i = 0; i < enemies.Count; ++i)
		{
			GameObject newEnemy = Instantiate(enemyPrefab);
			newEnemy.transform.localPosition = ENEMY_SPAWN_POSITIONS[i];
			EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
			enemyController.Init(enemies[i], this);
			EnemyCombatants.Add(enemyController);
		}
	}

	private void InitTurnOrder()
	{
		// Start with player turn, first hero
		HeroTurnIndex = 0;
		EnemyTurnIndex = 0;
	}


	/// <summary>
	/// Checks whether or not the given combatant should be allowed to take a turn
	/// in the current BattleController state.
	/// </summary>
	/// <param name="source">The CombatantController trying to take a turn.</param>
	/// <returns>Whether or not this combatant taking a turn is legal.</returns>
	private bool VerifyCombatantTurn(CombatantController source)
	{
		if (State == BattleState.PLAYERCHOICE)
		{
			return source.Allegiance == CombatantController.CombatantAllegiance.PLAYER
				&& source.Name == HeroCombatants[HeroTurnIndex].Name;
		}

		if (State == BattleState.ENEMYCHOICE)
		{
			return source.Allegiance == CombatantController.CombatantAllegiance.ENEMY
				&& source.Name == EnemyCombatants[EnemyTurnIndex].Name;
		}

		// We should never get here - why are we trying to take a turn when not in
		// a combatant choice state?
		Debug.Break();
		return false;
	}


	/// <summary>
	/// Deals damage to a given target combatant based on the stats of a given
	/// source combatant.
	/// </summary>
	/// <param name="source">The attacking combatant.</param>
	/// <param name="target">The combatant being attacked.</param>
	private void DoAttack(CombatantController source, CombatantController target)
	{
		// Determine the damage of this attack
		int damageMedian = source.Strength*3;
		int damage = Random.Range((int)(damageMedian*0.75f), (int)(damageMedian*1.25f));
		
		// Deal the damage
		target.HP -= damage;
		target.HP = Mathf.Clamp(target.HP, 0, target.MaxHP);
		Debug.Log(source.Name + " did " + damage + " damage to " + target.Name);
	}


	/// <summary>
	/// Executes an ability from a given combatant to a given target combatant.
	/// </summary>
	/// <param name="ability">The Ability object being executed</param>
	/// <param name="source"></param>
	/// <param name="target"></param>
	private void DoAbility(AbilityData ability, CombatantController source, CombatantController target)
	{
		// Apply any effects the ability has on the target
		foreach(EffectData effect in ability.effects)
		{
			DoEffect(effect, source, target);
		}

		// Apply any auras the ability has on the target
		foreach(AuraData aura in ability.auras)
		{
			target.AddAura(aura);
		}
	}


	/// <summary>
	/// Applies an effect to the given target combatant.
	/// </summary>
	/// <param name="effect">The effect being applied.</param>
	/// <param name="source">The combatant who is casting the effect.</param>
	/// <param name="target">The combatant the effect is affecting.</param>
	private void DoEffect(EffectData effect, CombatantController source, CombatantController target)
	{
		Debug.Log("Applying effect: " + effect.name);

		string statStr = effect.stat.ToLowerInvariant();
		if (statStr == "hp")
		{
			// Calculate the magnitude of this effect
			int magnitude = effect.amount;

			// Scale it with strength
			if (magnitude > 0)
				magnitude = (int)(magnitude + (source.Strength * effect.strengthScaling));
			else if (magnitude < 0)
				magnitude = (int)(magnitude - (source.Strength * effect.strengthScaling));

			// Calculate if it crit or not
			if (effect.canCrit)
			{
				// Crit chance is a 3% base plus an amount based on agility
				float critChance = 3.0f + 0.3f*source.Agility;
				critChance /= 100.0f;

				// Roll and see if this effect is critting
				if (Random.value >= 1.0f - critChance)
				{
					Debug.Log(source.Name + "'s " + effect.name + " effect crit!");
					magnitude *= 2;
				}
			}
			
			// Apply the effect
			target.HP += magnitude;
		}
		else
		{
			Debug.Log("Unsupported effect type received!");
			Debug.Break();
		}
	}


	private void Transition()
	{
		switch (State)
		{
			case BattleState.INIT:
				State = BattleState.PLAYERCHOICE;
				Debug.Log(HeroCombatants[HeroTurnIndex].Name + "'s turn!");
				break;

			case BattleState.PLAYERCHOICE:
				// It is now the enemy's turn
				State = BattleState.ENEMYCHOICE;
				EnemyTurnIndex = (EnemyTurnIndex + 1) % EnemyCombatants.Count;
				Debug.Log(EnemyCombatants[EnemyTurnIndex].Name + "'s turn!");

				// Resolve any auras
				EnemyCombatants[EnemyTurnIndex].ResolveAuras();

				// Carry out the enemy's turn immediately
				EnemyCombatants[EnemyTurnIndex].DoTurn();

				break;

			case BattleState.ENEMYCHOICE:
				State = BattleState.PLAYERCHOICE;
				HeroTurnIndex = (HeroTurnIndex + 1) % HeroCombatants.Count;
				Debug.Log(HeroCombatants[HeroTurnIndex].Name + "'s turn!");
				break;

			default:
				Debug.Log("Unexpected BattleState!");
				Debug.Break();
				break;
		}
	}
}
