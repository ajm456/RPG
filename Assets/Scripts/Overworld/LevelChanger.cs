using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages changing between different overworld scenes.
/// </summary>
public class LevelChanger : MonoBehaviour
{
	/// <summary>
	/// Black UI image used for fading in and out between transitions.
	/// </summary>
	[SerializeField]
	private Image fadeable;

	/// <summary>
	/// Property checked by PlayerController when reading input. Input is
	/// locked during scene transitions.
	/// </summary>
	public bool InputLocked
	{
		get;
		private set;
	}

	private void Start()
	{
		StartCoroutine(nameof(FadeIn));
	}

	/// <summary>
	/// Public method for starting the loading of the next level including fade
	/// to black.
	/// </summary>
	/// <param name="levelName">The name of the scene being loaded.</param>
	public void LoadNextLevel(string levelName)
	{
		StartCoroutine(nameof(FadeOut), levelName);
	}

	/// <summary>
	/// Fades away a screen-sized black cover. Used when loading into a new
	/// scene.
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Fades in a screen-sized black cover. Used when de-loading the current
	/// scene. Loads the specified next scene once fading is complete.
	/// </summary>
	/// <param name="nextScene">The name of the next scene to be loaded.</param>
	/// <returns></returns>
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
}
