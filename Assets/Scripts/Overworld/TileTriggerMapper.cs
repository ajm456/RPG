﻿using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maps cell coordinates in a scene to functions.
/// </summary>
public class TileTriggerMapper : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private LevelChanger levelChanger;
#pragma warning restore 0649

	/// <summary>
	/// Executes the behaviour for the given cell's trigger.
	/// </summary>
	/// <param name="cell">The Vector2Int coordinate of the cell being executed.</param>
	public void DoTriggerForCell(Vector2Int cell)
	{
		DoTriggerForCell(cell.x, cell.y);
	}

	/// <summary>
	/// Executes the behaviour for the given cell's trigger.
	/// </summary>
	/// <param name="x">The int x-coordinate of the cell being executed.</param>
	/// <param name="y">The int y-coordinate of the cell being executed.</param>
	public void DoTriggerForCell(int x, int y)
	{
		Scene activeScene = SceneManager.GetActiveScene();
		switch (activeScene.name)
		{
			case "PT_OverworldNew":
				HandleOverworldNew(x, y);
				break;
			case "PT_OverworldNewWarpTarget":
				HandleOverworldNewWarpTarget(x, y);
				break;
			default:
				Debug.Log("Current scene does not have an entry in TileTriggerMapper!");
				Debug.Break();
				break;
		}
	}

	private void HandleOverworldNew(int x, int y)
	{
		if ((x == 0 && y == 6)
			||  (x == 1 && y == 6)
			)
		{
			DoWarp("PT_OverworldNewWarpTarget");
		}
	}

	private void HandleOverworldNewWarpTarget(int x, int y)
	{
		if ((x == 1 && y == 1)
			||  (x == 2 && y == 1)
			)
		{
			DoWarp("PT_OverworldNew");
		}
	}

	/// <summary>
	/// Warps the player to a new scene.
	/// </summary>
	private void DoWarp(string scene)
	{
		levelChanger.LoadNextLevel(scene);
	}
}
