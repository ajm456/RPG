using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonParser
{
	private const string JSON_ABILITIES_ROOT = "Assets/JSON/abilities/";

	// Load all JSON-specified abilities in the JSON_ABILITIES_ROOT folder into
	// a dictionary where the ability name is the key.
	public static Dictionary<string, Ability> LoadAllAbilities() {
		Dictionary<string, Ability> abilityDict = new Dictionary<string, Ability>();

		foreach(string filename in Directory.EnumerateFiles(JSON_ABILITIES_ROOT)) {
			// Watch out for Unity-generated .meta files
			if(filename.Contains(".meta")) {
				continue;
			}
			Debug.Log("Reading from file: " + filename);
			string json = File.ReadAllText(filename);
			Ability ability = JsonUtility.FromJson<Ability>(json);
			abilityDict.Add(ability.name, ability);
		}

		return abilityDict;
	}
}
