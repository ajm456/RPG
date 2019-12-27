using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Holds the state required for a menu.
/// </summary>
internal class Menu
{
	// The local distance between each menu item
	private static readonly float ITEM_MARGIN = 10f;

	private GameObject menu;
	private List<MenuItem> items;

	internal Transform Transform
	{
		get
		{
			return menu.transform;
		}
	}
	internal RectTransform RectTransform
	{
		get;
	}
	internal int NumItems
	{
		get
		{
			return items.Count;
		}
	}


	/// <summary>
	/// Initialise a Menu object.
	/// </summary>
	/// <param name="menu">The (instantiated) GameObject representing this Menu in the game world.</param>
	internal Menu(GameObject menu)
	{
		this.menu = menu;
		RectTransform = menu.GetComponent<RectTransform>();
		items = new List<MenuItem>();
	}


	/// <summary>
	/// Adds a MenuItem object to this menu's list of items.
	/// </summary>
	/// <param name="item">The MenuItem object to add</param>
	internal void AddMenuItem(MenuItem item)
	{
		// Position it underneath all other menu items
		item.transform.localPosition -= new Vector3(0f, items.Count * (item.rectTransform.sizeDelta.y + ITEM_MARGIN));
		items.Add(item);
	}


	/// <summary>
	/// <para>
	/// Clears the list of MenuItem objects this Menu object holds.
	/// </para>
	/// <para>
	/// This does NOT destroy game objects, which must be done in a
	/// MonoBehaviour object. See <see cref="GetMenuItems"/> to get a list of
	/// GameObject objects to call Destroy on.
	/// </para>
	/// </summary>
	internal void ClearMenuItems()
	{
		items = new List<MenuItem>();
	}


	/// <summary>
	/// Returns an IEnumerable containing each menu item's GameObject.
	/// </summary>
	/// <returns>An IEnumerable of menu item GameObjects.</returns>
	internal IEnumerable<GameObject> GetMenuItems()
	{
		foreach (MenuItem item in items)
		{
			yield return item.GameObj;
		}
	}


	/// <summary>
	/// Set whether this menu GameObject is active or not.
	/// </summary>
	/// <param name="enabled"></param>
	internal void SetActive(bool enabled)
	{
		menu.SetActive(enabled);
	}


	/// <summary>
	/// Highlights the menu item at a given index with a given color.
	/// Highlighted menu items are larger in size and colored differently to
	/// non-highlighted items.
	/// </summary>
	/// <param name="index">The index of the menu item to highlight.</param>
	/// <param name="color">The color to set the menu item.</param>
	internal void HighlightMenuItem(int index, Color color)
	{
		Debug.Assert(index < items.Count, "Trying to highlight index " + index + " but there are only " + items.Count + " items in the menu!");
		items[index].Highlight(color);
	}


	/// <summary>
	/// Unhighlights the menu item at a given index. See
	/// <see cref="HighlightMenuItem(int, Color)"/>.
	/// </summary>
	/// <param name="index">The index of the menu item to unhighlight</param>
	internal void UnhighlightMenuItem(int index)
	{
		Debug.Assert(index < items.Count, "Trying to unhighlight index " + index + " but there are only " + items.Count + " items in the menu!");
		items[index].Unhighlight();
	}


	/// <summary>
	/// Executes the callback of the item at a given index.
	/// </summary>
	/// <param name="index">The index of the item to select.</param>
	internal void SelectItem(int index)
	{
		Debug.Assert(index < items.Count, "Trying to select index " + index + " but there are only " + items.Count + " items in the menu!");
		items[index].OnSelect();
	}


	/// <summary>
	/// Returns whether or not the menu item at a given index is currently
	/// enabled or not. Disabled menu items cannot be selected and appear
	/// faded.
	/// </summary>
	/// <param name="index">The index of the menu item being examined.</param>
	/// <returns>Whether or not the menu item is currently enabled.</returns>
	internal bool IsItemEnabled(int index)
	{
		Debug.Assert(index < items.Count, "Trying to check enabled status of index " + index + " but there are only " + items.Count + " items in the menu!");
		return items[index].IsEnabled();
	}


	/// <summary>
	/// Enables/disables the menu item at a given index. Disabled menu items
	/// cannot be selected and appear faded.
	/// </summary>
	/// <param name="index">The index of the item being enabled/disabled.</param>
	/// <param name="enabled">Whether to enable or disable the item.</param>
	internal void SetItemEnabled(int index, bool enabled)
	{
		Debug.Assert(index < items.Count, "Trying to enable/disable index " + index + " but there are only " + items.Count + " items in the menu!");
		items[index].SetEnabled(enabled);
	}
}


