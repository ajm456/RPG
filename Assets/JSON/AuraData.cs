using System.Collections.Generic;

public class AuraDataJsonWrapper
{
	public string name;
	public List<string> effectNames;

	public AuraDataJsonWrapper(string name, List<string> effectNames)
	{
		this.name = name;
		this.effectNames = effectNames;
	}
}


public class AuraData
{
	// List all effects so that we can determine the list of per-turn effects
	// this aura applies
	private static readonly List<EffectData> allEffects = JsonParser.LoadAllEffects();

	public string name;
	public List<EffectData> effects;
	public int effectTurnIndex;

	public AuraData(AuraDataJsonWrapper wrapper)
	{
		name = wrapper.name;

		// Filter to find this aura's effects
		effects = new List<EffectData>();
		foreach (string effectName in wrapper.effectNames)
		{
			effects.Add(allEffects.Find(d => d.name.ToUpperInvariant() == effectName.ToUpperInvariant()));
		}
		effectTurnIndex = 0;
	}
}
