using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
	private const float CARAT_FLASH_RATE = 0.2f;
	private const float CARAT_X_DIFF = -91.01f;

	[SerializeField] private GameObject botLeftChoice, topLeftChoice, botRightChoice, topRightChoice, caratChoice;
	private float caratFlashTimer;
	private Vector2[,] caratPositions;
	private int caratXInd, caratYInd;





	void Start() {
		// Load the positions the carat will occupy in the menu
		LoadCaratPositions();
	}

	void Update() {
		float dt = Time.deltaTime;

		// Flash the carat indicator
		AnimateCarat(dt);

		// Respond to any player input
		HandleInput();
	}




	// Get the ID of the currently highlighted otion
	public int GetChoice() {
		// For now, ID options by the carat position (this breaks with submenus)
		return caratYInd*2 + caratXInd;
	}

	private void LoadCaratPositions() {
		caratPositions = new Vector2[2,2];
		Vector2 caratDiffVec = new Vector2(CARAT_X_DIFF, 0f);
		caratPositions[0,0] = new Vector2(botLeftChoice.transform.localPosition.x, botLeftChoice.transform.localPosition.y) + caratDiffVec;
		caratPositions[0,1] = new Vector2(topLeftChoice.transform.localPosition.x, topLeftChoice.transform.localPosition.y) + caratDiffVec;
		caratPositions[1,0] = new Vector2(botRightChoice.transform.localPosition.x, botRightChoice.transform.localPosition.y) + caratDiffVec;
		caratPositions[1,1] = new Vector2(topRightChoice.transform.localPosition.x, topRightChoice.transform.localPosition.y) + caratDiffVec;

		caratXInd = 0;
		caratYInd = 1;
	}

	private void AnimateCarat(float dt) {
		caratFlashTimer += dt;
		if(caratFlashTimer >= CARAT_FLASH_RATE) {
			caratFlashTimer -= CARAT_FLASH_RATE;

			caratChoice.gameObject.SetActive(!caratChoice.gameObject.activeSelf);
		}
	}

	private void HandleInput() {
		if(Input.GetKeyDown(KeyCode.DownArrow)) {
			caratYInd = 0;
			UpdateCaratPos();
		}

		if(Input.GetKeyDown(KeyCode.UpArrow)) {
			caratYInd = 1;
			UpdateCaratPos();
		}

		if(Input.GetKeyDown(KeyCode.LeftArrow)) {
			caratXInd = 0;
			UpdateCaratPos();
		}

		if(Input.GetKeyDown(KeyCode.RightArrow)) {
			caratXInd = 1;
			UpdateCaratPos();
		}
	}

	private void UpdateCaratPos() {
		Vector2 newPos = caratPositions[caratXInd, caratYInd];
		caratChoice.transform.localPosition = new Vector3(newPos.x, newPos.y);

		// Reset carat flash
		caratFlashTimer = 0f;
		caratChoice.gameObject.SetActive(true);
	}
}
