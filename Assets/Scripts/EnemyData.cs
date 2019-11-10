using System.Collections.Generic;

public class EnemyDataStrAbilities
{
	public string name;
	public int maxHp, behaviourIndex;
	public List<string> abilityNames;

	public EnemyDataStrAbilities(string name, int maxHp, int behaviourIndex, List<string> abilityNames) {
		this.name = name;
		this.maxHp = maxHp;
		this.behaviourIndex = behaviourIndex;
		this.abilityNames = abilityNames;
	}
}

public class EnemyData
{
	private static readonly List<Ability> allAbilities = JsonParser.LoadAllAbilities();

	public string name;
	public int maxHp, behaviourIndex;
	public List<Ability> abilities;

	public EnemyData(EnemyDataStrAbilities enemy) {
		name = enemy.name;
		maxHp = enemy.maxHp;
		behaviourIndex = enemy.behaviourIndex;

		// Filter to find this character's abilities
		abilities = new List<Ability>();
		foreach(Ability ability in allAbilities) {
			if(enemy.abilityNames.Contains(ability.name)) {
				abilities.Add(ability);
			}
		}
	}

	public override string ToString() {
		string str = "[enemy name: " + name + ", max HP: " + maxHp + ", abilities: {";
		foreach(Ability ability in abilities) {
			str += ability.name;
			str += ", ";
		}
		str += "}";

		return str;
	}
}
