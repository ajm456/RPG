using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the turn order indicator  found at the bottom of the battle UI.
/// </summary>
public class TurnOrderUIController : MonoBehaviour
{
	[SerializeField] private Transform container;
	[SerializeField] private GameObject nameEntryPrefab;
	[SerializeField] private BattleController battleController;

	/// <summary>
	/// A list of each currently visible turn indicator game object.
	/// </summary>
	private List<GameObject> entryObjects;

	/// <summary>
	/// A cache of the last turn order index value we read from the battle
	/// controller.
	/// </summary>
	private int currTurnOrderIndex;

	protected void Start()
	{
		// Initialise prefab entries
		entryObjects = new List<GameObject>(battleController.GetNumCombatants());
		ConstructTurnOrderEntries();
		currTurnOrderIndex = 0;
	}

	protected void Update()
	{
		// If the turn order index has changed, we need to update the entries
		if (battleController.TurnOrderIndex != currTurnOrderIndex)
		{
			currTurnOrderIndex = battleController.TurnOrderIndex;

			// Pop the left-most entry
			Destroy(entryObjects[0]);
			entryObjects.RemoveAt(0);

			if (currTurnOrderIndex != 0)
			{
				// If it's not the end of the round, we just need to shift the
				// remaining entries left
				foreach (GameObject obj in entryObjects)
				{
					RectTransform transform = obj.GetComponent<RectTransform>();
					transform.localPosition -= new Vector3(transform.sizeDelta.x, 0f);
				}
			}
			else
			{
				// It's the start of a new round so we need to reconstruct the
				// entries
				ConstructTurnOrderEntries();
			}
		}
	}

	/// <summary>
	/// Instantiates and initialises turn order entry game objects for each
	/// combatant in the current battle.
	/// </summary>
	private void ConstructTurnOrderEntries()
	{
		// Used to keep track of where we placed the last entry; subsequent
		// entries must be placed to the right of it
		Vector3? lastPos = null;
		
		foreach (string name in battleController.GetOrderedCombatantNames())
		{
			GameObject nameObject = Instantiate(nameEntryPrefab, container.transform);
			nameObject.GetComponent<TextMeshProUGUI>().SetText(name);
			// lastPos is null when we initialise the first entry
			if (lastPos != null)
			{
				nameObject.GetComponent<RectTransform>().localPosition = lastPos.Value + new Vector3(nameObject.GetComponent<RectTransform>().sizeDelta.x, 0f);
			}
			lastPos = nameObject.GetComponent<RectTransform>().localPosition;
			entryObjects.Add(nameObject);
		}
	}
}
