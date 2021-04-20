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
#pragma warning restore 0649

	/// <summary>
	/// A list of each currently visible turn indicator game object.
	/// </summary>
	private List<GameObject> entryObjects;

	/// <summary>
	/// A cache of the last current combatant ID we read from the battle
	/// controller.
	/// </summary>
	private int currTurnNum;

	protected void Start()
	{
		currTurnNum = -1;

		// Initialise prefab entries
		ConstructEntryObjects();
		PopulateTurnOrderEntries();
	}

	protected void Update()
	{
		// If the current combatant ID has changed, we need to update the entries
		if (battleController.TurnNum != currTurnNum)
		{
			currTurnNum = battleController.TurnNum;

			PopulateTurnOrderEntries();
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
	private void PopulateTurnOrderEntries()
	{
		// Grab the combatant names for enough rounds to fill out
		// the turn entry list and then some
		List<string> entryTexts = battleController.GetOrderedCombatantNames();

		// Fill in the entry objects with as many names as we can
		for (var i = 0; i < numEntryObjects; ++i)
		{
			entryObjects[i].GetComponent<TextMeshProUGUI>().SetText(entryTexts[i]);
			entryObjects[i].SetActive(true);
		}
	}
}
