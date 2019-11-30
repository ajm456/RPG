using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Represents abilities that combatant's can use in a battle. See the GDD for
/// a specific definition of what an ability is.
/// </summary>
public class AbilityDataJsonWrapper
{
	public string name;
	public bool isCalm;
	public int calmReq, discordReq;
	public List<string> effectNames;
	public List<string> auraNames;

	public AbilityDataJsonWrapper(string name, bool isCalm, int calmReq, int discordReq, List<string> effectNames, List<string> auraNames)
	{
		this.name = name;
		this.isCalm = isCalm;
		this.calmReq = calmReq;
		this.discordReq = discordReq;
		this.effectNames = effectNames;
		this.auraNames = auraNames;
	}
}

public class AbilityData
{
	// Load all the effects so we can filter by name to set this ability's
	// list of instant effects
	private static readonly List<EffectData> allEffects = JsonParser.LoadAllEffects();

	// Load all the auras so we can filter by name to set this ability's
	// list of applied auras
	private static readonly List<AuraData> allAuras = JsonParser.LoadAllAuras();

	public string name;
	public bool isCalm;
	public int calmReq, discordReq;
	public List<EffectData> effects;
	public List<AuraData> auras;

	public AbilityData(AbilityDataJsonWrapper wrapper)
	{
		name = wrapper.name;
		isCalm = wrapper.isCalm;
		calmReq = wrapper.calmReq;
		discordReq = wrapper.discordReq;

		// Filter to find this ability's effects
		effects = new List<EffectData>();
		foreach (string effectName in wrapper.effectNames)
			effects.Add(allEffects.Find(d => d.name.ToUpperInvariant() == effectName.ToUpperInvariant()));

		// Filter to find this ability's auras
		auras = new List<AuraData>();
		foreach (string auraName in wrapper.auraNames)
			auras.Add(allAuras.Find(d => d.name.ToUpperInvariant() == auraName.ToUpperInvariant()));
	}
}