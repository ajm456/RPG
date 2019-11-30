﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the (de)serialization of data to and from JSON files.
/// </summary>
public class JsonParser
{
	private const string JSON_EFFECTS_ROOT = "Assets/JSON/effects/";
	private const string JSON_AURAS_ROOT = "Assets/JSON/auras/";
	private const string JSON_ABILITIES_ROOT = "Assets/JSON/abilities/";
	private const string JSON_HEROES_ROOT = "Assets/JSON/characters/heroes/";
	private const string JSON_ENEMIES_ROOT = "Assets/JSON/characters/enemies/";


	public static List<EffectData> LoadAllEffects()
	{
		List<EffectData> effectList = new List<EffectData>();

		foreach (string filename in Directory.EnumerateFiles(JSON_EFFECTS_ROOT))
		{
			// Watch out for Unity-generated .meta files
			if (filename.Contains(".meta"))
			{
				continue;
			}
			string json = File.ReadAllText(filename);
			EffectData effect = JsonUtility.FromJson<EffectData>(json);
			effectList.Add(effect);
		}

		return effectList;
	}


	public static List<AuraData> LoadAllAuras()
	{
		List<AuraData> auraList = new List<AuraData>();

		foreach (string filename in Directory.EnumerateFiles(JSON_AURAS_ROOT))
		{
			// Watch out for Unity-generated .meta files
			if (filename.Contains(".meta"))
			{
				continue;
			}
			string json = File.ReadAllText(filename);
			AuraData aura = new AuraData(JsonUtility.FromJson<AuraDataJsonWrapper>(json));
			auraList.Add(aura);
		}

		return auraList;
	}
	

	/// <summary>
	/// Load all JSON-specified abilities in the JSON_ABILITIES_ROOT folder
	/// into a list.
	/// </summary>
	/// <returns>A List of all abilities specified in JSON.</returns>
	public static List<AbilityData> LoadAllAbilities()
	{
		List<AbilityData> abilityList = new List<AbilityData>();

		foreach (string filename in Directory.EnumerateFiles(JSON_ABILITIES_ROOT))
		{
			// Watch out for Unity-generated .meta files
			if (filename.Contains(".meta"))
			{
				continue;
			}
			string json = File.ReadAllText(filename);
			AbilityData ability = new AbilityData(JsonUtility.FromJson<AbilityDataJsonWrapper>(json));
			abilityList.Add(ability);
		}

		return abilityList;
	}
	

	/// <summary>
	/// Load the player characters with given names from JSON.
	/// </summary>
	/// <param name="heroNames">The names of the heroes being loaded.</param>
	/// <returns>A list of HeroData objects for each specified hero.</returns>
	public static List<HeroData> LoadHeroes(List<string> heroNames)
	{
		List<HeroData> heroList = new List<HeroData>();

		foreach (string filename in Directory.EnumerateFiles(JSON_HEROES_ROOT))
		{
			// Watch out for Unity-generated .meta files
			if (filename.Contains(".meta"))
			{
				continue;
			}
			// Only deserialize files in our list of character names
			if (!heroNames.Any(s => filename.ToUpperInvariant().Contains(s.ToUpperInvariant())))
			{
				continue;
			}

			string json = File.ReadAllText(filename);
			HeroDataJsonWrapper wrapper = JsonUtility.FromJson<HeroDataJsonWrapper>(json);
			HeroData character = new HeroData(JsonUtility.FromJson<HeroDataJsonWrapper>(json));
			heroList.Add(character);
		}

		return heroList;
	}


	/// <summary>
	/// Load the enemy characters with given names from JSON.
	/// </summary>
	/// <param name="enemyNames">The names of the enemies being loaded.</param>
	/// <returns>A list of EnemyData objects for each specified enemy.</returns>
	public static List<EnemyData> LoadEnemies(List<string> enemyNames)
	{
		List<EnemyData> enemyList = new List<EnemyData>();

		foreach (string filename in Directory.EnumerateFiles(JSON_ENEMIES_ROOT))
		{
			// Watch out for Unity-generated .meta files
			if (filename.Contains(".meta"))
			{
				continue;
			}
			// Only deserialize files in our list of character names
			if (!enemyNames.Any(s => filename.Contains(s)))
			{
				continue;
			}

			string json = File.ReadAllText(filename);
			EnemyData enemy = new EnemyData(JsonUtility.FromJson<EnemyDataJsonWrapper>(json));
			enemyList.Add(enemy);
		}

		return enemyList;
	}
}
