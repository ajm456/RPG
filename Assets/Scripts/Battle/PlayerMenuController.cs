﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Holds the state required for a menu.
/// </summary>
internal class Menu
{
	internal static readonly float ITEM_MARGIN = 10f;
	internal static readonly float MENU_MARGIN = 8f;
	internal static readonly float MENU_H_PADDING = 15f;
	internal static readonly float COMB_MENU_H_GAP = 70f;
	internal static readonly float MENU_UP_SHIFT = 50f;

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
		item.transform.localPosition += new Vector3(MENU_H_PADDING, items.Count * -(item.rectTransform.sizeDelta.y + ITEM_MARGIN));
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
			color.a = textMeshPro.alpha;
			textMeshPro.color = color;
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
			Color c = new Color(1f, 1f, 1f, textMeshPro.alpha);
			textMeshPro.color = c;
		}
	}


	/// <summary>
	/// Enables/disables this menu item. Disabled menu items cannot be
	/// selected.
	/// </summary>
	/// <param name="enabled">Whether to enable or disable this menu item.</param>
	internal void SetEnabled(bool enabled)
	{
		textMeshPro.alpha = enabled ? 1f : 0.3f;

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
		if (enabled)
		{
			callback();
		}
	}
}


/// <summary>
/// Class responsible for managing player menu during battles. BattleController
/// will wait for this class to execute player turns.
/// </summary>
public class PlayerMenuController : MonoBehaviour
{
#pragma warning disable 0649
	// Prefabs
	[SerializeField] private GameObject menuPrefab, menuItemPrefab;

	// Other scene components
	[SerializeField] private BattleController battleController;
	[SerializeField] private Canvas gameUI;
#pragma warning restore 0649


	// Root player menu that moves between heroes
	private readonly Menu[] menus = new Menu[2];


	/// <summary>
	/// Current cursor position in form [menu,item].
	/// </summary>
	private Vector2Int cursorPos;

	/// <summary>
	/// A cache of the last turn number value we read from the battle
	/// controller. Used to discern when the menus need updating.
	/// </summary>
	private int currTurnNum;

	/// <summary>
	/// Ability list for each hero (calm and strife).
	/// 
	/// Outermost - list of heroes.
	/// Middle - list of ability paradigms (calm or strife)
	/// Innermost - list of abilities
	/// </summary>
	private List<List<List<AbilityData>>> heroAbilityLists;

	/// <summary>
	/// Flag raised when the player is selecting a target for their action.
	/// </summary>
	private bool selectingTarget;

	/// <summary>
	/// The ID of the combatant currently being targeted.
	/// </summary>
	private int targetID;

	/// <summary>
	/// AbilityData for the ability selected when a player starts choosing a
	/// target. Null selectedAbility indicates that a universal attack is being
	/// executed.
	/// </summary>
	private AbilityData selectedAbility;


	/// <summary>
	/// MonoBehaviour start.
	/// </summary>
	void Start()
	{
		InitialiseHeroAbilityLists();
		InitialiseRootMenu();
		InitialiseExtraMenu();
		currTurnNum = -1;
		cursorPos = new Vector2Int(0, 0);
		targetID = -1;
	}


	/// <summary>
	/// MonoBehaviour update.
	/// </summary>
	void Update()
	{
		if (battleController.State == BattleController.BattleState.PLAYERWON
			|| battleController.State == BattleController.BattleState.ENEMYWON)
		{
			menus[0].SetActive(false);
			menus[1].SetActive(false);
			gameObject.SetActive(false);
		}

		// If required, move the player menu to be next to the current hero
		if (battleController.State == BattleController.BattleState.PLAYERCHOICE)
		{
			// Check to see if we need to move the menu
			if (currTurnNum != battleController.TurnNum
				&& !battleController.IsCombatantAnimating(battleController.CurrCombatantID)
				&& !battleController.IsCombatantInAnimQueue(battleController.CurrCombatantID))
			{
				currTurnNum = battleController.TurnNum;

				// Move the menu
				menus[0].Transform.position = battleController.GetCombatantTransform(battleController.CurrCombatantID).position;
				// Push it slightly to the right of the current combatant and move it up
				menus[0].RectTransform.localPosition += new Vector3(Menu.COMB_MENU_H_GAP, Menu.MENU_UP_SHIFT);

				// Destroy the old gameobjects and clear list data in the extra menu
				foreach (GameObject item in menus[1].GetMenuItems())
				{
					Destroy(item);
				}
				menus[1].ClearMenuItems();

				// Reposition the extra menu
				menus[1].Transform.position = menus[0].Transform.position;
				menus[1].Transform.localPosition = menus[0].Transform.localPosition + new Vector3(menus[0].RectTransform.sizeDelta.x + Menu.MENU_MARGIN, 0f);

				// Hide the extra menu
				menus[1].SetActive(false);
				// Show the root menu
				menus[0].SetActive(true);

				// Disable menu items if we need to
				menus[0].SetItemEnabled(1, battleController.CurrCombatantStrifeAbilities.Count > 0);
				menus[0].SetItemEnabled(2, battleController.CurrCombatantCalmAbilities.Count > 0);

				// Since the hero's changed, the color of the highlighted menu item will change too
				if (cursorPos.x == 0)
				{
					UnhighlightMenuItem(cursorPos);
				}
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

		// Populate it with root level menu items
		menus[0].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[0].Transform), "ATTACK", new Action(OnSelectAttack)));
		menus[0].AddMenuItem(new MenuItem(Instantiate(menuItemPrefab, menus[0].Transform), "STRIFE", new Action(OnSelectStrife)));
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
		menus[1].Transform.localPosition = menus[0].Transform.localPosition + new Vector3(menus[0].RectTransform.sizeDelta.x + Menu.MENU_MARGIN, 0f);

