/// <summary>
/// <para>
/// Abstract base class for effects. See the GDD for a specific definition of
/// what effects are.
/// </para>
/// <para>
/// To create a new effect, simply subclass this and implement the DoEffect
/// method.
/// </para>
/// </summary>
public abstract class Effect
{
	/// <summary>
	/// Applies the effect to a given combatant.
	/// </summary>
	/// <param name="target">The CombatantController receiving the effect.</param>
	public abstract void DoEffect(CombatantController target);
}
