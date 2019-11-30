/// <summary>
/// Class for Effect objects which adjust the target's HP by a given amount.
/// </summary>
public class HpModEffect : Effect
{
	private readonly int amount;

	public HpModEffect(int amount)
	{
		this.amount = amount;
	}


	/// <summary>
	/// Modifies the target HP.
	/// </summary>
	/// <param name="target">The CombatantController whose HP is being modified.</param>
	public override void DoEffect(CombatantController target)
	{
		target.HP += amount;
	}
}