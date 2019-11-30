using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains deserialized data from hero JSON files, with their ability lists
/// in string form. To create a <see cref="HeroData"/> object, you must
/// instantiate this class and pass the object as an argument to
/// <see cref="HeroData"/>'s constructor.
/// </summary>
public class HeroDataJsonWrapper
{
	public string name;
	public List<int> color;
	public int hp, maxHp, strength, agility, calm, discord;
	public List<string> abilityNames;

	public HeroDataJsonWrapper(string name, List<int> color, int hp, int maxHp, int strength, int agility, int calm, int discord, List<string> abilityNames)
	{
		this.name = name;
		this.color = color;
		this.hp = hp;
		this.maxHp = maxHp;
		this.strength = strength;
		this.agility = agility;
		this.calm = calm;
		this.discord = discord;
		this.abilityNames = abilityNames;
	}
}


/// <summary>
/// Contains deserialized data from hero JSON files including lists of
/// fully-instantiated <see cref="Ability"/> objects.
/// </summary>
public class HeroData
{
	// Load all the abilities so we can filter by name to set this hero's
	// ability lists
	private static readonly List<Ability> allAbilities = JsonParser.LoadAllAbilities();

	public string name;
	public Color color;
	public int hp, maxHp, strength, agility, calm, discord;
	public List<Ability> calmAbilities, discordAbilities;

	public HeroData(HeroDataJsonWrapper wrapper)
	{
		name = wrapper.name;
		color = new Color(wrapper.color[0]/255f, wrapper.color[1]/255f, wrapper.color[2]/255f);
		hp =  wrapper.hp;
		maxHp = wrapper.maxHp;
		strength = wrapper.strength;
		agility = wrapper.agility;
		calm = wrapper.calm;
		discord = wrapper.discord;

		// Filter to find this hero's abilities
		calmAbilities = new List<Ability>();
		discordAbilities = new List<Ability>();
		foreach (Ability ability in allAbilities)
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
