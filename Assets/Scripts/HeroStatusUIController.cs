using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

class StatusMenu
{
	internal int heroID;
	internal Transform rootTransform;
	internal TextMeshProUGUI nameText, hpText, hpValue;
	internal RectTransform healthBar, missingHealthBar;

	// Lerp parameters
	internal float startTime;
	internal Vector3 startPos, endPos;
	internal bool expanded;
}

public class HeroStatusUIController : MonoBehaviour
{
	private const float ANIM_DURATION_FULL = 0.4f;
	private const float ANIM_DELTA_X = 30f;
	private const float ANIM_SPEED = ANIM_DELTA_X / ANIM_DURATION_FULL;

	[SerializeField] private GameObject statusMenuPrefab;
	[SerializeField] private Transform container;
	[SerializeField] private BattleController battleController;

	private List<StatusMenu> menus;
	private List<string> battleHeroNames;

	void Start()
	{
		menus = new List<StatusMenu>(battleController.GetNumHeroes());
		battleHeroNames = battleController.GetHeroNames();

		// Instantiate a status menu for each hero in this battle
		for (var i = 0; i < battleController.GetNumHeroes(); ++i)
		{
			GameObject statusMenu = Instantiate(statusMenuPrefab, container.transform);
			float yOffset = i * 90f;
			statusMenu.transform.localPosition = new Vector3(statusMenu.transform.localPosition.x, statusMenu.transform.localPosition.y - yOffset);

			StatusMenu status = new StatusMenu();
			status.heroID = battleController.GetNthHeroID(i);
			Debug.Log("Just set a StatusMenu's heroID to " + status.heroID);
			status.rootTransform = statusMenu.transform;
			status.nameText = statusMenu.transform.Find("Name").GetComponent<TextMeshProUGUI>();
			status.hpText = statusMenu.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
			status.hpValue = statusMenu.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
			status.healthBar = statusMenu.transform.Find("HealthBar").GetComponent<RectTransform>();
			status.missingHealthBar = status.healthBar.transform.Find("MissingHealthBar").GetComponent<RectTransform>();
			status.startTime = 0f;
			status.startPos = statusMenu.transform.localPosition;
			status.endPos = statusMenu.transform.localPosition + new Vector3(ANIM_DELTA_X, 0f);
			status.expanded = false;
			menus.Add(status);

			// Set the text to our hero name
			status.nameText.SetText(battleController.GetCombatantName(status.heroID));
		}
	}

	void Update()
	{
		// Update the contents of the UI elements
		for (var i = 0; i < menus.Count; ++i)
		{
			StatusMenu menu = menus[i];

			// Get all the data we need
			int hp = battleController.GetCombatantHP(menu.heroID);
			int maxHp = battleController.GetCombatantMaxHP(menu.heroID);
			Color color = battleController.GetHeroColor(menu.heroID);

			// Update the hit point numerical indicator
			menu.hpValue.SetText(hp.ToString() + " / " + maxHp.ToString());

			// Update the missing health mask
			float missingHealth =  battleController.GetCombatantMaxHP(menu.heroID) - battleController.GetCombatantHP(menu.heroID);
			float percentageMissing = missingHealth / maxHp;
			float newWidth = percentageMissing * menu.healthBar.sizeDelta.x;
			menu.missingHealthBar.sizeDelta = new Vector2(newWidth, menu.missingHealthBar.sizeDelta.y);

			// Highlight this menu if it's the hero's turn
			if (i == battleHeroNames.IndexOf(battleController.CurrCombatantName))
			{
				// Set the colours
				menu.nameText.color = color;
				menu.hpText.color = color;
				menu.hpValue.color = color;
				menu.healthBar.GetComponent<Image>().color = color;

				// Lerp animate to expanded position
				if (!menu.expanded)
				{
					menu.startTime = Time.time;
					menu.expanded = true;
				}
				float distCovered = (Time.time - menu.startTime) * ANIM_SPEED;
				float journeyLength = Vector3.Distance(menu.rootTransform.localPosition, menu.endPos);
				if (journeyLength > 0f)
				{
					float fractionOfJourney = distCovered / journeyLength;
					menu.rootTransform.localPosition = Vector3.Lerp(menu.startPos, menu.endPos, fractionOfJourney);
				}
			}
			else
			{
				// Set the colours
				menu.nameText.color = Color.white;
				menu.hpText.color = Color.white;
				menu.hpValue.color = Color.white;
				menu.healthBar.GetComponent<Image>().color = Color.white;

				// Lerp to non-animated position
				if (menu.expanded)
				{
					menu.startTime = Time.time;
					menu.expanded = false;
				}
				float distCovered = (Time.time - menu.startTime) * ANIM_SPEED;
				float journeyLength = Vector3.Distance(menu.rootTransform.localPosition, menu.startPos);
				if (journeyLength > 0f)
				{
					float fractionOfJourney = distCovered / journeyLength;
					menu.rootTransform.localPosition = Vector3.Lerp(menu.endPos, menu.startPos, fractionOfJourney);
				}
			}
		}
	}
}