/// <summary>
/// Holds the state required for a menu item.
/// </summary>
internal class MenuItem
{
	private TextMeshProUGUI textMeshPro;
	private Action callback;
	private bool enabled;

	internal GameObject GameObj
	{
		get;
	}
	internal Transform transform
	{
		get
		{
			return GameObj.transform;
		}
	}
	internal RectTransform rectTransform
	{
		get;
	}


	/// <summary>
	/// Initialises a MenuItem object.
	/// </summary>
	/// <param name="gameObj">The (instantiated) GameObject representing this menu item in the game world.</param>
	/// <param name="text">The text this menu item displays.</param>
	/// <param name="callback">The callback to execute when this menu item is selected.</param>
	internal MenuItem(GameObject gameObj, string text, Action callback)
	{
		GameObj = gameObj;
		this.callback = callback;
		rectTransform = gameObj.GetComponent<RectTransform>();
		textMeshPro = gameObj.GetComponent<TextMeshProUGUI>();
		textMeshPro.SetText(text);
		enabled = true;
	}


	/// <summary>
	/// Highlights this menu item with a given color. Highlighted menu items
	/// are larger in size and colored differently to unhighlighted items.
	/// </summary>
	/// <param name="color">The color to set this menu item.</param>
	internal void Highlight(Color color)
	{
		if (enabled)
		{
			textMeshPro.fontSize *= 1.2f;
			textMeshPro.color = color;
		}
		else
		{
			Debug.LogWarning("Tried to highlight a disabled item!");
			Debug.Break();
		}
	}


	/// <summary>
	/// Unhighlights this menu item.
	/// </summary>
	internal void Unhighlight()
	{
		if (enabled)
		{
			textMeshPro.fontSize /= 1.2f;
			textMeshPro.color = Color.white;
		}
		else
		{
			Debug.LogWarning("Tried to unhighlight a disabled item!");
		}
	}


	/// <summary>
	/// Enables/disables this menu item. Disabled menu items cannot be
	/// selected.
	/// </summary>
	/// <param name="enabled">Whether to enable or disable this menu item.</param>
	internal void SetEnabled(bool enabled)
	{
		if (!enabled)
			textMeshPro.alpha = 0.3f;

		this.enabled = enabled;
	}


	/// <summary>
	/// Returns whether or not this menu item is currently enabled.
	/// </summary>
	/// <returns>Whether or not this menu item is currently enabled.</returns>
	internal bool IsEnabled()
	{
		return enabled;
	}


	/// <summary>
	/// Executes this MenuItem object's select callback.
	/// </summary>
	internal void OnSelect()
	{
		Debug.Assert(enabled, "Trying to execute the select callback of a disabled menu item...?");
		if (!enabled)
			Debug.Break();

		callback();
	}
}

public class PlayerMenuController : MonoBehaviour
{
	// Prefabs
	[SerializeField] private GameObject menuPrefab, menuItemPrefab;

	// Other scene components
	[SerializeField] private BattleController battleController;
	[SerializeField] private Canvas gameUI;


	// Root player menu that moves between heroes
	private Menu[] menus = new Menu[2];


	// Current cursor position in form [menu,item]
	private Vector2Int cursorPos;

	/// <summary>
	/// A list of names of all the heroes participating in this battle.
	/// </summary>
	private List<string> battleHeroNames;

	/// <summary>
	/// The index of the hero combatant the menu is currently attached to (this
	/// is not guaranteed to always be the same combatant as whose turn it is.)
	/// </summary>
	private int currHeroIndex;

	// Ability list for each hero (calm and discord)
	private List<List<List<AbilityData>>> heroAbilityLists;


	/// <summary>
	/// MonoBehaviour start.
	/// </summary>
	void Start()
	{
		InitialiseHeroAbilityLists();
		InitialiseRootMenu();
		InitialiseExtraMenu();
		battleHeroNames = new List<string>(battleController.HeroCombatants.Count);
		foreach (CombatantController comb in battleController.HeroCombatants)
			battleHeroNames.Add(comb.Name);	
		currHeroIndex = -1;
		cursorPos = new Vector2Int(0, 0);
	}


