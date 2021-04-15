using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BattleBlur : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private float intensity;
#pragma warning restore 0649

	private Material material;
	
	void Awake()
	{
		// Create a private material to be used for the effect
		material = new Material(Shader.Find("Hidden/BattleBlur"));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// Postprocess the image
		if (intensity == 0)
		{
			Graphics.Blit(source, destination);
			return;
		}

		material.SetFloat("_bwBlend", intensity);
		Graphics.Blit(source, destination, material);
	}
}
