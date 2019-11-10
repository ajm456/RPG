using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class JsonParser
{
	private const string JSON_ABILITIES_ROOT = "Assets/JSON/abilities/";
	private const string JSON_HEROES_ROOT = "Assets/JSON/characters/heroes/";
	private const string JSON_ENEMIES_ROOT = "Assets/JSON/characters/enemies/";

	// Load all JSON-specified abilities in the JSON_ABILITIES_ROOT folder into
	// a dictionary where the ability name is the key.
	public static List<Ability> LoadAllAbilities() {
		List<Ability> abilityList = new List<Ability>();

		foreach(string filename in Directory.EnumerateFiles(JSON_ABILITIES_ROOT)) {
			// Watch out for Unity-generated .meta files
			if(filename.Contains(".meta")) {
				continue;
			}
			string json = File.ReadAllText(filename);
			Ability ability = JsonUtility.FromJson<Ability>(json);
			abilityList.Add(ability);
		}

		return abilityList;
	}

	// Load the player characters with given names from JSON
	public static List<HeroData> LoadHeroes(List<string> heroNames) {
		List<HeroData> heroList = new List<HeroData>();

		foreach(string filename in Directory.EnumerateFiles(JSON_HEROES_ROOT)) {
			// Watch out for Unity-generated .meta files
			if(filename.Contains(".meta")) {
				continue;
			}
			// Only deserialize files in our list of character names
			if(!heroNames.Any(s => filename.ToUpperInvariant().Contains(s.ToUpperInvariant()))) {
				continue;
			}
			
			string json = File.ReadAllText(filename);
			HeroData character = new HeroData(JsonUtility.FromJson<HeroDataStrAbilities>(json));
			heroList.Add(character);
		}

		return heroList;
	}

	public static List<EnemyData> LoadEnemies(List<string> enemyNames) {
		List<EnemyData> enemyList = new List<EnemyData>();

		foreach(string filename in Directory.EnumerateFiles(JSON_ENEMIES_ROOT)) {
			// Watch out for Unity-generated .meta files
			if(filename.Contains(".meta")) {
				continue;
			}
			// Only deserialize files in our list of character names
			if(!enemyNames.Any(s => filename.Contains(s))) {
				continue;
			}
			
			string json = File.ReadAllText(filename);
			EnemyData enemy = new EnemyData(JsonUtility.FromJson<EnemyDataStrAbilities>(json));
			enemyList.Add(enemy);
		}

		return enemyList;
	}
}
