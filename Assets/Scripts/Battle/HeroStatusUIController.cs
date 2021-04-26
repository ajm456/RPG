using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

class StatusMenu
{
	internal int heroID;
	internal bool isProtagonist;
	internal Transform rootTransform;
	internal TextMeshProUGUI nameText, hpText, hpValue;
	internal RectTransform healthBar, missingHealthBar, calmBar, missingCalmBar, strifeBar, missingStrifeBar;
	internal TextMeshProUGUI resourceVal, calmVal, strifeVal;

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

#pragma warning disable 0649
	[SerializeField]
	private float yOffset;

	[SerializeField] 
	private GameObject statusMenuPrefab;

	[SerializeField] 
	private Transform container;

	[SerializeField] 
	private BattleController battleController;
#pragma warning restore 0649

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
			statusMenu.transform.localPosition = new Vector3(statusMenu.transform.localPosition.x, statusMenu.transform.localPosition.y - (i * yOffset));

			StatusMenu status = new StatusMenu();
			status.heroID = battleController.GetNthHeroID(i);
			status.rootTransform = statusMenu.transform;
			status.nameText = statusMenu.transform.Find("Name").GetComponent<TextMeshProUGUI>();
			status.hpText = statusMenu.transform.Find("HPText").GetComponent<TextMeshProUGUI>();
			status.hpValue = statusMenu.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
			status.healthBar = statusMenu.transform.Find("HealthBar").GetComponent<RectTransform>();
			status.missingHealthBar = status.healthBar.transform.Find("MissingHealthBar").GetComponent<RectTransform>();
			
			if (battleController.GetCombatantName(status.heroID).ToLower() == "jack")
			{
				status.isProtagonist = true;

				// Handle Jack's separate Calm and Strife resources
				status.calmVal = statusMenu.transform.Find("JackCalmValue").GetComponent<TextMeshProUGUI>();
				statusMenu.transform.Find("JackCalmValue").gameObject.SetActive(true);
				status.strifeVal = statusMenu.transform.Find("JackStrifeValue").GetComponent<TextMeshProUGUI>();
				statusMenu.transform.Find("JackStrifeValue").gameObject.SetActive(true);
				statusMenu.transform.Find("ResourceValue").gameObject.SetActive(false);
			}
			else
			{
				status.isProtagonist = false;

				// Other characters only have a single resource value label
				status.resourceVal = statusMenu.transform.Find("ResourceValue").GetComponent<TextMeshProUGUI>();
			}
			status.calmBar = statusMenu.transform.Find("CalmBar").GetComponent<RectTransform>();
			status.missingCalmBar = status.calmBar.transform.Find("MissingCalmBar").GetComponent<RectTransform>();
			status.strifeBar = statusMenu.transform.Find("StrifeBar").GetComponent<RectTransform>();
			status.missingStrifeBar = status.strifeBar.transform.Find("MissingStrifeBar").GetComponent<RectTransform>();

			status.startTime = 0f;
			status.startPos = statusMenu.transform.localPosition;
			status.endPos = statusMenu.transform.localPosition + new Vector3(ANIM_DELTA_X, 0f);
			status.expanded = false;
			menus.Add(status);

			// Set the text to our hero name
			status.nameText.SetText(battleController.GetCombatantName(status.heroID).ToUpper());
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
			float newWidth = percentageMissing * menu.healthBar.rect.width;
			menu.missingHealthBar.sizeDelta = new Vector2(newWidth, menu.missingHealthBar.sizeDelta.y);

			// Update the resource value numerical indicator
			if (menu.isProtagonist)
			{
				menu.calmVal.SetText(battleController.GetHeroCalm(menu.heroID).ToString());
				menu.strifeVal.SetText(battleController.GetHeroStrife(menu.heroID).ToString());
			}
			else
			{
				int currResource = battleController.GetHeroResource(menu.heroID);
				if (currResource > 0)
				{
					menu.resourceVal.color = menu.strifeBar.GetComponent<Image>().color;
					float newXPos = menu.strifeBar.rect.width - menu.missingStrifeBar.rect.width;
					menu.resourceVal.rectTransform.localPosition = new Vector3(newXPos, menu.resourceVal.rectTransform.localPosition.y);
				}
				else if (currResource < 0)
				{
					menu.resourceVal.color = menu.calmBar.GetComponent<Image>().color;
					float newXPos = menu.missingCalmBar.rect.width - menu.calmBar.rect.width;
					menu.resourceVal.rectTransform.localPosition = new Vector3(newXPos, menu.resourceVal.rectTransform.localPosition.y);
				}
				else
				{
					menu.resourceVal.color = Color.white;
				}

				menu.resourceVal.SetText(Mathf.Abs(currResource).ToString());
				
			}
			// Update the resource masks
			float calmWidthPercentage = (100f - battleController.GetHeroCalm(menu.heroID)) / 100f;
			float strifeWidthPercentage = (100f - battleController.GetHeroStrife(menu.heroID)) / 100f;
			float newCalmWidth = calmWidthPercentage * menu.calmBar.sizeDelta.x;
			float newStrifeWidth = strifeWidthPercentage * menu.strifeBar.sizeDelta.x;
			menu.missingCalmBar.sizeDelta = new Vector2(newCalmWidth, menu.missingCalmBar.sizeDelta.y);
			menu.missingStrifeBar.sizeDelta = new Vector2(newStrifeWidth, menu.missingStrifeBar.sizeDelta.y);

			// Highlight this menu if it's the hero's turn
			if (i == battleHeroNames.IndexOf(battleController.CurrCombatantName)
				&& !battleController.IsCombatantAnimating(battleController.CurrCombatantID)
				&& battleController.State == BattleController.BattleState.PLAYERCHOICE)
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
