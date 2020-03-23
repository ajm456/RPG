using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Allegiance = CombatantController.CombatantAllegiance;

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
	/// Holds a ref to each hero combatant's controller in this battle. Ordered
	/// according to how the heroes appear in battle, not in their turn order.
	/// </summary>
	public List<HeroController> HeroCombatants
	{
		get;
		private set;
	}

	/// <summary>
	/// Holds a ref to each enemy combatant's controller in this battle.
	/// Ordered according to how the enemies appear in battle, not in their
	/// turn order.
	/// </summary>
	public List<EnemyController> EnemyCombatants
	{
		get;
		private set;
	}

	/// <summary>
	/// An turn-ordered list of the turns of each combatant. This will be valid
	/// until a combatant dies or agility stats change.
	/// </summary>
	private List<CombatantController> TurnOrderCombatants
	{
		get;
		set;
	}

	/// <summary>
	/// The index for the current combatant in <see cref="TurnOrderCombatants"/>
	/// </summary>
	private int TurnOrderIndex
	{
		get;
		set;
	}

	/// <summary>
	/// The name of the combatant whose turn it is.
	/// </summary>
	public string CurrCombatantName
	{
		get
		{
			return TurnOrderCombatants[TurnOrderIndex].Name;
		}
	}

	/// <summary>
	/// Flag to indicate whether this controller is currently waiting for a
	/// player turn to be executed by <see cref="PlayerMenuController"/>.
	/// </summary>
	public bool WaitingOnPlayerTurn
	{
		get;
		set;
	}

	/// <summary>
	/// Flag raised once auras have been resolved for this turn. This is
	/// required for player turns as they can take an indefinite number of
	/// update cycles before actually executing.
	/// </summary>
	private bool ResolvedAurasThisTurn
	{
		get;
		set;
	}




	/* OVERRIDES */

	private void Awake()
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

		// If we're going to have to wait on the player, raise our flag
		if (TurnOrderCombatants[TurnOrderIndex].Allegiance == Allegiance.PLAYER)
		{
			WaitingOnPlayerTurn = true;
			State = BattleState.PLAYERCHOICE;
		}
		else
		{
			WaitingOnPlayerTurn = false;
			State = BattleState.ENEMYCHOICE;
		}

		ResolvedAurasThisTurn = false;
	}

	private void Update()
	{
		// Resolve all auras affecting the current combatant
		if (!ResolvedAurasThisTurn)
		{
			TurnOrderCombatants[TurnOrderIndex].ResolveAuras();
			ResolvedAurasThisTurn = true;
		}

		if (TurnOrderCombatants[TurnOrderIndex].Allegiance == Allegiance.ENEMY)
		{
			// For enemies, execute their turn straight away
			((EnemyController)TurnOrderCombatants[TurnOrderIndex]).DoTurn();
		}
		else
		{
			// For players, we've got to wait until PlayerMenuController takes
			// the player turn
			//
			// This is pretty grim (why are we running update cycles if nothing's
			// happening?) but whatever, sue me
			if (WaitingOnPlayerTurn)
				return;
		}

		// The turn has been executed; prepare for the next one
		TurnOrderIndex = (TurnOrderIndex + 1) % TurnOrderCombatants.Count;
		if (TurnOrderCombatants[TurnOrderIndex].Allegiance == Allegiance.PLAYER)
		{
			State = BattleState.PLAYERCHOICE;
			WaitingOnPlayerTurn = true;
		}
		else
		{
			State = BattleState.ENEMYCHOICE;
			WaitingOnPlayerTurn = false;
		}
		ResolvedAurasThisTurn = false;
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
	}

	public void PassTurn()
	{
		Debug.Log("Turn passed!");
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

	/// <summary>
	/// Set the initial turn order for the combatants in this battle. Details of how
	/// this is calculated can be found in the GDD.
	/// </summary>
	private void InitTurnOrder()
	{
		TurnOrderCombatants = new List<CombatantController>();
		TurnOrderIndex = 0;

		// For each hero and enemy, store a copy of their faction, index, and agility stat
		// (just to make sorting easier)
		List<Tuple<Allegiance, int, float>> factionIndexAgilityList = new List<Tuple<Allegiance, int, float>>();
		for (int i = 0; i < HeroCombatants.Count; ++i)
		{
			factionIndexAgilityList.Add(new Tuple<Allegiance, int, float>(Allegiance.PLAYER, i, HeroCombatants[i].Agility));
		}
		for (int i = 0; i < EnemyCombatants.Count; ++i)
		{
			factionIndexAgilityList.Add(new Tuple<Allegiance, int, float>(Allegiance.ENEMY, i, EnemyCombatants[i].Agility));
		}

		// Sort list by agility descending
		factionIndexAgilityList.Sort((x, y) => y.Item3.CompareTo(x.Item3));

		// Find the lowest agility in this battle
		float lowestAgility = factionIndexAgilityList[factionIndexAgilityList.Count - 1].Item3;
		
		while (factionIndexAgilityList.Count > 0)
		{
			// Sort by agility
			factionIndexAgilityList.Sort((x, y) => y.Item3.CompareTo(x.Item3));
			// Add the highest agility combatant to the turn list
			var candidate = factionIndexAgilityList[0];
			if (candidate.Item1 == Allegiance.PLAYER)
			{
				TurnOrderCombatants.Add(HeroCombatants[candidate.Item2]);
			}
			else
			{
				TurnOrderCombatants.Add(EnemyCombatants[candidate.Item2]);
			}
			// Halve their agility stat
			factionIndexAgilityList[0] = new Tuple<Allegiance, int, float>(candidate.Item1, candidate.Item2, candidate.Item3 / 2f);
			// Remove them if their agility has dropped below the lowest
			if (factionIndexAgilityList[0].Item3 < lowestAgility)
			{
				factionIndexAgilityList.RemoveAt(0);
			}
		}
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
			return source.Allegiance == Allegiance.PLAYER
				&& source.Name == TurnOrderCombatants[TurnOrderIndex].Name;
		}

		if (State == BattleState.ENEMYCHOICE)
		{
			return source.Allegiance == Allegiance.ENEMY
				&& source.Name == TurnOrderCombatants[TurnOrderIndex].Name;
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
			target.ApplyEffect(effect, source);
		}

		// Apply any auras the ability has on the target
		foreach(AuraData aura in ability.auras)
		{
			target.AddAura(aura, source);
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
		target.ApplyEffect(effect, source);
	}


	private void ExecuteEnemyTurn()
	{
		// One last check that the current combatant is an enemy
		Debug.Assert(TurnOrderCombatants[TurnOrderIndex].Allegiance == Allegiance.ENEMY);

		EnemyController enemy = (EnemyController)TurnOrderCombatants[TurnOrderIndex];
		// Carry out the enemy's turn
		enemy.DoTurn();

		// If the next turn is an enemy too, recursively execute their turn
		TurnOrderIndex++;
		if (TurnOrderCombatants[TurnOrderIndex].Allegiance == Allegiance.ENEMY)
		{
			ExecuteEnemyTurn();
		}
		else
		{
			// The next combatant is player controlled, so let's wait for
			// PlayerMenuController to execute the turn
			State = BattleState.PLAYERCHOICE;
		}
	}
}
