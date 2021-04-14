using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the turn order indicator  found at the bottom of the battle UI.
/// </summary>
public class TurnOrderUIController : MonoBehaviour
{
	[SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject nameEntryPrefab;

	[SerializeField]
	private BattleController battleController;

	/// <summary>
	/// A list of each currently visible turn indicator game object.
	/// </summary>
	private List<GameObject> entryObjects;

	private int numActiveEntries;

	/// <summary>
	/// A cache of the last turn order index value we read from the battle
	/// controller.
	/// </summary>
	private int currTurnOrderIndex;

	private List<int> currTurnOrderedCombatantIDs;

	protected void Start()
	{
		currTurnOrderedCombatantIDs = new List<int>(battleController.TurnOrderCombatantIDs);
		currTurnOrderIndex = 0;

		// Initialise prefab entries
		ConstructEntryObjects();
		InitialiseTurnOrderEntries();
	}

	protected void Update()
	{
		// If the turn order index has changed, we need to update the entries
		if (battleController.TurnOrderIndex != currTurnOrderIndex)
		{
			currTurnOrderIndex = battleController.TurnOrderIndex;

			// We need to check if the turn ordered combatant IDs list has
			// changed due to an agility (de)buff
			bool orderChanged = !currTurnOrderedCombatantIDs.SequenceEqual(battleController.TurnOrderCombatantIDs);

			if (orderChanged)
			{
				// No choice but to do a full rebuild
				Debug.Log("TURN ORDER CHANGED! Re-initialising UI");
				ConstructEntryObjects();
				InitialiseTurnOrderEntries();
				currTurnOrderedCombatantIDs = new List<int>(battleController.TurnOrderCombatantIDs);
			}
			else
			{
				// If the order didn't change, adjusting the turn order entries
				// is a lot simpler

				if (currTurnOrderIndex == 0)
				{
					// This is a new round so reinitialise
					InitialiseTurnOrderEntries();
				}
				else
				{
					// This isn't a new round, so we just need to adjust the
					// text of each entry object

					// Hide the rightmost entry
					entryObjects[numActiveEntries - 1].SetActive(false);
					--numActiveEntries;
				
					// The text of each entry becomes the text of its right
					// neighbour's
					for (var i = 0; i < numActiveEntries; ++i)
					{
						entryObjects[i].GetComponent<TextMeshProUGUI>().text = entryObjects[i + 1].GetComponent<TextMeshProUGUI>().text;
					}
				}
			}
		}
	}

	private void ConstructEntryObjects()
	{
		// Clear entryObjects if it was previously populated
		if (entryObjects != null)
		{
			foreach (GameObject obj in entryObjects)
			{
				Destroy(obj);
			}
		}
		
		entryObjects = new List<GameObject>(battleController.GetNumCombatants());

		// Used to keep track of where we placed the last entry; subsequent
		// entries must be placed to the right of it
		Vector3? lastPos = null;
		
		for (var i = 0; i < battleController.TurnOrderCombatantIDs.Count; ++i)
		{
			GameObject nameObject = Instantiate(nameEntryPrefab, container.transform);
			// lastPos is null when we initialise the first entry
			if (lastPos != null)
			{
				nameObject.GetComponent<RectTransform>().localPosition = lastPos.Value + new Vector3(nameObject.GetComponent<RectTransform>().sizeDelta.x, 0f);
			}
			lastPos = nameObject.GetComponent<RectTransform>().localPosition;

			// Fade out every entry past the 3rd
			if (i >= 3)
			{
				nameObject.GetComponent<TextMeshProUGUI>().canvasRenderer.SetAlpha(1f - (i - 3)*0.25f);
			}

			entryObjects.Add(nameObject);

			// Disable any hidden entries for turns already taken
			if (i >= battleController.TurnOrderCombatantIDs.Count - battleController.TurnOrderIndex)
			{
				nameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Instantiates and initialises turn order entry game objects for each
	/// combatant in the current battle.
	/// </summary>
	private void InitialiseTurnOrderEntries()
	{
		numActiveEntries = currTurnOrderedCombatantIDs.Count;

		if (battleController.GetNumTurnsPerRound() != entryObjects.Count)
		{
			Debug.Log("Ordered combatant ID and entry object list counts not identical!");
			Debug.Break();
		}

		List<string> orderedCombatantNames = battleController.GetOrderedCombatantNames();
		for (var i = 0; i < numActiveEntries; ++i)
		{
			entryObjects[i].GetComponent<TextMeshProUGUI>().SetText(orderedCombatantNames[i]);
			entryObjects[i].SetActive(true);
		}

		numActiveEntries = entryObjects.Count;
	}
}
