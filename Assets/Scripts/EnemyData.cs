using System.Collections.Generic;

public class EnemyDataStrAbilities
{
	public string name;
	public int maxHp, strength, behaviourIndex;
	public List<string> abilityNames;

	public EnemyDataStrAbilities(string name, int maxHp, int strength, int behaviourIndex, List<string> abilityNames)
	{
		this.name = name;
		this.maxHp = maxHp;
		this.strength = strength;
		this.behaviourIndex = behaviourIndex;
		this.abilityNames = abilityNames;
	}
}

public class EnemyData
{
	private static readonly List<Ability> allAbilities = JsonParser.LoadAllAbilities();

	public string name;
	public int maxHp, strength, behaviourIndex;
	public List<Ability> calmAbilities, discordAbilities;

	public EnemyData(EnemyDataStrAbilities enemy)
	{
		name = enemy.name;
		maxHp = enemy.maxHp;
		strength = enemy.strength;
		behaviourIndex = enemy.behaviourIndex;

		// Filter to find this character's abilities
		calmAbilities = new List<Ability>();
		foreach (Ability ability in allAbilities)
		{
			if (enemy.abilityNames.Contains(ability.name))
			{
				if (ability.isCalm)
					calmAbilities.Add(ability);
				else
					discordAbilities.Add(ability);
			}
		}
	}

	public override string ToString()
	{
		string str = "[enemy name: " + name + ", max HP: " + maxHp + ", abilities: {";
		foreach (Ability ability in calmAbilities)
		{
			str += ability.name;
			str += ", ";
		}
		foreach (Ability ability in discordAbilities)
		{
			str += ability.name;
			str += ", ";
		}
		str += "}";

		return str;
	}
}