	/// <summary>
	/// MonoBehaviour update.
	/// </summary>
	void Update()
	{
		// If required, move the player menu to be next to the current hero
		if (battleController.State == BattleController.BattleState.PLAYERCHOICE)
		{
			// Check to see if we need to move the menu
			if (currHeroIndex != battleHeroNames.IndexOf(battleController.CurrCombatantName))
			{
				currHeroIndex = battleHeroNames.IndexOf(battleController.CurrCombatantName);

				menus[0].Transform.position = battleController.HeroCombatants[currHeroIndex].transform.position;
				menus[0].RectTransform.localPosition += new Vector3(menus[0].RectTransform.sizeDelta.x * 0.8f, 0f);

				// Destroy the old gameobjects and clear list data in the extra menu
				foreach (GameObject item in menus[1].GetMenuItems())
				{
					Destroy(item);
				}
				menus[1].ClearMenuItems();

				// Reposition the extra menu
				menus[1].Transform.position = menus[0].Transform.position;
				menus[1].Transform.localPosition = menus[0].Transform.localPosition + new Vector3(menus[0].RectTransform.sizeDelta.x, 0f);

				// Hide the extra menu
				menus[1].SetActive(false);

				// Disable menu items if we need to
				if (battleController.HeroCombatants[currHeroIndex].DiscordAbilities.Count == 0)
				{
					menus[0].SetItemEnabled(1, false);
				}
				if (battleController.HeroCombatants[currHeroIndex].CalmAbilities.Count == 0)
				{
					menus[0].SetItemEnabled(2, false);
				}

				// Since the hero's changed, the color of the highlighted menu item will change too
				if (cursorPos.x == 0)
					UnhighlightMenuItem(cursorPos);
				cursorPos.x = 0;
				cursorPos.y = 0;
				HighlightMenuItem(cursorPos);
			}
		}

		Vector2Int oldCursorPos = cursorPos;
		// Handle navigating menus
		NavigateMenus();

		if (cursorPos != oldCursorPos)
		{
			UnhighlightMenuItem(oldCursorPos);
			HighlightMenuItem(cursorPos);
		}
	}


