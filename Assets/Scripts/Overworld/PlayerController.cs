using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
	private enum MovementState
	{
		IDLE,
		MOVING_LEFT,
		MOVING_RIGHT,
		MOVING_UP,
		MOVING_DOWN
	}


	[SerializeField]
	private Grid grid;

	[SerializeField]
	private float speed;

	[SerializeField]
	private Vector2Int spawnCell;

	[SerializeField]
	private TilemapCollider2D collider;


	private Vector3 cellOffset;
	private Vector3Int cellPos;
	private Vector3 movementTarget;
	private Vector3 movementOrigin;
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
		if (state == MovementState.IDLE)
		{
			float hInput = Input.GetAxisRaw("Horizontal");
			float vInput = Input.GetAxisRaw("Vertical");

			if (hInput < 0f)
			{
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x - 1, cellPos.y, cellPos.z)) + cellOffset;
				if (!collider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_LEFT;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
			else if (hInput > 0f)
			{
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x + 1, cellPos.y, cellPos.z)) + cellOffset;
				if (!collider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_RIGHT;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
			else if (vInput > 0f)
			{
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x, cellPos.y + 1, cellPos.z)) + cellOffset;
				if (!collider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_UP;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
			else if (vInput < 0f)
			{
				Vector3 potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x, cellPos.y - 1, cellPos.z)) + cellOffset;
				if (!collider.OverlapPoint(potentialMovementTarget))
				{
					state = MovementState.MOVING_DOWN;
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
				}
			}
		}
		else
		{
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
					break;
			}

			transform.position += vel * Time.deltaTime;

			if (Vector3.Distance(movementOrigin, transform.position) > Vector3.Distance(movementTarget, movementOrigin))
			{
				transform.position = movementTarget;
				cellPos = grid.WorldToCell(transform.position);
				state = MovementState.IDLE;
			}
		}
	}
}
