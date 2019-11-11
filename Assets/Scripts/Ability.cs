﻿using UnityEngine;
using UnityEditor;

public class Ability
{
	public string name;
	public int calmReq, discordReq, calmAdj, discordAdj, hpAdjMin, hpAdjMax;

	public Ability(string name, int calmReq, int discordReq, int calmAdj, int discordAdj, int hpAdjMin, int hpAdjMax) {
		this.name = name;
		this.calmReq = calmReq;
		this.discordReq = discordReq;
		this.calmAdj = calmAdj;
		this.discordAdj = discordAdj;
		this.hpAdjMin = hpAdjMin;
		this.hpAdjMax = hpAdjMax;
	}

	public override string ToString() {
		return "[abilityName: " + name + ", hpAdj: " + hpAdjMin + " to " + hpAdjMax + "]";
	}

	public override int GetHashCode() {
		return name.GetHashCode() + (calmReq ^ discordReq ^ calmAdj ^ discordAdj ^ hpAdjMin ^ hpAdjMax);
	}
}