	/// <summary>
	/// Initialise the root menu containing all the player actions.
	/// </summary>
	private void InitialiseRootMenu()
	{
		// Initialise it
		GameObject rootMenuObj = Instantiate(menuPrefab, gameUI.transform);
		menus[0] = new Menu(rootMenuObj);

		// Position it
		//menus[0].Transform.position = battleController.HeroCombatants[battleController.HeroTurnIndex].transform.position;
		//menus[0].RectTransform.localPosition += new Vector3(menus[0].RectTransform.sizeDelta.x * 0.8f, 0f);

		// Populate it with root level menu items
		menus[0].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[0].Transform), "ATTACK", new Action(OnSelectAttack)));
		menus[0].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[0].Transform), "DISCORD", new Action(OnSelectDiscord)));
		menus[0].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[0].Transform), "CALM", new Action(OnSelectCalm)));
		menus[0].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[0].Transform), "GUARD", new Action(OnSelectGuard)));

		// Highlight the first item
		HighlightMenuItem(0, 0);
	}


	/// <summary>
	/// Initialise the extra menu containing abilities.
	/// </summary>
	private void InitialiseExtraMenu()
	{
		// Initialise it
		GameObject extraMenuObj = Instantiate(menuPrefab, gameUI.transform);
		menus[1] = new Menu(extraMenuObj);

		// Position it
		menus[1].Transform.position = menus[0].Transform.position;
		menus[1].Transform.localPosition = menus[0].Transform.localPosition + new Vector3(menus[0].RectTransform.sizeDelta.x, 0f);

		// For now, hide the menu
		menus[1].SetActive(false);
	}


	/// <summary>
	/// Cache the ability lists of each hero.
	/// </summary>
	private void InitialiseHeroAbilityLists()
	{
		heroAbilityLists = new List<List<List<AbilityData>>>();
		// For each hero, load their abilities
		foreach (HeroController hero in battleController.HeroCombatants)
		{
			// Calm abilities
			List<AbilityData> calmAbilities = hero.CalmAbilities;
			// Discord abilities
			List<AbilityData> discordAbilities = hero.DiscordAbilities;
			// Group em up
			List<List<AbilityData>> allAbilities = new List<List<AbilityData>> {
				calmAbilities,
				discordAbilities
			};
			// And put em away
			heroAbilityLists.Add(allAbilities);
		}
	}


	/// <summary>
	/// Manages user navigation of the root and extra menu.
	/// </summary>
	private void NavigateMenus()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			// Keep navigating until we find an enabled menu item
			do
			{
				cursorPos.y = (cursorPos.y + 1) % menus[cursorPos.x].NumItems;
			} while (!menus[cursorPos.x].IsItemEnabled(cursorPos.y));
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			// Keep navigating until we find an enabled menu item
			do
			{
				cursorPos.y = (menus[cursorPos.x].NumItems + cursorPos.y - 1) % menus[cursorPos.x].NumItems;
			} while (!menus[cursorPos.x].IsItemEnabled(cursorPos.y));
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			// We don't do anything if the cursor is on the root menu
			if (cursorPos.x == 0)
				return;

			// Providing the cursor is on the extra menu, navigate left and close
			cursorPos.x -= 1;
			menus[1].SetActive(false);
		}
		else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
		{
			menus[cursorPos.x].SelectItem(cursorPos.y);
		}
	}


	/// <summary>
	/// Highlights the menu item at a given position.
	/// </summary>
	/// <param name="pos">The xy position of the item to highlight.</param>
	private void HighlightMenuItem(Vector2Int pos)
	{
		menus[pos.x].HighlightMenuItem(pos.y, battleController.HeroCombatants[currHeroIndex].Color);
	}


	/// <summary>
	/// Highlights the menu item at a given position.
	/// </summary>
	/// <param name="x">The x position of the item to highlight (aka which menu it's in).</param>
	/// <param name="y">The y position of the item to highlight.</param>
	private void HighlightMenuItem(int x, int y)
	{
		menus[x].HighlightMenuItem(y, battleController.HeroCombatants[currHeroIndex].Color);
	}


	/// <summary>
	/// Reverts the menu item at given position back to its original size and color.
	/// </summary>
	/// <param name="pos">xy position of the menu item to unhighlight.</param>
	private void UnhighlightMenuItem(Vector2Int pos)
	{
		menus[pos.x].UnhighlightMenuItem(pos.y);
	}


	/// <summary>
	/// Callback for selecting the attack menu item.
	/// </summary>
	private void OnSelectAttack()
	{
		// TODO: Determine which enemy index to attack
		battleController.ExecuteTurnWithAttack(battleController.HeroCombatants[currHeroIndex], battleController.EnemyCombatants[0]);

		// Let BattleController know we've finished taking our turn
		battleController.WaitingOnPlayerTurn = false;
	}


	/// <summary>
	/// Callback for selecting the discord abilities menu item.
	/// </summary>
	private void OnSelectDiscord()
	{
		// Destroy the old gameobjects before clearing list data
		foreach (GameObject item in menus[1].GetMenuItems())
		{
			Destroy(item);
		}
		menus[1].ClearMenuItems();

		// Create menu items for each of the hero's discord abilities
		for (var i = 0; i < heroAbilityLists[currHeroIndex][1].Count; ++i)
		{
			AbilityData ability = heroAbilityLists[currHeroIndex][1][i];
			menus[1].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[1].Transform), ability.name.ToUpper(), new Action(() =>
			{
				// TODO: Determine which enemy index to cast ability on
				battleController.ExecuteTurnWithAbility(ability, battleController.HeroCombatants[currHeroIndex], battleController.EnemyCombatants[0]);
			})));
		}

		// Move the cursor to the top of the list
		cursorPos.x = 1;
		cursorPos.y = 0;

		// Activate the menu
		menus[1].SetActive(true);
	}


	/// <summary>
	/// Callback for selecting the calm abilities menu item.
	/// </summary>
	private void OnSelectCalm()
	{
		// Destroy the old gameobjects before clearing list data
		foreach (GameObject item in menus[1].GetMenuItems())
		{
			Destroy(item);
		}
		menus[1].ClearMenuItems();

		for (var i = 0; i < heroAbilityLists[currHeroIndex][0].Count; ++i)
		{
			AbilityData ability = heroAbilityLists[currHeroIndex][0][i];
			menus[1].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[1].Transform), ability.name.ToUpper(), new Action(() => { Debug.Log(ability.name); })));
		}

		// Move the cursor to the top of the list
		cursorPos.x = 1;
		cursorPos.y = 0;

		// Activate the menu
		menus[1].SetActive(true);
	}


	/// <summary>
	/// Callback for selecting the guard menu item.
	/// </summary>
	private void OnSelectGuard()
	{
		Debug.Log("Selected guard!");
	}
}
