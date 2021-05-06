/// <summary>
/// Represents effects, the foundations of auras and abilities See the GDD for
/// a specific definition of what an effect is.
/// </summary>
public class EffectData
{
	public string name, stat;
	public int amount;
	public float strengthScaling;
	public bool canCrit;

	public bool IsEmpty()
	{
		return name == "Empty";
	}

	public EffectData(string name, string stat, int amount, float strengthScaling, bool canCrit)
	{
		this.name = name;
		this.stat = stat;
		this.amount = amount;
		this.strengthScaling = strengthScaling;
		this.canCrit = canCrit;
	}
}
