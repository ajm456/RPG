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
	private List<CombatantController> Combatants
	{
		get;
		set;
	}

	/// <summary>
	/// An turn-ordered list of the turns of each combatant. This will be valid
	/// until a combatant dies or agility stats change.
	/// </summary>
	private List<int> TurnOrderCombatantIDs
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
	/// Helper property for accessing the current combatant.
	/// </summary>
	private CombatantController CurrCombatant
	{
		get
		{
			return Combatants[TurnOrderCombatantIDs[TurnOrderIndex]];
		}
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
	/// Flag raised once auras have been resolved for this turn. This is
	/// required for player turns as they can take an indefinite number of
	/// update cycles before actually executing.
	/// </summary>
	private bool ResolvedAurasThisTurn
	{
		get;
		set;
	}

	/// <summary>
	/// Keeps track of the number of turns each combatant has had.
	/// </summary>
	private Dictionary<int,int> NumTurns{
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
		SetupCombatants();

		// Discern the turn order
		InitTurnOrder();



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
	}

	private void Update()
	{
		// Resolve all auras affecting the current combatant
		if (!ResolvedAurasThisTurn)
		{
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
		TurnOrderIndex = (TurnOrderIndex + 1) % TurnOrderCombatantIDs.Count;
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
		NumTurns[CurrCombatant.BattleID] += 1;
		ResolvedAurasThisTurn = false;
		if (TurnOrderIndex == 0)
		{
			InitTurnOrder();
		}
	}





	/* METHODS */

	/// <summary>
	/// Debug attack implementation to be called by HeroControllers which
	/// attacks the first enemy found in Combatants.
	/// </summary>
	public void DebugAttack()
	{
		// DEBUG: Find the first enemy combatant and use the attack on that
		foreach (CombatantController combatant in Combatants)
		{
			if (combatant.Allegiance == Allegiance.ENEMY)
			{
				ExecuteTurnWithAttack(CurrCombatantID, combatant.BattleID);
				break;
			}
		}
	}

	/// <summary>
	/// Executes a turn from a give combatant by attacking a given target.
	/// </summary>
	/// <param name="source">The combatant taking the turn.</param>
	/// <param name="target">The combatant being attacked.</param>
	public void ExecuteTurnWithAttack(int sourceID, int targetID)
	{
		// Check that it really is the turn of the combatant executing this turn
		if (!VerifyCombatantTurn(sourceID))
		{
			Debug.Log("Someone tried to execute a turn when it wasn't their turn!");
			Debug.Break();
			return;
		}

		// Execute attack
		DoAttack(Combatants[sourceID], Combatants[targetID]);
	}

	/// <summary>
	/// Executes a turn from a given combatant with a given ability on a given
	/// target combatant.
	/// </summary>
	/// <param name="ability">The ability the source combatant is using.</param>
	/// <param name="source">The combatant taking the turn.</param>
	/// <param name="target">The combatant receiving the ability.</param>
	public void ExecuteTurnWithAbilityOnRandomTarget(AbilityData ability, int sourceID, Allegiance sourceAllegiance)
	{
		// Check that it really is the turn of the combatant executing this turn
		if (!VerifyCombatantTurn(sourceID))
		{
			Debug.Log("Someone tried to execute a turn when it wasn't their turn!");
			Debug.Break();
			return;
		}

		// Pick a random enemy to the given allegiance type
		List<int> possibleTargetIDs = new List<int>();
		foreach (CombatantController combatant in Combatants)
		{
			if (combatant.Allegiance != sourceAllegiance)
			{
				possibleTargetIDs.Add(combatant.BattleID);
			}
		}
		int targetID = possibleTargetIDs[Random.Range(0, possibleTargetIDs.Count)];

		// Try and execute the ability
		DoAbility(ability, Combatants[sourceID], Combatants[targetID]);
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

	public List<string> GetCombatantNames()
	{
		List<string> names = new List<string>(Combatants.Count);
		foreach (CombatantController combatant in Combatants)
		{
			names.Add(combatant.Name);
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
			// Discord abilities
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




	private void InitEncounterData()
	{
		data = EncounterDataStaticContainer.GetData();
	}

	private void SetupCombatants()
	{
		// Initialise the combatants list
		Combatants = new List<CombatantController>();

		// Keep track of the assigned IDs
		int lastAssignedID = 0;

		// Deserialize the heroes we need
		List<HeroData> heroes = JsonParser.LoadHeroes(data.heroNames);

		// Instantiate game objects for the heroes
		for (var i = 0; i < heroes.Count; ++i)
		{
			GameObject newHero = Instantiate(heroPrefab);
			newHero.transform.localPosition = HERO_SPAWN_POSITIONS[i];
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
			newEnemy.transform.localPosition = ENEMY_SPAWN_POSITIONS[i];
			EnemyController enemyController = newEnemy.GetComponent<EnemyController>();
			enemyController.Init(enemies[i], this, lastAssignedID);
			++lastAssignedID;
			Combatants.Add(enemyController);
		}
	}

	/// <summary>
	/// Set the initial turn order for the combatants in this battle. Details of how
	/// this is calculated can be found in the GDD.
	/// </summary>
	private void InitTurnOrder()
	{
		TurnOrderCombatantIDs = new List<int>();
		TurnOrderIndex = 0;

		// For each combatant, store a copy of their faction, index, and agility
		// stat (just to make sorting easier)
		List<Tuple<Allegiance, int, float>> factionIndexAgilityList = new List<Tuple<Allegiance, int, float>>();
		NumTurns = new Dictionary<int, int>();
		for (int i = 0; i < Combatants.Count; ++i)
		{
			factionIndexAgilityList.Add(new Tuple<Allegiance, int, float>(Combatants[i].Allegiance, i, Combatants[i].Agility));
			NumTurns.Add(Combatants[i].BattleID, 0);
		}

		// Sort list by agility descending
		factionIndexAgilityList.Sort((x, y) => y.Item3.CompareTo(x.Item3));

		// Find the lowest agility in this battle
		float lowestAgility = factionIndexAgilityList[factionIndexAgilityList.Count - 1].Item3;
		
		// Keep halving agilities and assigning turn order until everyone's
		// turns have been assigned
		while (factionIndexAgilityList.Count > 0)
		{
			// Sort by agility
			factionIndexAgilityList.Sort((x, y) => y.Item3.CompareTo(x.Item3));
			// Add the highest agility combatant to the turn list
			var candidate = factionIndexAgilityList[0];
			TurnOrderCombatantIDs.Add(Combatants[candidate.Item2].BattleID);
			// Halve their agility stat
			factionIndexAgilityList[0] = new Tuple<Allegiance, int, float>(candidate.Item1, candidate.Item2, candidate.Item3 / 2f);
			// Remove them if their agility has dropped below the lowest
			if (factionIndexAgilityList[0].Item3 < lowestAgility)
			{
				factionIndexAgilityList.RemoveAt(0);
			}
		}
	}

	public void ChangeTurnOrder(){
		TurnOrderCombatantIDs = new List<int>();
		TurnOrderIndex = 0;

		// For each combatant, store a copy of their faction, index, and agility
		// stat (just to make sorting easier)
		List<Tuple<Allegiance, int, float>> factionIndexAgilityList = new List<Tuple<Allegiance, int, float>>();
		for (int i = 0; i < Combatants.Count; ++i)
		{
			int TurnsTaken = NumTurns[Combatants[i].BattleID];
			if (TurnsTaken == 0)
			{
				factionIndexAgilityList.Add(new Tuple<Allegiance, int, float>(Combatants[i].Allegiance, i, Combatants[i].Agility));
			} 
			else 
			{
				factionIndexAgilityList.Add(new Tuple<Allegiance, int, float>(Combatants[i].Allegiance, i, Combatants[i].Agility / 2 * TurnsTaken));
			}

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
			TurnOrderCombatantIDs.Add(Combatants[candidate.Item2].BattleID);
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
		Debug.Assert(CurrCombatant.Allegiance == Allegiance.ENEMY);

		EnemyController enemy = (EnemyController)CurrCombatant;
		// Carry out the enemy's turn
		enemy.DoTurn();

		// If the next turn is an enemy too, recursively execute their turn
		TurnOrderIndex++;
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
}
