using System.Collections.Generic;
using UnityEngine;

public class HeroDataJsonWrapper
{
	public string name;
	public List<int> color;
	public int hp, maxHp, strength, calm, discord;
	public List<string> abilityNames;

	public HeroDataJsonWrapper(string name, List<int> color, int hp, int maxHp, int strength, int calm, int discord, List<string> abilityNames)
	{
		this.name = name;
		this.color = color;
		this.hp = hp;
		this.maxHp = maxHp;
		this.strength = strength;
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
	public int hp, maxHp, strength, calm, discord;
	public List<Ability> calmAbilities, discordAbilities;

	public HeroData(HeroDataJsonWrapper hero)
	{
		name = hero.name;
		color = new Color(hero.color[0]/255f, hero.color[1]/255f, hero.color[2]/255f);
		hp =  hero.hp;
		maxHp = hero.maxHp;
		strength = hero.strength;
		calm = hero.calm;
		discord = hero.discord;

		// Filter to find this hero's abilities
		calmAbilities = new List<Ability>();
		discordAbilities = new List<Ability>();
		foreach (Ability ability in allAbilities)
		{
			if (hero.abilityNames.Contains(ability.name))
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
