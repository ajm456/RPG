using System.Collections.Generic;

/// <summary>
/// Contains deserialized data from enemy JSON files, with their ability lists
/// in string form. To create a <see cref="EnemyData"/> object, you must
/// instantiate this class and pass the object as an argument to
/// <see cref="EnemyData"/>'s constructor.
/// </summary>
public class EnemyDataJsonWrapper
{
	public string name;
	public int maxHp, strength, agility, behaviourIndex;
	public List<string> abilityNames;

	public EnemyDataJsonWrapper(string name, int maxHp, int strength, int agility, int behaviourIndex, List<string> abilityNames)
	{
		this.name = name;
		this.maxHp = maxHp;
		this.strength = strength;
		this.agility = agility;
		this.behaviourIndex = behaviourIndex;
		this.abilityNames = abilityNames;
	}
}

/// <summary>
/// Contains deserialized data from enemy JSON files including lists of
/// fully-instantiated <see cref="AbilityData"/> objects.
/// </summary>
public class EnemyData
{
	private static readonly List<AbilityData> allAbilities = JsonParser.LoadAllAbilities();

	public string name;
	public int maxHp, strength, agility, behaviourIndex;
	public List<AbilityData> calmAbilities, discordAbilities;

	public EnemyData(EnemyDataJsonWrapper wrapper)
	{
		name = wrapper.name;
		maxHp = wrapper.maxHp;
		strength = wrapper.strength;
		agility = wrapper.agility;
		behaviourIndex = wrapper.behaviourIndex;

		// Filter to find this character's abilities
		calmAbilities = new List<AbilityData>();
		discordAbilities = new List<AbilityData>();
		foreach (AbilityData ability in allAbilities)
		{
			if (wrapper.abilityNames.Contains(ability.name))
			{
				if (ability.isCalm)
					calmAbilities.Add(ability);
				else
					discordAbilities.Add(ability);
			}
		}
	}
}
