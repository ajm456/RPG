using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
	/// <summary>
	/// Bitmask to simplify storing inputs.
	/// </summary>
	[Flags]
	private enum InputDir
	{
		NONE = 0,
		LEFT = 1,
		RIGHT = 2,
		UP = 4,
		DOWN = 8
	}

	/// <summary>
	/// Represents the current movement state the player sprite is in.
	/// </summary>
	private enum PlayerState
	{
		IDLE,
		MOVING
	}

	/// <summary>
	/// Represents the direction the player is currently facing.
	/// </summary>
	private enum PlayerDirection
	{
		UP,
		DOWN,
		LEFT,
		RIGHT
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
	/// How long should the player have to hold down a movement button before
	/// they start moving (as opposed to simply facing the direction).
	/// </summary>
	[SerializeField]
	private int moveFrameDelay;

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

	/// <summary>
	/// Game object responsible for transitioning between different scenes.
	/// </summary>
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
	/// The input object from the last frame. Used to see if the player is
	/// holding down a button.
	/// </summary>
	private InputDir lastInput;

	/// <summary>
	/// Keeps track of for how many frames the player has faced the current
	/// direction.
	/// </summary>
	private int sameDirectionFrameCount;

	/// <summary>
	/// Flag for when the player sprite arrives at its movement destination.
	/// </summary>
	private bool playerArrived;

	/// <summary>
	/// The current movement state of the player sprite.
	/// </summary>
	private PlayerState state;

	/// <summary>
	/// The direction the player sprite is currently facing.
	/// </summary>
	private PlayerDirection faceDirection;

	/// <summary>
	/// The component managing the player sprite's animation.
	/// </summary>
	private Animator animController;

	private void Start()
	{
		cellPos = new Vector3Int(spawnCell.x, spawnCell.y, 0);
		cellOffset = grid.cellSize / 2f;
		transform.position = grid.CellToWorld(new Vector3Int(spawnCell.x, spawnCell.y, 0)) + cellOffset;
		lastInput = InputDir.NONE;
		sameDirectionFrameCount = 0;
		playerArrived = true;
		state = PlayerState.IDLE;
		faceDirection = PlayerDirection.DOWN;
		animController = GetComponent<Animator>();
	}

	void Update()
	{
		InputDir input = InputDir.NONE;
		if (!levelChanger.InputLocked)
		{
			input = GetInputDir();
		}

		if (state == PlayerState.IDLE || playerArrived)
		{
			SetInputTarget(input);
		}
		
		if (state == PlayerState.MOVING)
		{
			MovePlayer();
		}

		ManageAnim();
	}

	/// <summary>
	/// Gathers input information from the player and stores it in an InputDir
	/// object, just to simplify input matters.
	/// </summary>
	/// <returns>An InputDir enum object holding the input information for
	/// this frame.</returns>
	private InputDir GetInputDir()
	{
		float hInput = Input.GetAxisRaw("Horizontal");
		float vInput = Input.GetAxisRaw("Vertical");

		InputDir input = InputDir.NONE;
		if (hInput < 0f)
		{
			input |= InputDir.LEFT;
		}
		else if (hInput > 0f)
		{
			input |= InputDir.RIGHT;
		}

		if (vInput > 0f)
		{
			input |= InputDir.UP;
		}
		else if (vInput < 0f)
		{
			input |= InputDir.DOWN;
		}

		return input;
	}

	/// <summary>
	/// Translates input into an action target such as a movement destination.
	/// </summary>
	/// <param name="input">The InpurDir object from the current frame.</param>
	private void SetInputTarget(InputDir input)
	{
		// If we've arrived at a movement target and no input is being given,
		// force our state to idle
		if (playerArrived && input == InputDir.NONE)
		{
			state = PlayerState.IDLE;
			return;
		}

		if ((state == PlayerState.IDLE || playerArrived)
			&& !levelChanger.InputLocked)
		{
			// Handle direction change
			if (input.HasFlag(InputDir.LEFT))
			{
				if (faceDirection == PlayerDirection.LEFT)
				{
					sameDirectionFrameCount++;
				}
				else
				{
					sameDirectionFrameCount = 0;
					faceDirection = PlayerDirection.LEFT;
				}
			}
			else if (input.HasFlag(InputDir.RIGHT))
			{
				if (faceDirection == PlayerDirection.RIGHT)
				{
					sameDirectionFrameCount++;
				}
				else
				{
					sameDirectionFrameCount = 0;
					faceDirection = PlayerDirection.RIGHT;
				}
			}
			else if (input.HasFlag(InputDir.UP))
			{
				if (faceDirection == PlayerDirection.UP)
				{
					sameDirectionFrameCount++;
				}
				else
				{
					sameDirectionFrameCount = 0;
					faceDirection = PlayerDirection.UP;
				}
			}
			else if (input.HasFlag(InputDir.DOWN))
			{
				if (faceDirection == PlayerDirection.DOWN)
				{
					sameDirectionFrameCount++;
				}
				else
				{
					sameDirectionFrameCount = 0;
					faceDirection = PlayerDirection.DOWN;
				}
			}

			// Prevent any kind of crazy overflow nonsense
			sameDirectionFrameCount = Mathf.Clamp(sameDirectionFrameCount, 0, 1024);

			if (sameDirectionFrameCount >= moveFrameDelay || (playerArrived && state == PlayerState.MOVING))
			{
				Vector3 potentialMovementTarget = transform.position;

				if (input.HasFlag(InputDir.LEFT))
				{
					potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x - 1, cellPos.y, cellPos.z)) + cellOffset;
				}
				else if (input.HasFlag(InputDir.RIGHT))
				{
					potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x + 1, cellPos.y, cellPos.z)) + cellOffset;
				}
				else if (input.HasFlag(InputDir.UP))
				{
					potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x, cellPos.y + 1, cellPos.z)) + cellOffset;
				}
				else if (input.HasFlag(InputDir.DOWN))
				{
					potentialMovementTarget = grid.CellToWorld(new Vector3Int(cellPos.x, cellPos.y - 1, cellPos.z)) + cellOffset;
				}

				// Check we're not trying to move into a collidable cell
				if (!tilemapCollider.OverlapPoint(potentialMovementTarget)
					&& potentialMovementTarget != transform.position)
				{
					movementTarget = potentialMovementTarget;
					movementOrigin = transform.position;
					state = PlayerState.MOVING;
					playerArrived = false;
				}
			}
		}
	}

	/// <summary>
	/// Moves the player sprite to its target destination.
	/// </summary>
	private void MovePlayer()
	{
		// Move towards the target
		Vector3 vel = Vector3.Normalize(movementTarget - transform.position) * speed;

		transform.position += vel * Time.deltaTime;
		
		// Check if we've arrived at/past our destination
		if (Vector3.Distance(movementOrigin, transform.position) > Vector3.Distance(movementTarget, movementOrigin))
		{
			transform.position = movementTarget;
			cellPos = grid.WorldToCell(transform.position);
			playerArrived = true;

			// Handle any triggers this cell might have
			triggerMapper.DoTriggerForCell(cellPos.x, cellPos.y);
		}
	}

	/// <summary>
	/// Handles managing the player sprite's animation state.
	/// </summary>
	private void ManageAnim()
	{
		if (state == PlayerState.MOVING)
		{
			switch (faceDirection)
			{
				case PlayerDirection.UP:
					ExclusiveSetMoveAnimBool("up");
					break;
				case PlayerDirection.DOWN:
					ExclusiveSetMoveAnimBool("down");
					break;
				case PlayerDirection.LEFT:
					ExclusiveSetMoveAnimBool("left");
					break;
				case PlayerDirection.RIGHT:
					ExclusiveSetMoveAnimBool("right");
					break;
			}
		}
		else
		{
			ClearMoveAnimBools();
			switch (faceDirection)
			{
				case PlayerDirection.UP:
					ExclusiveSetFaceAnimBool("up");
					break;
				case PlayerDirection.DOWN:
					ExclusiveSetFaceAnimBool("down");
					break;
				case PlayerDirection.LEFT:
					ExclusiveSetFaceAnimBool("left");
					break;
				case PlayerDirection.RIGHT:
					ExclusiveSetFaceAnimBool("right");
					break;
			}
		}
	}

	/// <summary>
	/// Sets a walk animation flag for the given direction, clearing flags for
	/// all other directions.
	/// </summary>
	/// <param name="dir">The direction of the flag to raise.</param>
	private void ExclusiveSetMoveAnimBool(string dir)
	{
		switch (dir.ToLower())
		{
			case "up":
				animController.SetBool("walking_up", true);
				animController.SetBool("walking_down", false);
				animController.SetBool("walking_left", false);
				animController.SetBool("walking_right", false);
				break;
			case "down":
				animController.SetBool("walking_up", false);
				animController.SetBool("walking_down", true);
				animController.SetBool("walking_left", false);
				animController.SetBool("walking_right", false);
				break;
			case "left":
				animController.SetBool("walking_up", false);
				animController.SetBool("walking_down", false);
				animController.SetBool("walking_left", true);
				animController.SetBool("walking_right", false);
				break;
			case "right":
				animController.SetBool("walking_up", false);
				animController.SetBool("walking_down", false);
				animController.SetBool("walking_left", false);
				animController.SetBool("walking_right", true);
				break;
			default:
				Debug.Log("Unrecognised direction string when setting player walking animation bool");
				Debug.Break();
				break;
		}
	}

	/// <summary>
	/// Sets a facing animation flag for the given direction, clearing flags
	/// for all other directions.
	/// </summary>
	/// <param name="dir">The direction of the flag to raise.</param>
	private void ExclusiveSetFaceAnimBool(string dir)
	{
		switch (dir.ToLower())
		{
			case "up":
				animController.SetBool("facing_up", true);
				animController.SetBool("facing_down", false);
				animController.SetBool("facing_left", false);
				animController.SetBool("facing_right", false);
				break;
			case "down":
				animController.SetBool("facing_up", false);
				animController.SetBool("facing_down", true);
				animController.SetBool("facing_left", false);
				animController.SetBool("facing_right", false);
				break;
			case "left":
				animController.SetBool("facing_up", false);
				animController.SetBool("facing_down", false);
				animController.SetBool("facing_left", true);
				animController.SetBool("facing_right", false);
				break;
			case "right":
				animController.SetBool("facing_up", false);
				animController.SetBool("facing_down", false);
				animController.SetBool("facing_left", false);
				animController.SetBool("facing_right", true);
				break;
			default:
				Debug.Log("Unrecognised direction string when setting player facing animation bool");
				Debug.Break();
				break;
		}
	}

	private void ClearMoveAnimBools()
	{
		animController.SetBool("walking_up", false);
		animController.SetBool("walking_down", false);
		animController.SetBool("walking_left", false);
		animController.SetBool("walking_right", false);
	}
}
