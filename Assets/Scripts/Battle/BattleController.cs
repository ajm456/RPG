using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Allegiance = CombatantController.CombatantAllegiance;
using System.Collections;

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




	/* MEMBERS */
#pragma warning disable 0649
	[SerializeField] 
	private GameObject heroPrefab;

	[SerializeField]
	private GameObject enemyPrefab;

	[SerializeField]
	private TurnOrderUIController turnController;

	[SerializeField]
	private CameraController cameraController;

	[SerializeField]
	private FloatingTextController floatingTextController;

	[SerializeField]
	private List<string> debugHeroNames;

	[SerializeField]
	private List<string> debugEnemyNames;

	/// <summary>
	/// The spawn vectors for the heroes if there are 3 heroes combatants.
	/// </summary>
	[SerializeField]
	private Vector2[] heroSpawnPositions;

	/// <summary>
	/// The spawn vectors for enemies if there are 3 enemy combatants.
	/// </summary>
	[SerializeField]
	private Vector2[] enemySpawnPositions;

	[SerializeField]
	private bool skipIntro;

	/// <summary>
	/// The size of the turn queue.
	/// </summary>
	[SerializeField]
	private int turnQueueSize;

	/// <summary>
	/// The threshold value agilities are subtracted from to calculate turn
	/// order.
	/// </summary>
	[SerializeField]
	private int turnClockStartVal;
