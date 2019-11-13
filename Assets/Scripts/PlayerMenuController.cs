using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Holds the state required for processing root menu items
internal class RootMenuItem
{
	private GameObject menuItem;
	private RectTransform rectTransform;
	private TextMeshProUGUI textMeshPro;
	private Action Callback;

	internal RootMenuItem(GameObject menuItem, RectTransform rectTransform, TextMeshProUGUI textMeshPro, Action callback) {
		this.menuItem = menuItem;
		this.rectTransform = rectTransform;
		this.textMeshPro = textMeshPro;
		Callback = callback;
	}

	internal void Highlight(Color color) {
		textMeshPro.fontSize *= 1.2f;
		textMeshPro.color = color;
	}

	internal void ChangeHighlightColor(Color color) {
		textMeshPro.color = color;
	}

	internal void Unhighlight() {
		textMeshPro.fontSize /= 1.2f;
		textMeshPro.color = Color.white;
	}

	internal void OnSelect() {
		Callback();
	}
}

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


	// List of root menu items
	private List<RootMenuItem> rootMenuItems;

	// Index of the highlighted root menu item
	private int rootMenuItemIndex;


	// Index of the hero the menu is currently attached to
	private int currHeroIndex;

	// Ability list for each hero (calm and discord)
	private List<List<List<Ability>>> heroAbilityLists;

	void Start() {
		InitialiseMenu();
		InitialiseHeroAbilityLists();
		currHeroIndex = battleController.HeroTurnIndex;
		rootMenuItemIndex = 0;
	}

	void Update() {
		// If required, move the player menu to be next to the current hero
		if(currHeroIndex != battleController.HeroTurnIndex) {
			currHeroIndex = battleController.HeroTurnIndex;

			playerMenu.transform.position = battleController.HeroCombatants[battleController.HeroTurnIndex].transform.position;
			playerMenuRectTransform.localPosition += new Vector3(playerMenuRectTransform.sizeDelta.x * 0.8f, 0f);

			// Since the hero's changed, the color of the highlighted menu item will change too
			UnhighlightMenuItem(rootMenuItemIndex);
			rootMenuItemIndex = 0;
			HighlightMenuItem(rootMenuItemIndex);
		}
		
		int oldRootMenuItemIndex = rootMenuItemIndex;
		// Handle navigating menus
		NavigateMenus();

		if (rootMenuItemIndex != oldRootMenuItemIndex) {
			UnhighlightMenuItem(oldRootMenuItemIndex);
			HighlightMenuItem(rootMenuItemIndex);
		}
	}

	// Initialise the root menu containing all the player actions
	private void InitialiseMenu() {
		// Initialise it
		playerMenu = Instantiate(playerMenuPrefab, gameUI.transform).transform;
		playerMenuRectTransform = playerMenu.GetComponent<RectTransform>();

		// Position it
		playerMenu.transform.position = battleController.HeroCombatants[battleController.HeroTurnIndex].transform.position;
		playerMenuRectTransform.localPosition += new Vector3(playerMenuRectTransform.sizeDelta.x * 0.8f, 0f);

		// Populate it with root level menu items
		rootMenuItems = new List<RootMenuItem>();
		AddMenuItem("ATTACK", 0, new Action(() => {
			Debug.Log("Clicked on the ATTACK button!");
		}));
		AddMenuItem("DISCORD", 1, new Action(() => {
			Debug.Log("Clicked on the DISCORD button!");
		}));
		AddMenuItem("CALM", 2, new Action(() => {
			Debug.Log("Clicked on the CALM button!");
		}));
		AddMenuItem("GUARD", 3, new Action(() => {
			Debug.Log("Clicked on the GUARD button!");
		}));

		// Highlight the first item
		rootMenuItems[0].Highlight(battleController.HeroCombatants[battleController.HeroTurnIndex].Color);
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

	private void AddMenuItem(string text, int itemNum, Action callback) {
		GameObject menuItem = Instantiate(menuItemPrefab, playerMenu);
		RectTransform rectTransform = menuItem.GetComponent<RectTransform>();
		TextMeshProUGUI textMeshPro = menuItem.GetComponent<TextMeshProUGUI>();
		float margin = 10f;

		menuItem.transform.localPosition -= new Vector3(0f, itemNum * (rectTransform.sizeDelta.y + margin));
		textMeshPro.SetText(text);

		// Create a RootMenuItem object and add it to the list
		rootMenuItems.Add(new RootMenuItem(menuItem, rectTransform, textMeshPro, callback));
	}

	private void NavigateMenus() {
		if(Input.GetKeyDown(KeyCode.DownArrow)) {
			rootMenuItemIndex = (rootMenuItemIndex + 1) % rootMenuItems.Count;
		} else if(Input.GetKeyDown(KeyCode.UpArrow)) {
			rootMenuItemIndex = (rootMenuItems.Count + rootMenuItemIndex - 1) % rootMenuItems.Count;
		} else if(Input.GetKeyDown(KeyCode.Space)) {
			rootMenuItems[rootMenuItemIndex].OnSelect();
		}
	}

	private void HighlightMenuItem(int index) {
		rootMenuItems[index].Highlight(battleController.HeroCombatants[currHeroIndex].Color);
	}

	private void UnhighlightMenuItem(int index) {
		rootMenuItems[index].Unhighlight();
	}
}
