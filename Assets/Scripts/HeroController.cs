using UnityEngine;

public class HeroController : CombatantController
{
	public Color Color
	{
		get;
		set;
	}
	public int Calm
	{
		get;
		set;
	}
	public int Discord
	{
		get;
		set;
	}
	public int PartyOrder
	{
		get;
		set;
	}

	public void Init(HeroData data, BattleController battleController)
	{
		Name = data.name;
		Color = data.color;
		HP = data.hp;
		MaxHP = data.maxHp;
		CalmAbilities = data.calmAbilities;
		DiscordAbilities = data.discordAbilities;
		Calm = data.calm;
		Discord = data.discord;
		this.battleController = battleController;
		State = CombatantState.IDLE;
	}

	// Discern which ability the player is using, ensure they have sufficient
	// resources, etc.
	public override void PollForTurn()
	{
		//if(Input.GetKeyDown(KeyCode.Space)) {
		//	// Work out which ability the player is using
		//	Ability ability = DetermineAbility();

		//	// Determine player target
		//	CombatantController target = DetermineTarget();

		//	// Execute turn
		//	battleController.ExecuteTurn(this, ability, target);
		//	// Adjust source resource values
		//	Calm += ability.calmAdj;
		//	Discord += ability.discordAdj;
		//	Calm = Mathf.Clamp(Calm, 0, 100);
		//	Discord = Mathf.Clamp(Discord, 0, 100);
		//}
	}

	private Ability DetermineAbility()
	{
		return DiscordAbilities[0];
	}

	private CombatantController DetermineTarget()
	{
		return battleController.EnemyCombatants[0];
	}
}
