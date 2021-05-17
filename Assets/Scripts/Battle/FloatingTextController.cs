using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingTextController : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private GameObject textPrefab;

	[SerializeField]
	private Canvas gameUI;

	[SerializeField]
	private float floatDuration;

	[SerializeField]
	private float floatHeight;
#pragma warning restore 0649

	public void PlayTextForEffect(EffectData effect, Transform target)
	{
		int value = Mathf.Abs(effect.amount);

		GameObject textObj = Instantiate(textPrefab, gameUI.transform);
		TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();

		// Determine how to show the text
		switch (effect.stat.ToLower())
		{
			case "hp":
				tmp.SetText(value.ToString());
				tmp.color = effect.amount > 0 ? Color.green : Color.white;
				break;
			case "agility":
				tmp.SetText(effect.amount > 0 ? "+Agi" : "-Agi");
				tmp.color = effect.amount > 0 ? Color.green : Color.red;
				break;
		}
		
		textObj.transform.position = target.position;

		StartCoroutine(AnimateFloatingText(textObj));
	}

	
	private IEnumerator AnimateFloatingText(GameObject text)
	{
		float startY = text.transform.position.y;
		float endY = text.transform.position.y + floatHeight;
		float startTime = Time.time;
		float factor = 0;
		TextMeshProUGUI tmp = text.GetComponent<TextMeshProUGUI>();

		while (factor < 1)
		{
			float currY = Mathf.Lerp(startY, endY, factor);
			float currAlpha = Mathf.Lerp(1f, 0f, factor);

			factor = (Time.time - startTime) / floatDuration;

			text.transform.position = new Vector3(text.transform.position.x, currY, text.transform.position.z);
			tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, currAlpha);
			
			yield return null;
		}
	}
}
