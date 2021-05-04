using System.Collections.Generic;


/// <summary>
/// Represents abilities that combatant's can use in a battle. See the GDD for
/// a specific definition of what an ability is.
/// </summary>
public class AbilityDataJsonWrapper
{
	public string name;
	public bool isCalm;
	public int calmReq, strifeReq, calmGen, strifeGen;
	public List<string> effectNames;
	public List<string> auraNames;
	public string targetingMode;

	public AbilityDataJsonWrapper(string name, bool isCalm, int calmReq, int strifeReq, int calmGen, int strifeGen, List<string> effectNames, List<string> auraNames, string targetingMode)
	{
		this.name = name;
		this.isCalm = isCalm;
		this.calmReq = calmReq;
		this.strifeReq = strifeReq;
		this.calmGen = calmGen;
		this.strifeGen = strifeGen;
		this.effectNames = effectNames;
		this.auraNames = auraNames;
		this.targetingMode = targetingMode;
	}
}

public class AbilityData
{
	/// <summary>
	/// Represents the targeting mode of an ability:
	/// 
	/// Single - a single target is chosen.
	/// Party - all hero combatants are targeted simultaneously.
	/// Opposition - all enemy combatants are targeted simultaneously.
	/// Error - coult not discern the targeting mode from JSON.
	/// </summary>
	public enum TargetingMode
	{
		SINGLE,
		PARTY,
		OPPOSITION,
		ERROR
	}

	// Load all the effects so we can filter by name to set this ability's
	// list of instant effects
	private static readonly List<EffectData> allEffects = JsonParser.LoadAllEffects();

	// Load all the auras so we can filter by name to set this ability's
	// list of applied auras
	private static readonly List<AuraData> allAuras = JsonParser.LoadAllAuras();

	public string name;
	public bool isCalm;
	public int calmReq, strifeReq, calmGen, strifeGen;
	public List<EffectData> effects;
	public List<AuraData> auras;
	public TargetingMode targetingMode;

	public AbilityData(AbilityDataJsonWrapper wrapper)
	{
		name = wrapper.name;
		isCalm = wrapper.isCalm;
		calmReq = wrapper.calmReq;
		strifeReq = wrapper.strifeReq;
		calmGen = wrapper.calmGen;
		strifeGen = wrapper.strifeGen;

		// Filter to find this ability's effects
		effects = new List<EffectData>();
		foreach (string effectName in wrapper.effectNames)
		{
			effects.Add(allEffects.Find(d => d.name.ToUpperInvariant() == effectName.ToUpperInvariant()));
		}

		// Filter to find this ability's auras
		auras = new List<AuraData>();
		foreach (string auraName in wrapper.auraNames)
		{
			auras.Add(allAuras.Find(d => d.name.ToUpperInvariant() == auraName.ToUpperInvariant()));
		}

		// Discern this ability's targeting mode
		targetingMode =(wrapper.targetingMode.ToLowerInvariant()) switch
		{
			"single" => TargetingMode.SINGLE,
			"party" => TargetingMode.PARTY,
			"opposition" => TargetingMode.OPPOSITION,
			_ => throw new System.Exception("Could not discern ability's targeting mode"),
		};
	}

	/// <summary>
	/// Copy constructor.
	/// </summary>
	/// <param name="old">AbilityData object being copied from.</param>
	public AbilityData(AbilityData old)
	{
		name = old.name;
		isCalm = old.isCalm;
		calmReq = old.calmReq;
		strifeReq = old.strifeReq;
		calmGen = old.calmGen;
		strifeGen = old.strifeGen;
		effects = new List<EffectData>(old.effects);
		auras = new List<AuraData>(old.auras);
		targetingMode = old.targetingMode;
	}
}