#pragma warning restore 0649

	/// <summary>
	/// Current state of the battle.
	/// </summary>
	public BattleState State
	{
		get;
		set;
	}

	public bool UIEnabled
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
	private List<CombatantController> Combatants
	{
		get;
		set;
	}

	/// <summary>
	/// Helper property for accessing the current combatant.
	/// </summary>
	private CombatantController CurrCombatant
	{
		get
		{
			return Combatants[CombatantTurnQueue.Peek()];
		}
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

	public Queue<int> CombatantTurnQueue
	{
		get;
		private set;
	}


	/// <summary>
	/// A list of turn clocks for each combatant.
	/// </summary>
	private List<int> CombatantTurnClocks
	{
		get;
		set;
	}


	private Dictionary<int, List<int>> TurnClocksAtGivenTurn
	{
		get;
		set;
	}

	public int TurnNum
	{
		get;
		private set;
	}


	/// <summary>
	/// The name of the combatant whose turn it is.
	/// </summary>
	public string CurrCombatantName
	{
		get
		{
			return CurrCombatant.Name;
		}
	}

	/// <summary>
	/// The ID of the combatant whose turn it is
	/// </summary>
	public int CurrCombatantID
	{
		get
		{
			return CurrCombatant.BattleID;
		}
	}

	/// <summary>
	/// A list of the current combatant's Strife abilities
	/// </summary>
	public List<AbilityData> CurrCombatantStrifeAbilities
	{
		get
		{
			return CurrCombatant.StrifeAbilities;
		}
	}

	/// <summary>
	/// A list of the current combatant's Calm abilities
	/// </summary>
	public List<AbilityData> CurrCombatantCalmAbilities
	{
		get
		{
			return CurrCombatant.CalmAbilities;
		}
	}

	/// <summary>
	/// Fetches the color of the current combatant. This is meaningless if the
	/// current combatant is an enemy as they do not have a color property.
	/// </summary>
	public Color CurrCombatantColor
	{
		get
		{
			if (CurrCombatant.Allegiance != Allegiance.PLAYER)
			{
				Debug.Log("Trying to get the color of a non-player Combatant - is this right?");
				Debug.Break();
			}

			return ((HeroController)CurrCombatant).Color;
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
	/// A list of SpriteRenderer objects for each combatant in this battle,
	/// ordered by their ID.
	/// </summary>
	private List<SpriteRenderer> CombatantSpriteRenderers
	{
		get;
		set;
	}

	/// <summary>
	/// The universal attack ability used whenever a combatant uses attack.
	/// </summary>
	private AbilityData attackAbility;


	/// <summary>
	/// Holds the IDs of all combatants currently involved in a turn animation.
	/// </summary>
	private List<int> currentlyAnimatingCombatants;


	/// <summary>
	/// A set of IDs of all combatants who are queued for a future animation.
	/// </summary>
	private HashSet<int> combatantsQueuedForAnim;


	/// <summary>
	/// Holds a queue of all turn animations to be carried out, processed by
	/// AnimCoroutineManager().
	/// </summary>
	private Queue<IEnumerator> animCoroutineQueue;




	/* OVERRIDES */

	private void Awake()
	{
		State = BattleState.INIT;

		// DEBUG - Set encounter data from inside scene
		if (Application.isEditor)
		{
			if (!EncounterDataStaticContainer.IsDataSet())
			{
				Debug.Log("Setting dummy data from inside BattleController - did you mean to do this?");
				EncounterDataStaticContainer.SetData(new EncounterData(debugHeroNames, debugEnemyNames));
			}
		}
		
		// Grab the encounter data (hopefully it was set before loading this scene)
		InitEncounterData();

		// Deserialize Hero/Enemy data and initialise objects
		SetupCombatants();

		// Discern the turn order
		InitialiseTurnOrder();

		// Start the animation coroutine handler
		StartCoroutine(AnimCoroutineManager());

		// If we're going to have to wait on the player, raise our flag
		if (CurrCombatant.Allegiance == Allegiance.PLAYER)
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

		// Start the camera animation
		if (!skipIntro)
		{
			UIEnabled = false;
			cameraController.PlayIntro();
		}
		else
		{
			UIEnabled = true;
		}
	}

	private void Update()
	{
		// If the combatant is involved in an animation currently, we have to
		// wait for that to finish
		if (!ResolvedAurasThisTurn)
		{
			if (CurrCombatant.Allegiance == Allegiance.ENEMY)
			{
				if (currentlyAnimatingCombatants.Count > 0)
				{
					return;
				}
			}
			else
			{
				if (currentlyAnimatingCombatants.Contains(CurrCombatantID))
				{
					return;
				}
			}
		}

		// Resolve all auras affecting the current combatant
		if (!ResolvedAurasThisTurn)
		{
			Debug.Log("~~~~~~~~~~~");
			Debug.Log(CurrCombatant.Name + "[" + CurrCombatantID + "]'s turn!");
			CurrCombatant.ResolveAuras();
			ResolvedAurasThisTurn = true;
		}

		if (CurrCombatant.Allegiance == Allegiance.ENEMY)
		{
			// For enemies, execute their turn straight away
			((EnemyController)CurrCombatant).DoTurn();
		}
		else
		{
			// For players, we've got to wait until PlayerMenuController takes
			// the player turn
			//
			// This is pretty grim (why are we running update cycles if nothing's
			// happening?) but whatever, sue me
			if (WaitingOnPlayerTurn)
			{
				return;
			}
		}

		// The turn has been executed; prepare for the next one
		CombatantTurnQueue.Dequeue();
		++TurnNum;
		if (CurrCombatant.Allegiance == Allegiance.PLAYER)
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

		GenerateNewTurnEntry();
	}





	/* METHODS */

	/// <summary>
	/// Executes a turn from a given combatant by attacking a given target.
	/// </summary>
	/// <param name="source">The combatant taking the turn.</param>
	/// <param name="target">The combatant being attacked.</param>
	public void ExecuteTurnWithAttack(int sourceID, int targetID)
	{
		// Check that it really is the turn of the combatant executing this turn
		if (!VerifyCombatantTurn(sourceID))
		{
			Debug.Log("Someone tried to use an attack when it wasn't their turn!");
			Debug.Break();
			return;
		}

		// Execute attack
		combatantsQueuedForAnim.Add(sourceID);
		combatantsQueuedForAnim.Add(targetID);
		animCoroutineQueue.Enqueue(DoAttack(Combatants[sourceID], Combatants[targetID]));
	}


	/// <summary>
	/// Executes a turn from a given combatant by using a given ability on a
	/// given target.
	/// </summary>
	/// <param name="ability">The AbilityData object of the ability being used.</param>
	/// <param name="sourceID">The combatant taking the turn.</param>
	/// <param name="targetID">The combatant targeted by the ability.</param>
	public void ExecuteTurnWithAbility(AbilityData ability, int sourceID, List<int> targetIDs)
	{
		// Check that it really is the turn of the combatant executing this turn
		if (!VerifyCombatantTurn(sourceID))
		{
			Debug.Log("Someone tried to use an ability when it wasn't their turn!");
			Debug.Break();
			return;
		}

		// Try and execute the ability
		List<CombatantController> targets = new List<CombatantController>(targetIDs.Count);
		foreach (int id in targetIDs)
		{
			targets.Add(Combatants[id]);
		}
		DoAbility(ability, Combatants[sourceID], targets);
	}


	/// <summary>
	/// Executes a turn for the current combatant with the given ability.
	/// </summary>
	/// <param name="ability">The ability the source combatant is using.</param>
	/// <param name="source">The combatant taking the turn.</param>
	/// <param name="target">The combatant receiving the ability.</param>
	public void ExecuteTurnWithAbilityOnRandomTarget(AbilityData ability)
	{
		// Pick a random enemy to the given allegiance type
		List<int> possibleTargetIDs = new List<int>();
		foreach (CombatantController combatant in Combatants)
		{
			if (combatant.Allegiance != CurrCombatant.Allegiance)
			{
				possibleTargetIDs.Add(combatant.BattleID);
			}
		}
		int targetID = possibleTargetIDs[Random.Range(0, possibleTargetIDs.Count)];

		// Try and execute the ability
		DoAbility(ability, Combatants[CurrCombatantID], new List<CombatantController>() { Combatants[targetID] });
	}

	/// <summary>
	/// Executes a turn by doing nothing.
	/// </summary>
	public void PassTurn()
	{
		Debug.Log("Turn passed!");
	}



	/// <summary>
	/// Fetches the transform of the combatant with a given ID.
	/// </summary>
	/// <param name="combatantID"></param>
	/// <returns></returns>
	public Transform GetCombatantTransform(int combatantID)
	{
		return Combatants[combatantID].transform;
	}

	public string GetCombatantName(int combatantID)
	{
		Debug.Assert(combatantID < Combatants.Count, "Trying to get combatant name for index " + combatantID + " which is outside of combatant list range " + Combatants.Count);
		return Combatants[combatantID].Name;
	}

	public int GetCombatantHP(int combatantID)
	{
		return Combatants[combatantID].HP;
	}

	public int GetCombatantMaxHP(int combatantID)
	{
		return Combatants[combatantID].MaxHP;
	}

	public int GetCombatantCalm(int combatantID)
	{
		return ((HeroController)Combatants[combatantID]).Calm;
	}

	public int GetCombatantStrife(int combatantID)
	{
		return ((HeroController)Combatants[combatantID]).Strife;
	}

	public List<string> GetOrderedCombatantNames()
	{
		List<string> names = new List<string>();
		foreach (int id in CombatantTurnQueue)
		{
			names.Add(Combatants[id].Name);
		}

		return names;
	}

	public int GetNumCombatants()
	{
		return Combatants.Count;
	}

	/// <summary>
	/// Fetches the number of hero combatants.
	/// </summary>
	/// <returns>The integer number of hero combatants in this battle.</returns>
	public int GetNumHeroes()
	{
		int num = 0;
		foreach (CombatantController combatant in Combatants)
		{
			if (combatant.Allegiance == Allegiance.PLAYER)
			{
				++num;
			}
		}
		
		return num;
	}

	public int GetNumEnemies()
	{
		return GetNumCombatants() - GetNumHeroes();
	}

	/// <summary>
	/// Fetches the battle ID for the nth hero combatant ordered by how
	/// they're loaded.
	/// </summary>
	/// <param name="n">The number of the hero combatant being searched for.</param>
	/// <returns>The integer battle ID of the nth hero combatant.</returns>
	public int GetNthHeroID(int n)
	{
		int currHeroIndex = 0;
		int nthHeroID = -1;
		foreach (CombatantController combatant in Combatants)
		{
			if (combatant.Allegiance != Allegiance.PLAYER)
			{
				continue;
			}

			if (currHeroIndex == n)
			{
				nthHeroID = combatant.BattleID;
				break;
			}

			currHeroIndex++;
		}

		if (nthHeroID == -1)
		{
			Debug.Log("Could not find hero number " + n + "!");
			Debug.Break();
		}
		
		return nthHeroID;
	}

	/// <summary>
	/// Fetches a list of all hero combatant names in this battle including
	/// dead ones.
	/// </summary>
	/// <returns>String list of all hero combatant names.</returns>
	public List<string> GetHeroNames()
	{
		List<string> heroNames = new List<string>();
		foreach (CombatantController combatant in Combatants)
		{
			if (combatant.Allegiance == Allegiance.PLAYER)
			{
				heroNames.Add(combatant.Name);
			}
		}

		return heroNames;
	}

	public Color GetHeroColor(int heroID)
	{
		Color color = Color.magenta;

		if (Combatants[heroID].Allegiance == Allegiance.ENEMY)
		{
			Debug.Log("Trying to get color of a non-hero combatant!");
			Debug.Break();
		}
		else
		{
			color = ((HeroController)Combatants[heroID]).Color;
		}

		return color;
	}

	public int GetHeroCalm(int heroID)
	{
		return ((HeroController)Combatants[heroID]).Calm;
	}

	public int GetHeroStrife(int heroID)
	{
		return ((HeroController)Combatants[heroID]).Strife;
	}

	public int GetHeroResource(int heroID)
	{
		return ((HeroController)Combatants[heroID]).Strife - ((HeroController)Combatants[heroID]).Calm;
	}

	/// <summary>
	/// Fetches a list of all hero combatant ability lists.
	/// </summary>
	/// <returns>A list of Calm + Strife ability lists.</returns>
	public List<List<List<AbilityData>>> GetHeroAbilities()
	{
		List<List<List<AbilityData>>> heroAbilityLists = new List<List<List<AbilityData>>>();

		foreach (CombatantController combatant in Combatants)
		{
			// Skip non-Hero combatants
			if (combatant.Allegiance != Allegiance.PLAYER)
			{
				continue;
			}

			// Calm abilities
			List<AbilityData> calmAbilities = combatant.CalmAbilities;
			// Strife abilities
			List<AbilityData> strifeAbilities = combatant.StrifeAbilities;
			// Group em up
			List<List<AbilityData>> allAbilities = new List<List<AbilityData>> {
				calmAbilities,
				strifeAbilities
			};
			// And put em away
			heroAbilityLists.Add(allAbilities);
		}

		return heroAbilityLists;
	}


	/// <summary>
	/// Generates a list of all hero battle IDs.
	/// </summary>
	public List<int> GetHeroIDs()
	{
		List<int> ids = new List<int>(GetNumHeroes());
		for (int i = 0; i < GetNumHeroes(); ++i)
		{
			ids.Add(i);
		}

		return ids;
	}


	/// <summary>
	/// Generates a list of all enemy battle IDs.
	/// </summary>
	public List<int> GetEnemyIDs()
	{
		List<int> ids = new List<int>(GetNumEnemies());
		for (int i = GetNumHeroes(); i < GetNumCombatants(); ++i)
		{
			ids.Add(i);
		}

		return ids;
	}


	/// <summary>
	/// Get the ID of a random target of the given allegiance.
	/// </summary>
	/// <param name="allegiance">
	/// The allegiance of the target to be randomly
	/// chosen.</param>
	/// <returns>The target's battle ID.</returns>
	public int GetRandomTargetID(Allegiance allegiance)
	{
		if (allegiance == Allegiance.PLAYER)
		{
			return (int)(Random.value * GetNumHeroes());
		}
		else
		{
			return (int)(GetNumHeroes() + (Random.value * GetNumEnemies()));
		}
	}


	/// <summary>
	/// Returns whether or not the combatant for the given ID is currently in an
	/// animation.
	/// </summary>
	/// <param name="id">The battle ID of the combatant being queried for.</param>
	/// <returns>
	/// Whether or not the combatant with the given ID is currently
	/// in an animation.
	/// </returns>
	public bool IsCombatantAnimating(int id)
	{
		return currentlyAnimatingCombatants.Contains(id);
	}


	/// <summary>
	/// Returns whether or not the combatant for the given ID is queued for an
	/// animation that is yet to take place.
	/// </summary>
	/// <param name="id">The battle ID of the combatant being queried for.</param>
	/// <returns>
	/// Whether or not the combatant with the given ID is queued for a future
	/// animation.
	/// </returns>
	public bool IsCombatantInAnimQueue(int id)
	{
		return combatantsQueuedForAnim.Contains(id);
	}


	/// <summary>
	/// Returns whether or not the combatant for the given ID is currently
	/// alive.
	/// </summary>
	/// <param name="id">The battle ID of the combatant being queried for.</param>
	/// <returns>
	/// Whether or not the combatant with the given ID is currently
	/// alive.
	/// </returns>
	public bool IsCombatantAlive(int id)
	{
		return Combatants[id].HP > 0;
	}




	private void InitEncounterData()
	{
		data = EncounterDataStaticContainer.GetData();
		// Pre-load the attack ability data
		attackAbility = JsonParser.LoadAttackAbility();
	}

	private void SetupCombatants()
	{
		// Initialise the combatants list
		Combatants = new List<CombatantController>();

		// Initialise the list of sprite renderers
		CombatantSpriteRenderers = new List<SpriteRenderer>();

		// Initialise the list of animating combatant IDs
		currentlyAnimatingCombatants = new List<int>();

		// Initialise the animation coroutine queue
		animCoroutineQueue = new Queue<IEnumerator>();

		// Initialise the set of animation-queued combatant IDs
		combatantsQueuedForAnim = new HashSet<int>();

		// Keep track of the assigned IDs
		int lastAssignedID = 0;

		// Deserialize the heroes we need
		List<HeroData> heroes = JsonParser.LoadHeroes(data.heroNames);

		// Instantiate game objects for the heroes
		for (var i = 0; i < heroes.Count; ++i)
		{
			GameObject newHero = Instantiate(heroPrefab);
			newHero.transform.localPosition = heroSpawnPositions[i];
			CombatantSpriteRenderers.Add(newHero.GetComponent<SpriteRenderer>());
			HeroController heroController = newHero.GetComponent<HeroController>();
			heroController.Init(heroes[i], this, lastAssignedID);
			++lastAssignedID;
			Combatants.Add(heroController);
		}

		// Deserialize the enemies we need
		List<EnemyData> enemies = JsonParser.LoadEnemies(data.enemyNames);

		// Instantiate game objects for the enemies
		for (var i = 0; i < enemies.Count; ++i)
		{
			GameObject newEnemy = Instantiate(enemyPrefab);
			// The position depends on how many enemies there are:
			// 1 enemy: middle
			// 2 enemies: top, bottom
			// 3 enemies: top, middle, bottom
			if (enemies.Count == 1)
			{
				newEnemy.transform.localPosition = enemySpawnPositions[1];
			}
			else if (enemies.Count == 2)
			{
				if (i == 0)
				{
					newEnemy.transform.localPosition = enemySpawnPositions[0];
				}
				else if (i == 1)
				{
					newEnemy.transform.localPosition = enemySpawnPositions[2];
				}
			}
			else
			{
				newEnemy.transform.localPosition = enemySpawnPositions[i];
			}
			
			CombatantSpriteRenderers.Add(newEnemy.GetComponent<SpriteRenderer>());
			EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
			enemyController.Init(enemies[i], this, lastAssignedID);
			++lastAssignedID;
			Combatants.Add(enemyController);
		}
	}

	private void InitialiseTurnOrder()
	{
		CombatantTurnQueue = new Queue<int>(turnQueueSize);
		CombatantTurnClocks = new List<int>(GetNumCombatants());
		TurnClocksAtGivenTurn = new Dictionary<int, List<int>>();
		TurnNum = 0;
		
		// Initialise ID-agility list and turn clock dict
		foreach (CombatantController comb in Combatants)
		{
			CombatantTurnClocks.Add(turnClockStartVal);
		}

		// Generate a full queue of turns
		while (CombatantTurnQueue.Count < turnQueueSize)
		{
			GenerateNewTurnEntry();
		}
	}

	private void GenerateNewTurnEntry()
	{
		bool turnAssigned = false;

		while (!turnAssigned)
		{
			// First check if any combatants' clocks are already below zero
			List<Tuple<int, int>> subZeroClockIDs = new List<Tuple<int, int>>();
			for (int i = 0; i < CombatantTurnClocks.Count; ++i)
			{
				// If the combatant is dead, don't consider them
				if (!IsCombatantAlive(i))
				{
					continue;
				}

				if (CombatantTurnClocks[i] <= 0)
				{
					subZeroClockIDs.Add(new Tuple<int, int>(i, CombatantTurnClocks[i]));
				}
			}
		
			if (subZeroClockIDs.Count > 0)
			{
				// If there are combatants below zero on their clocks, one of them will
				// have the next turn

				// Find which ID has precedence
				subZeroClockIDs.Sort(CompareIDTurnClockPair);
				int idOfNextTurnTaker = subZeroClockIDs[0].Item1;

				// Set their turn clock appropriately for how much they've overspilled
				CombatantTurnClocks[idOfNextTurnTaker] = turnClockStartVal + CombatantTurnClocks[idOfNextTurnTaker] + Combatants[idOfNextTurnTaker].Agility;

				// Add them to the turn queue
				CombatantTurnQueue.Enqueue(idOfNextTurnTaker);

				// Store the turn clocks at this turn
				TurnClocksAtGivenTurn[TurnClocksAtGivenTurn.Keys.Count] = new List<int>(CombatantTurnClocks);

				// A turn has been assigned - we can exit the loop
				turnAssigned = true;
			}
			else
			{
				// If there aren't any combatants below zero on their clocks,
				// subtract agilities and check again
				for (int i = 0; i < CombatantTurnClocks.Count; ++i)
				{
					// If the combatant is dead, don't consider them
					if (!IsCombatantAlive(i))
					{
						continue;
					}

					CombatantTurnClocks[i] -= Combatants[i].Agility;
				}

				// We'll need to run the loop again to check if anyone's clock
				// has hit zero
				turnAssigned = false;
			}
		}
	}

	private int CompareIDTurnClockPair(Tuple<int, int> p1, Tuple<int, int> p2)
	{
		// First try sorting by turn clock (lower time = precedence)
		var result = p1.Item2.CompareTo(p2.Item2);
		
		if (result == 0)
		{
			// If they have the same time remaining, sort favouring player
			// over enemy
			if (Combatants[p1.Item1].Allegiance == Combatants[p2.Item1].Allegiance)
			{
				// If they are the same allegiance, sort by ID
				result = p1.Item1.CompareTo(p2.Item1);
			}
			else
			{
				// Player characters get precedence
				if (Combatants[p1.Item1].Allegiance == Allegiance.PLAYER)
				{
					result = -1;
				}
				else
				{
					result = 1;
				}
			}
		}

		return result;
	}

	public void RefreshTurnOrder()
	{
		// Cache the current combatant ID as they should remain at the front
		// of the queue until their turn actually ends (which is after this),
		// as long as they are alive
		int currentId = CurrCombatantID;
		CombatantTurnQueue = new Queue<int>(turnQueueSize);
		CombatantTurnClocks = TurnClocksAtGivenTurn[TurnNum];
		if (IsCombatantAlive(currentId))
		{
			CombatantTurnQueue.Enqueue(currentId);
		}

		// Generate a full queue of turns
		while (CombatantTurnQueue.Count < turnQueueSize)
		{
			GenerateNewTurnEntry();
		}
	}

	public void HighlightCombatant(int combatantID)
	{
		// This should never be called when it's not a player turn
		Debug.Assert(CurrCombatant.Allegiance == Allegiance.PLAYER);

		Color c = ((HeroController)CurrCombatant).Color;
		CombatantSpriteRenderers[combatantID].color = c;
	}

	public void UnhighlightCombatant(int combatantID)
	{
		CombatantSpriteRenderers[combatantID].color = Color.white;
	}


	/// <summary>
	/// Continuously processes turn animations in animCoroutineQueue
	/// </summary>
	/// <returns></returns>
	private IEnumerator AnimCoroutineManager()
	{
		while (true)
		{
			while (animCoroutineQueue.Count > 0)
			{
				// Grab the next animation coroutine in the queue and start it
				yield return StartCoroutine(animCoroutineQueue.Dequeue());
			}

			yield return null;
		}
	}

	/// <summary>
	/// Checks whether or not the given combatant ID should be allowed to take
	/// a turn in the current BattleController state.
	/// </summary>
	/// <param name="sourceID">The ID of the CombatantController trying to take a turn.</param>
	/// <returns>Whether or not this combatant taking a turn is legal.</returns>
	private bool VerifyCombatantTurn(int sourceID)
	{
		if (State == BattleState.PLAYERCHOICE)
		{
			return Combatants[sourceID].Allegiance == Allegiance.PLAYER
				&& Combatants[sourceID].Name == CurrCombatant.Name;
		}

		if (State == BattleState.ENEMYCHOICE)
		{
			return Combatants[sourceID].Allegiance == Allegiance.ENEMY
				&& Combatants[sourceID].Name == CurrCombatant.Name;
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
	private IEnumerator DoAttack(CombatantController source, CombatantController target)
	{
		// If these combatants are animating, wait for them to finish
		while (currentlyAnimatingCombatants.Contains(source.BattleID)
			|| currentlyAnimatingCombatants.Contains(target.BattleID))
		{
			Debug.Log("Waiting for combatants to finish animating!");
			yield return null;
		}

		// These combatants are ready to animate, so remove them from the anim
		// queue
		combatantsQueuedForAnim.Remove(source.BattleID);
		combatantsQueuedForAnim.Remove(target.BattleID);

		// Add the combatant IDs to the list of currently animating ones
		// so they don't get involved in any future animations throughout this
		// one
		currentlyAnimatingCombatants.Add(source.BattleID);
		currentlyAnimatingCombatants.Add(target.BattleID);

		Vector3 moveTarget = target.transform.position;
		if (target.Allegiance == Allegiance.PLAYER)
		{
			moveTarget += new Vector3(0.3f, 0f);
		}
		else
		{
			moveTarget += new Vector3(-0.3f, 0f);
		}
		Vector3 startPos = source.transform.position;
		
		// Move the combatant to "attack range" of its target
		yield return MoveCombatantToPos(source, moveTarget, 0.5f);

		source.SetAnimBool("attacking", true);

		yield return new WaitForSeconds(source.GetAnimDuration("attacking"));
		// Simply use the attack ability
		DoAbility(attackAbility, source, new List<CombatantController>() { target });

		source.SetAnimBool("attacking", false);

		yield return MoveCombatantToPos(source, startPos, 0.5f);
		
		// These combatants are no longer animating, so remove them from the
		// list so they can be involved in future animations
		currentlyAnimatingCombatants.Remove(source.BattleID);
		currentlyAnimatingCombatants.Remove(target.BattleID);
	}


	/// <summary>
	/// Executes an ability from a given combatant to the given target combatants.
	/// </summary>
	/// <param name="ability">The Ability object being executed</param>
	/// <param name="source">The combatant using the ability.</param>
	/// <param name="targets">A list of combatants being targeted by the ability.</param>
	private void DoAbility(AbilityData ability, CombatantController source, List<CombatantController> targets)
	{
		// Start the ability's animation
		Debug.Log("TODO: Ability animations!");


		// Apply the ability to each target combatant
		foreach (CombatantController target in targets)
		{
			// Apply any effects the ability has on the target
			foreach (EffectData effect in ability.effects)
			{
				target.ApplyEffect(effect, source);
				// Generate any floating combat text if required
				floatingTextController.PlayTextForEffect(effect, GetCombatantTransform(target.BattleID));
			}

			// If the target combatant died, set their animation state
			if (target.HP <= 0)
			{
				KillCombatant(target);
			}
			else
			{
				// Apply any auras the ability has on the target
				foreach (AuraData aura in ability.auras)
				{
					target.AddAura(aura, source);
				}
			}
			
			// Modify the caster's resource values
			if (source.Allegiance == Allegiance.PLAYER)
			{
				HeroController heroSource = (HeroController)source;
				if (heroSource.IsProtag)
				{
					// Jack's Calm and Strife generation are not mutually exclusive
					heroSource.Calm += ability.calmGen;
					heroSource.Strife += ability.strifeGen;
				}
				else
				{
					// Abilities which generate Calm and Strife exist, but should
					// not be usable by anyone other than the protagonist
					if (ability.calmGen != 0 && ability.strifeGen != 0)
					{
						Debug.LogError("Abilities which generate Calm and Strife should not be usable by non-protagonist!");
						Debug.Break();
					}

					// Non-protagonists lose one resource when they gain another
					if (ability.calmGen > 0)
					{
						if (heroSource.Strife > 0)
						{
							heroSource.Strife -= ability.calmGen;
						}
						else
						{
							heroSource.Calm += ability.calmGen;
						}
					}
					else if (ability.strifeGen > 0)
					{
						if (heroSource.Calm > 0)
						{
							heroSource.Calm -= ability.strifeGen;
						}
						else
						{
							heroSource.Strife += ability.strifeGen;
						}
					}
				}
			}
		}
	}


	/// <summary>
	/// Smoothly moves a combatant's sprite to the given location in world space
	/// over the given duration.
	/// </summary>
	/// <param name="source">The combatant moving.</param>
	/// <param name="target">The location being moved to.</param>
	/// <param name="duration">How long the combatant should be in motion for</param>
	/// <returns></returns>
	private IEnumerator MoveCombatantToPos(CombatantController source, Vector3 target, float duration)
	{
		source.SetAnimBool("moving", true);
		
		Vector3 startPos = source.transform.position;
		float dt = 0f;
		while (dt < duration)
		{
			dt += Time.smoothDeltaTime;
			source.transform.position = Vector3.Lerp(startPos, target, dt / duration);
			yield return null;
		}
		source.transform.position = target;

		source.SetAnimBool("moving", false);
	}


	private void KillCombatant(CombatantController target)
	{
		// Set their animation
		target.SetAnimBool("dead", true);

		// If all enemies or heroes are dead, end the battle
		int numDeadHeroes = 0;
		int numDeadEnemies = 0;
		for (int i = 0; i < GetNumHeroes(); ++i)
		{
			if (Combatants[i].HP <= 0)
			{
				numDeadHeroes++;
			}
		}
		for (int i = GetNumHeroes(); i < GetNumCombatants(); ++i)
		{
			if (Combatants[i].HP <= 0)
			{
				numDeadEnemies++;
			}
		}

		// Check if the battle is over
		if (numDeadHeroes == GetNumHeroes())
		{
			State = BattleState.ENEMYWON;
			EndBattle();
		}
		else if (numDeadEnemies == GetNumEnemies())
		{
			State = BattleState.PLAYERWON;
			EndBattle();
		}
		else
		{
			// Refresh turn order
			RefreshTurnOrder();
			turnController.ForceRefreshTurnOrder();
		}
	}



	private void ExecuteEnemyTurn()
	{
		// One last check that the current combatant is an enemy
		Debug.Assert(CurrCombatant.Allegiance == Allegiance.ENEMY);

		EnemyController enemy = (EnemyController)CurrCombatant;
		// Carry out the enemy's turn
		enemy.DoTurn();

		// If the next turn is an enemy too, recursively execute their turn
		CombatantTurnQueue.Dequeue();
		if (CurrCombatant.Allegiance == Allegiance.ENEMY)
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


	/// <summary>
	/// Marks the end of a battle for whatever reason.
	/// </summary>
	private void EndBattle()
	{
		animCoroutineQueue.Clear();
	}
}
