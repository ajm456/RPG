using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
	/// <summary>
	/// Represents the current movement state the player sprite is in.
	/// </summary>
	private enum MovementState
	{
		IDLE,
		MOVING_LEFT,
		MOVING_RIGHT,
		MOVING_UP,
		MOVING_DOWN
	}


	/// <summary>
	/// The Tilemap Grid object for this scene.
	/// </summary>
	[SerializeField]
	private Grid grid;

	/// <summary>
	/// How fast the player should move between cells.
	/// </summary>
	[SerializeField]
	private float speed;

	/// <summary>
	/// The x,y of the grid cell the player should spawn in on this scene.
	/// </summary>
	[SerializeField]
	private Vector2Int spawnCell;

	/// <summary>
	/// The Tilemap collider containing cells the player cannot walk into.
	/// </summary>
	[SerializeField]
	private TilemapCollider2D tilemapCollider;

	/// <summary>
	/// The cell trigger map for this scene.
	/// </summary>
	[SerializeField]
	private TileTriggerMapper triggerMapper;

	[SerializeField]
	private LevelChanger levelChanger;


	/// <summary>
	/// The offset from grid line intersection points the player occupies.
	/// 
	/// Since (0,0) on a Grid is not the middle of a cell but the intersection
	/// of two axes.
	/// </summary>
	private Vector3 cellOffset;

	/// <summary>
	/// The current cell the player is occupying.
	/// </summary>
	private Vector3Int cellPos;

	/// <summary>
	/// The world position that the player is currently moving to.
	/// </summary>
	private Vector3 movementTarget;

	/// <summary>
	/// The world position that the player is currently moving from.
	/// </summary>
	private Vector3 movementOrigin;

	/// <summary>
	/// The current movement state of the player sprite.
	/// </summary>
	private MovementState state;

	private void Start()
	{
		cellPos = new Vector3Int(spawnCell.x, spawnCell.y, 0);
		cellOffset = grid.cellSize / 2f;
		transform.position = grid.CellToWorld(new Vector3Int(spawnCell.x, spawnCell.y, 0)) + cellOffset;
		state = MovementState.IDLE;
	}

	void Update()
	{
		if (state == MovementState.IDLE && !levelChanger.InputLocked)
		{
			float hInput = Input.GetAxisRaw("Horizontal");
			float vInput = Input.GetAxisRaw("Vertical");

			// Left
			if (hInput < 0f)
			{
				// Check the left-adjacent cell isn't blocked by collider
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x - 1, cellPos.y, cellPos.z)) + cellOffset;
				if (!tilemapCollider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_LEFT;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
			// Right
			else if (hInput > 0f)
			{
				// Check the right-adjacent cell isn't blocked by collider
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x + 1, cellPos.y, cellPos.z)) + cellOffset;
				if (!tilemapCollider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_RIGHT;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
			// Up
			else if (vInput > 0f)
			{
				// Check the up-adjacent cell isn't blocked by collider
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x, cellPos.y + 1, cellPos.z)) + cellOffset;
				if (!tilemapCollider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_UP;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
			// Down
			else if (vInput < 0f)
			{
				// Check the down-adjacent cell isn't blocked by collider
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x, cellPos.y - 1, cellPos.z)) + cellOffset;
				if (!tilemapCollider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_DOWN;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
		}
		else if (state != MovementState.IDLE)
		{
			// We're currently in a MOVING_... state, so move the sprite
			Vector3 vel;

			switch (state)
			{
				case MovementState.MOVING_LEFT:
					vel = new Vector3(-speed, 0f);
					break;
				case MovementState.MOVING_RIGHT:
					vel = new Vector3(speed, 0f);
					break;
				case MovementState.MOVING_UP:
					vel = new Vector3(0f, speed);
					break;
				case MovementState.MOVING_DOWN:
					vel = new Vector3(0f, -speed);
					break;
				default:
					vel = new Vector3();
					Debug.Break();
					break;
			}

			transform.position += vel * Time.deltaTime;

			// Check if we've arrived at/past our destination
			if (Vector3.Distance(movementOrigin, transform.position) > Vector3.Distance(movementTarget, movementOrigin))
			{
				transform.position = movementTarget;
				cellPos = grid.WorldToCell(transform.position);
				state = MovementState.IDLE;
				// If this cell is a trigger, execute it
				triggerMapper.DoTriggerForCell(cellPos.x, cellPos.y);
			}
		}
	}
}
