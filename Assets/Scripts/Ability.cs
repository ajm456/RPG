using UnityEngine;
using UnityEditor;

public class Ability
{
	// Abilities (how does this interact with auras and such?)
	public string name;
	public int calmAdj, discordAdj, hpAdjMin, hpAdjMax;

	public Ability(string name, int calmAdj, int discordAdj, int hpAdjMin, int hpAdjMax) {
		this.name = name;
		this.calmAdj = calmAdj;
		this.discordAdj = discordAdj;
		this.hpAdjMin = hpAdjMin;
		this.hpAdjMax = hpAdjMax;
	}

	public override string ToString() {
		return "[abilityName: " + name + ", hpAdj: " + hpAdjMin + " to " + hpAdjMax + "]";
	}
}