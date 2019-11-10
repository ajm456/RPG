using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject uiStatusPrefab;
	private GameObject uiStatus;
	private TextMeshProUGUI nameText, hpValue;
	private RectTransform missingHealthBar, healthBar;
	private HeroController heroController;

	void Start() {
		heroController = GetComponent<HeroController>();

		uiStatus = Instantiate(uiStatusPrefab, GameObject.Find("HealthBarsContainer").transform);
		float yOffset = heroController.PartyOrder * 90f;
		uiStatus.transform.localPosition = new Vector3(uiStatus.transform.localPosition.x, uiStatus.transform.localPosition.y - yOffset);

		nameText = uiStatus.transform.Find("Name").GetComponent<TextMeshProUGUI>();
		hpValue = uiStatus.transform.Find("HPValue").GetComponent<TextMeshProUGUI>();
		healthBar = uiStatus.transform.Find("HealthBar").GetComponent<RectTransform>();
		missingHealthBar = healthBar.transform.Find("MissingHealthBar").GetComponent<RectTransform>();

		// Set the text to our hero name
		nameText.SetText(heroController.Name.ToUpper());
	}

	void Update() {
		// Update the hit point numerical indicator
		hpValue.SetText(heroController.HP.ToString() + " / " + heroController.MaxHP.ToString());

		// Update the missing health mask
		float missingHealth = heroController.MaxHP - heroController.HP;
		float percentageMissing = missingHealth / heroController.MaxHP;
		float newWidth = percentageMissing * healthBar.sizeDelta.x;
		//float newX = -newWidth;
		//missingHealthBar.transform.localPosition = new Vector3(newX, missingHealthBar.transform.localPosition.y);
		missingHealthBar.sizeDelta = new Vector2(newWidth, missingHealthBar.sizeDelta.y);
	}

	void OnDestroy() {
		Destroy(uiStatus);
	}
}
