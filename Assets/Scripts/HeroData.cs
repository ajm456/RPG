using System.Collections.Generic;
using UnityEngine;

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

public class HeroData
{
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

	public override string ToString()
	{
		string str = "[hero name: " + name + ", HP: " + hp + "/" + maxHp + ", abilities: {";
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
