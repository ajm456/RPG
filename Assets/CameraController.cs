using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField]
	private Animator cameraAnimator;

	[SerializeField]
	private GameObject blackBarContainer;

	[SerializeField]
	private BattleController battleController;

	public void PlayIntro()
	{
		// Show the black bars
		blackBarContainer.SetActive(true);

		// Start animation
		cameraAnimator.SetTrigger("sceneLoaded");
	}

	public void OnIntroFinish()
	{
		StartCoroutine(RetractBlackBars());
	}

	private IEnumerator RetractBlackBars()
	{
		RectTransform rectTransform = blackBarContainer.GetComponent<RectTransform>();
		while (rectTransform.sizeDelta.y < 1300)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + (500*Time.deltaTime));
			yield return null;
		}

		// Tell the battle controller to enable the UI
		battleController.UIEnabled = true;
	}
}
