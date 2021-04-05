using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelChanger : MonoBehaviour
{
	[SerializeField]
	private Image fadeable;

	public bool InputLocked
	{
		get;
		private set;
	}

	private void Start()
	{
		StartCoroutine("FadeIn");
	}

	private IEnumerator FadeIn()
	{
		InputLocked = true;

		Color initialColor = fadeable.color;
		initialColor.a = 0f;
		fadeable.color = initialColor;

		for (float ft = 1f; ft > 0f; ft -= 2*Time.deltaTime)
		{
			Color c = fadeable.color;
			c.a = ft;
			fadeable.color = c;
			yield return null;
		}

		Debug.Log("DONE!");
		InputLocked = false;
	}

	private IEnumerator FadeOut(string nextScene)
	{
		InputLocked = true;
		Color initialColor = fadeable.color;
		initialColor.a = 1f;
		fadeable.color = initialColor;

		for (float ft = 0f; ft < 1f; ft += 2*Time.deltaTime)
		{
			Color c = fadeable.color;
			c.a = ft;
			fadeable.color = c;
			yield return null;
		}

		SceneManager.LoadSceneAsync(nextScene);
	}

	public void LoadNextLevel(string levelName)
	{
		StartCoroutine("FadeOut", levelName);
	}
}
