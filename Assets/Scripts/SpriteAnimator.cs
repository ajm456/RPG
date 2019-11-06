using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
	[SerializeField] private Sprite[] idleFrameArray, deathFrameArray;
	[SerializeField] private float idleCycleDuration, deathCycleDuration;

	private Sprite[] currFrameArray;
	private int currentFrame;
	private float idleTimer, deathTimer;
	private float idleFrameDuration, deathFrameDuration;
	private SpriteRenderer spriteRenderer;
	private CombatantController combatantController;


	void Start() {
		currFrameArray = idleFrameArray;
		currentFrame = 0;
		idleTimer = 0;
		deathTimer = 0;
		idleFrameDuration = idleCycleDuration / idleFrameArray.Length;
		deathFrameDuration = deathCycleDuration / deathFrameArray.Length;
		spriteRenderer = GetComponent<SpriteRenderer>();
		combatantController = GetComponent<CombatantController>();

	}

	void Update() {
		if(combatantController.CurrHP > 0) {
			// If alive, play idle animation
			idleTimer += Time.deltaTime;

			if(idleTimer >= idleFrameDuration) {
				idleTimer -= idleFrameDuration;
				currentFrame = (currentFrame + 1) % idleFrameArray.Length;
				spriteRenderer.sprite = currFrameArray[currentFrame];
			}
		} else {
			// If dead, play death animation once
			if(currFrameArray == idleFrameArray)
				currFrameArray = deathFrameArray;

			deathTimer += Time.deltaTime;

			if(deathTimer >= deathFrameDuration) {
				deathTimer -= deathFrameDuration;
				currentFrame++;
				if(currentFrame >= deathFrameArray.Length) {
					Destroy(gameObject);
					return;
				}

				spriteRenderer.sprite = currFrameArray[currentFrame];
			}
		}
	}
}
