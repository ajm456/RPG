using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMenuController : MonoBehaviour
{
	// Prefabs
	[SerializeField] private GameObject playerMenuPrefab, menuItemPrefab;

	// Other scene components
	[SerializeField] private BattleController battleController;
	[SerializeField] private Canvas gameUI;


	// Root player menu that moves between heroes
	private Transform playerMenu;
	private RectTransform playerMenuRectTransform;

	// List of actions corresponding to each root menu
	private List<Action> rootMenuItemOnClicks;

	// Index of the highlighted root menu item
	private int rootMenuItemIndex;


	// Index of the hero the menu is currently attached to
	private int currHeroIndex;

	// Ability list for each hero (calm and discord)
	private List<List<List<Ability>>> heroAbilityLists;

	void Start() {
		InitialiseMenu();
		InitialiseHeroAbilityLists();
		currHeroIndex = -1;
		rootMenuItemIndex = 0;
	}

	void Update() {
		// If required, move the player menu to be next to the current hero
		if(currHeroIndex != battleController.HeroTurnIndex) {
			currHeroIndex = battleController.HeroTurnIndex;

			playerMenu.transform.position = battleController.HeroCombatants[battleController.HeroTurnIndex].transform.position;
			playerMenuRectTransform.localPosition += new Vector3(playerMenuRectTransform.sizeDelta.x * 0.8f, 0f);
		}
		
		// Handle navigating menus
		NavigateMenus();
	}

	// Initialise the root menu containing all the player actions
	private void InitialiseMenu() {
		// Initialise it
		playerMenu = Instantiate(playerMenuPrefab, gameUI.transform).transform;
		playerMenuRectTransform = playerMenu.GetComponent<RectTransform>();

		// Populate it with root level menu items
		AddMenuItem("ATTACK",	0);
		AddMenuItem("DISCORD",	1);
		AddMenuItem("CALM",		2);
		AddMenuItem("GUARD",	3);

		// Set the callback for each root menu item
		rootMenuItemOnClicks = new List<Action> {
			// Menu item 0: perform a standard attack
			new Action(() => {
				Debug.Log("Clicked on the ATTACK button!");
			}),
			// Menu item 1: open the discord abilities list
			new Action(() => {
				Debug.Log("Clicked on the DISCORD button!");
			}),
			// Menu item 2: open the calm abilities list
			new Action(() => {
				Debug.Log("Clicked on the CALM button!");
			}),
			// Menu item 3: guard
			new Action(() => {
				Debug.Log("Clicked on the GUARD button!");
			})
		};
	}

	// Cache the abilities of each hero
	private void InitialiseHeroAbilityLists() {
		heroAbilityLists = new List<List<List<Ability>>>();
		// For each hero, load their abilities
		foreach(HeroController hero in battleController.HeroCombatants) {
			// Calm abilities
			List<Ability> calmAbilities = hero.CalmAbilities;
			// Discord abilities
			List<Ability> discordAbilities = hero.DiscordAbilities;
			// Group em up
			List<List<Ability>> allAbilities = new List<List<Ability>> {
				calmAbilities,
				discordAbilities
			};
			// And put em away
			heroAbilityLists.Add(allAbilities);
		}
	}

	private void AddMenuItem(string text, int itemNum) {
		GameObject menuItem = Instantiate(menuItemPrefab, playerMenu);
		RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
		TextMeshProUGUI textMeshPro = menuItem.GetComponent<TextMeshProUGUI>();
		float margin = 10f;

		menuItem.transform.localPosition -= new Vector3(0f, itemNum * (rectTransform.sizeDelta.y + margin));
		textMeshPro.SetText(text);
	}

	private void NavigateMenus() {
		if(Input.GetKeyDown(KeyCode.DownArrow)) {
			rootMenuItemIndex = (rootMenuItemIndex + 1) % rootMenuItemOnClicks.Count;
		} else if(Input.GetKeyDown(KeyCode.UpArrow)) {
			rootMenuItemIndex = (rootMenuItemOnClicks.Count + rootMenuItemIndex - 1) % rootMenuItemOnClicks.Count;
		} else if(Input.GetKeyDown(KeyCode.Space)) {
			rootMenuItemOnClicks[rootMenuItemIndex]();
		}
	}
}
