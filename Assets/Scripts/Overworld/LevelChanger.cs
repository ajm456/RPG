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
		StartCoroutine(FadeOut(Color.black, levelName));
	}

	public void LoadBattle(EncounterData data)
	{
		EncounterDataStaticContainer.SetData(data);
		StartCoroutine(FadeOut(Color.white, "PT_BattleScene"));
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
	/// Fades in a screen-sized cover of the given color. Used when de-loading
	/// the current scene. Loads the specified next scene once fading is
	/// complete.
	/// </summary>
	/// <param name="color">The color being faded to.</param>
	/// <param name="nextScene">The name of the next scene to be loaded.</param>
	/// <returns></returns>
	private IEnumerator FadeOut(Color color, string nextScene)
	{
		InputLocked = true;
		color.a = 1f;
		fadeable.color = color;

		for (float ft = 0f; ft <= 1f; ft += 2*Time.deltaTime)
		{
			Color c = fadeable.color;
			c.a = ft;
			fadeable.color = c;
			yield return null;
		}

		Color cf = fadeable.color;
		cf.a = 1f;
		fadeable.color = cf;

		SceneManager.LoadSceneAsync(nextScene);
	}
}
