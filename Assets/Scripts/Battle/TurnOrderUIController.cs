using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the turn order indicator  found at the bottom of the battle UI.
/// </summary>
public class TurnOrderUIController : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject nameEntryPrefab;

	[SerializeField]
	private BattleController battleController;

	[SerializeField]
	private int numEntryObjects;

	[SerializeField]
	private string roundEndText;
#pragma warning restore 0649

	/// <summary>
	/// A list of each currently visible turn indicator game object.
	/// </summary>
	private List<GameObject> entryObjects;

	/// <summary>
	/// A cache of the last turn order index value we read from the battle
	/// controller.
	/// </summary>
	private int currTurnOrderIndex;

	private List<int> currTurnOrderedCombatantIDs;

	private List<string> entryTexts;

	protected void Start()
	{
		currTurnOrderedCombatantIDs = new List<int>(battleController.TurnOrderCombatantIDs);
		currTurnOrderIndex = 0;
		entryTexts = new List<string>();

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
				//ConstructEntryObjects();
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
				
					// The text of each entry becomes the text of its right
					// neighbour's
					for (var i = currTurnOrderIndex; i < entryObjects.Count; ++i)
					{
						entryObjects[i - currTurnOrderIndex].GetComponent<TextMeshProUGUI>().text = entryTexts[i];
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

		entryObjects = new List<GameObject>(numEntryObjects);

		// Used to keep track of where we placed the last entry; subsequent
		// entries must be placed to the right of it
		Vector3? lastPos = null;
		
		for (var i = 0; i < numEntryObjects; ++i)
		{
			GameObject nameObject = Instantiate(nameEntryPrefab, container.transform);
			// lastPos is null when we initialise the first entry
			if (lastPos != null)
			{
				nameObject.GetComponent<RectTransform>().localPosition = lastPos.Value + new Vector3(nameObject.GetComponent<RectTransform>().sizeDelta.x, 0f);
			}
			lastPos = nameObject.GetComponent<RectTransform>().localPosition;

			// Fade out every entry past the 5th
			if (i >= 5)
			{
				nameObject.GetComponent<TextMeshProUGUI>().canvasRenderer.SetAlpha(1f - (i - 3)*0.15f);
			}

			entryObjects.Add(nameObject);
		}
	}

	/// <summary>
	/// Generates the list of combatant names for this and future rounds, and 
	/// fills in the turn order entry objects with said names.
	/// </summary>
	private void InitialiseTurnOrderEntries()
	{
		// Grab the combatant names for enough rounds to fill out
		// the turn entry list and then some
		entryTexts = battleController.GetOrderedCombatantNames();
		do
		{
			entryTexts.AddRange(battleController.GetOrderedCombatantNamesForNextRound());
		}
		while (entryTexts.Count <= numEntryObjects);

		// Fill in the entry objects with as many names as we can
		for (var i = 0; i < numEntryObjects; ++i)
		{
			entryObjects[i].GetComponent<TextMeshProUGUI>().SetText(entryTexts[i]);
			entryObjects[i].SetActive(true);
		}
	}
}
