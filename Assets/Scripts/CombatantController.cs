using UnityEngine;
using TMPro;

public class CombatantController : MonoBehaviour
{
	/* MEMBERS */
	[SerializeField] private int initHp;
	[SerializeField] private TextMeshProUGUI hpText, calmText, discordText;

	public int CurrHP {
		get;
		set;
	}
	public int CurrCalm {
		get;
		set;
	}
	public int CurrDiscord {
		get;
		set;
	}

	[SerializeField] private string combatantName;
	public string Name {
		get { return combatantName; }
	}





	/* METHODS */
	void Start() {
		CurrHP = initHp;
		CurrCalm = 0;
		CurrDiscord = 0;
	}

	void Update() {
		hpText.SetText(CurrHP.ToString());
		calmText.SetText(CurrCalm.ToString());
		discordText.SetText(CurrDiscord.ToString());
	}
}