		// Double the width
		menus[1].RectTransform.sizeDelta += new Vector2(menus[1].RectTransform.sizeDelta.x, 0f);

		// For now, hide the menu
		menus[1].SetActive(false);
	}


	/// <summary>
	/// Cache the ability lists of each hero.
	/// </summary>
	private void InitialiseHeroAbilityLists()
	{
		heroAbilityLists = battleController.GetHeroAbilities();
	}


	/// <summary>
	/// Manages user navigation of the root and extra menu.
	/// </summary>
	private void NavigateMenus()
	{
		if (selectingTarget)
		{
			if (selectedAbility == null
				|| selectedAbility.targetingMode == AbilityData.TargetingMode.SINGLE)
			{
				NavigateSingleTargetSelection();
			}
			else if (selectedAbility.targetingMode == AbilityData.TargetingMode.PARTY)
			{
				NavigatePartyTargetSelection();
			}
			else if (selectedAbility.targetingMode == AbilityData.TargetingMode.OPPOSITION)
			{
				NavigateOppositionTargetSelection();
			}
		}
		else
		{
			NavigateRootMenu();
		}
	}


	/// <summary>
	/// Handles navigation of the root menu.
	/// </summary>
	private void NavigateRootMenu()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			cursorPos.y = (cursorPos.y + 1) % menus[cursorPos.x].NumItems;
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			cursorPos.y = (menus[cursorPos.x].NumItems + cursorPos.y - 1) % menus[cursorPos.x].NumItems;
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			// We don't do anything if the cursor is on the root menu
			if (cursorPos.x == 0)
				return;

			// Providing the cursor is on the extra menu, navigate left and close
			cursorPos.x -= 1;
			menus[1].SetActive(false);
		}
		else if (Input.GetKeyDown(KeyCode.Z))
		{
			menus[cursorPos.x].SelectItem(cursorPos.y);
		}
	}


	/// <summary>
	/// Handles cursor navigation when targeting with a single-target ability.
	/// </summary>
	private void NavigateSingleTargetSelection()
	{
		bool targetingHeroes = targetID < battleController.GetNumHeroes();

		if (!battleController.IsCombatantAlive(targetID) && !targetingHeroes)
		{
			// If the combatant we're targeting has died and is an enemy,
			// scroll down
			FindNextVerticalCombatant(false);
			return;
		}

		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			battleController.UnhighlightCombatant(targetID);
			if (targetingHeroes)
			{
				targetID = (targetID + 1) % battleController.GetNumHeroes();
			}
			else
			{
				// We cannot target dead enemies, so scroll past if we're
				// trying to target one
				do
				{
					targetID = battleController.GetNumHeroes() + ((targetID - battleController.GetNumHeroes() + 1) % battleController.GetNumEnemies());
				} while (!battleController.IsCombatantAlive(targetID));
			}
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			battleController.UnhighlightCombatant(targetID);
			if (targetingHeroes)
			{
				// We are targeting heroes
				targetID = (battleController.GetNumHeroes() + targetID - 1) % battleController.GetNumHeroes();
			}
			else
			{
				// We are targeting enemies
				// We cannot target dead enemies, so scroll past if we're
				// trying to target one
				do
				{
					targetID = battleController.GetNumHeroes() + ((battleController.GetNumEnemies() + targetID - battleController.GetNumHeroes() - 1) % battleController.GetNumEnemies());
				} while (!battleController.IsCombatantAlive(targetID));
			}
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			battleController.UnhighlightCombatant(targetID);
			if (targetingHeroes)
			{
				// We are targeting heroes
				targetID = Mathf.Min(targetID + battleController.GetNumHeroes(), battleController.GetNumCombatants() - 1);
			}
			else
			{
				// We are targeting enemies
				targetID = Mathf.Min(targetID - battleController.GetNumHeroes(), battleController.GetNumHeroes() - 1);
			}
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			battleController.UnhighlightCombatant(targetID);
			if (targetingHeroes)
			{
				// We are targeting heroes
				targetID = Mathf.Min(targetID + battleController.GetNumHeroes(), battleController.GetNumCombatants() - 1);
			}
			else
			{
				// We are targeting enemies
				targetID = Mathf.Min(targetID - battleController.GetNumHeroes(), battleController.GetNumHeroes() - 1);
			}
		}

		// Set the targeted enemy's sprite color to the hero's color
		battleController.HighlightCombatant(targetID);

		if (Input.GetKeyDown(KeyCode.Z))
		{
			// Hide the menus whilst the animation is carried out
			menus[0].SetActive(false);
			menus[1].SetActive(false);

			if (selectedAbility == null)
			{
				// Null selectedAbility implies we are doing an attack command
				battleController.ExecuteTurnWithAttack(battleController.CurrCombatantID, targetID);
			}
			else
			{
				battleController.ExecuteTurnWithAbility(selectedAbility, battleController.CurrCombatantID, new List<int>() { targetID });
			}
				
			selectingTarget = false;
			battleController.UnhighlightCombatant(targetID);
			battleController.WaitingOnPlayerTurn = false;
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			battleController.UnhighlightCombatant(targetID);
			selectingTarget = false;
		}
	}


	/// <summary>
	/// Handles cursor navigation when targeting with a party-wide ability.
	/// </summary>
	private void NavigatePartyTargetSelection()
	{
		if (Input.GetKeyDown(KeyCode.Z))
		{
			// Hide the menus whilst the animation is carried out
			menus[0].SetActive(false);
			menus[1].SetActive(false);

			if (selectedAbility == null)
			{
				// Null selectedAbility implies we are doing an attack command
				battleController.ExecuteTurnWithAttack(battleController.CurrCombatantID, targetID);
			}
			else
			{
				List<int> heroIDs = battleController.GetHeroIDs();
				battleController.ExecuteTurnWithAbility(selectedAbility, battleController.CurrCombatantID, heroIDs);
			}
				
			selectingTarget = false;
			for (int i = 0; i < battleController.GetNumHeroes(); ++i)
			{
				battleController.UnhighlightCombatant(i);
			}
			battleController.WaitingOnPlayerTurn = false;
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			for (int i = 0; i < battleController.GetNumHeroes(); ++i)
			{
				battleController.UnhighlightCombatant(i);
			}
			selectingTarget = false;
		}
	}


	/// <summary>
	/// Handles cursor navigation when targeting with an opposition-wide
	/// ability.
	/// </summary>
	private void NavigateOppositionTargetSelection()
	{
		if (Input.GetKeyDown(KeyCode.Z))
		{
			// Hide the menus whilst the animation is carried out
			menus[0].SetActive(false);
			menus[1].SetActive(false);

			if (selectedAbility == null)
			{
				// Attack is never party-wide
				Debug.LogError("Trying to use attack action in opposition target selection!");
				Debug.Break();
			}
			else
			{
				List<int> targetIDs = battleController.GetEnemyIDs();
				battleController.ExecuteTurnWithAbility(selectedAbility, battleController.CurrCombatantID, targetIDs);
			}
				
			selectingTarget = false;
			for (int i = battleController.GetNumHeroes(); i < battleController.GetNumCombatants(); ++i)
			{
				battleController.UnhighlightCombatant(i);
			}
			battleController.WaitingOnPlayerTurn = false;
		}
		else if (Input.GetKeyDown(KeyCode.X))
		{
			for (int i = battleController.GetNumHeroes(); i < battleController.GetNumCombatants(); ++i)
			{
				battleController.UnhighlightCombatant(i);
			}
			selectingTarget = false;
		}
	}


	/// <summary>
	/// Changes the target to the next appropriate combatant. Used when a
	/// currently targeted combatant dies.
	/// </summary>
	/// <param name="targetingHeroes">
	/// Whether or not the current target is a hero or enemy.
	/// </param>
	private void FindNextVerticalCombatant(bool targetingHeroes)
	{
		if (targetingHeroes)
		{
			targetID = (targetID + 1) % battleController.GetNumHeroes();
		}
		else
		{
			do
			{
				battleController.UnhighlightCombatant(targetID);
				targetID = battleController.GetNumHeroes() + ((targetID - battleController.GetNumHeroes() + 1) % battleController.GetNumEnemies());
			} while (!battleController.IsCombatantAlive(targetID));
		}
	}


	/// <summary>
	/// Highlights the menu item at a given position.
	/// </summary>
	/// <param name="pos">The xy position of the item to highlight.</param>
	private void HighlightMenuItem(Vector2Int pos)
	{
		menus[pos.x].HighlightMenuItem(pos.y, battleController.CurrCombatantColor);
	}


	/// <summary>
	/// Highlights the menu item at a given position.
	/// </summary>
	/// <param name="x">The x position of the item to highlight (aka which menu it's in).</param>
	/// <param name="y">The y position of the item to highlight.</param>
	private void HighlightMenuItem(int x, int y)
	{
		menus[x].HighlightMenuItem(y, battleController.CurrCombatantColor);
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
		// Set selectedAbility to null to indicate we are targeting with an
		// attack, not an ability
		selectedAbility = null;

		// Initial target for attacks is the topmost enemy
		targetID = battleController.GetNumHeroes();

		// Transition to target selection
		selectingTarget = true;
		Debug.Log("Selecting target for attack");
	}


	/// <summary>
	/// Callback for selecting the strife abilities menu item.
	/// </summary>
	private void OnSelectStrife()
	{
		// Destroy the old gameobjects before clearing list data
		foreach (GameObject item in menus[1].GetMenuItems())
		{
			Destroy(item);
		}
		menus[1].ClearMenuItems();

		// Create menu items for each of the hero's strife abilities
		for (var i = 0; i < heroAbilityLists[battleController.CurrCombatantID][1].Count; ++i)
		{
			AbilityData ability = heroAbilityLists[battleController.CurrCombatantID][1][i];
			MenuItem item = new MenuItem(Instantiate(menuItemPrefab, menus[1].Transform), ability.name.ToUpper(), new Action(() =>
			{
				// Store the selected ability to be cast when the player
				// selects their target
				selectedAbility = new AbilityData(ability);

				// Initial target for strife abilities is the topmost enemy
				targetID = battleController.GetNumHeroes();

				// Transition to target selection
				selectingTarget = true;
				// Highlight appropriate combatants
				if (ability.targetingMode == AbilityData.TargetingMode.PARTY)
				{
					for (int i = 0; i < battleController.GetNumHeroes(); ++i)
					{
						battleController.HighlightCombatant(i);
					}
				}
				else if (ability.targetingMode == AbilityData.TargetingMode.OPPOSITION)
				{
					for (int i = battleController.GetNumHeroes(); i < battleController.GetNumCombatants(); ++i)
					{
						battleController.HighlightCombatant(i);
					}
				}
				
				Debug.Log("Selecting target for ability " + ability.name + "...");
			}));

			// If the player doesn't have enough resources for an ability,
			// disable it
			if (battleController.GetCombatantStrife(battleController.CurrCombatantID) < ability.strifeReq)
			{
				item.SetEnabled(false);
			}

			menus[1].AddMenuItem(item);
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

		// Populate the list of available Calm abilities
		for (var i = 0; i < heroAbilityLists[battleController.CurrCombatantID][0].Count; ++i)
		{
			AbilityData ability = heroAbilityLists[battleController.CurrCombatantID][0][i];
			MenuItem item = new MenuItem(Instantiate(menuItemPrefab, menus[1].Transform), ability.name.ToUpper(), new Action(() =>
			{
				// Store the selected ability to be cast when the player
				// selects their target
				selectedAbility = new AbilityData(ability);

				// Initial target for strife abilities is the topmost enemy
				targetID = 0;

				// Transition to target selection
				selectingTarget = true;
				// Highlight appropriate combatants
				if (ability.targetingMode == AbilityData.TargetingMode.PARTY)
				{
					for (int i = 0; i < battleController.GetNumHeroes(); ++i)
					{
						battleController.HighlightCombatant(i);
					}
				}
				else if (ability.targetingMode == AbilityData.TargetingMode.OPPOSITION)
				{
					for (int i = battleController.GetNumHeroes(); i < battleController.GetNumCombatants(); ++i)
					{
						battleController.HighlightCombatant(i);
					}
				}
				Debug.Log("Selecting target for ability " + ability.name + "...");
			}));

			// If the player doesn't have enough resources for an ability,
			// disable it
			if (battleController.GetCombatantCalm(battleController.CurrCombatantID) < ability.calmReq)
			{
				item.SetEnabled(false);
			}

			menus[1].AddMenuItem(item);
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
		battleController.WaitingOnPlayerTurn = false;
	}
}
