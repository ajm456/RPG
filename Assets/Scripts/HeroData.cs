using System.Collections.Generic;

public class HeroDataStrAbilities
{
	public string name;
	public int hp, maxHp, calm, discord;
	public List<string> abilityNames;

	public HeroDataStrAbilities(string name, int hp, int maxHp, int calm, int discord, List<string> abilityNames) {
		this.name = name;
		this.hp = hp;
		this.maxHp = maxHp;
		this.calm = calm;
		this.discord = discord;
		this.abilityNames = abilityNames;
	}
}

public class HeroData
{
	private static readonly List<Ability> allAbilities = JsonParser.LoadAllAbilities();

	public string name;
	public int hp, maxHp, calm, discord;
	public List<Ability> abilities;

	public HeroData(HeroDataStrAbilities hero) {
		name = hero.name;
		hp =  hero.hp;
		maxHp = hero.maxHp;
		calm = hero.calm;
		discord = hero.discord;

		// Filter to find this hero's abilities
		abilities = new List<Ability>();
		foreach(Ability ability in allAbilities) {
			if(hero.abilityNames.Contains(ability.name)) {
				abilities.Add(ability);
			}
		}
	}

	public override string ToString() {
		string str = "[hero name: " + name + ", HP: " + hp + "/" + maxHp + ", abilities: {";
		foreach(Ability ability in abilities) {
			str += ability.name;
			str += ", ";
		}
		str += "}";

		return str;
	}
}
