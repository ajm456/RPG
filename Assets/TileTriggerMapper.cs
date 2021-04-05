using UnityEngine;

/// <summary>
/// Maps cell coordinates in a scene to functions.
/// </summary>
public class TileTriggerMapper : MonoBehaviour
{
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
		if (	(x == 0 && y == 6)
			||	(x == 1 && y == 6)
			)
		{
			DoWarp();
		}
	}

	/// <summary>
	/// Warps the player to a new scene.
	/// </summary>
	private void DoWarp()
	{
		Debug.Break();
	}
}
