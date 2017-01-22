using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
	public static PlayerManager instance;

	public GameObject HUD;
	public Text ScoreAmount;
	public Text HealthAmount;

	public List<GameObject> Pests;
	public List<GameObject> Collectibles;

	public float InitialHealth = 1000;

	public float MaxFirePower = 1.0f;
	public float FirePowerConsumePerSecond = 1.0f;
	public float FirePowerRechargePerSecond = 0.2f;

	private float firePowerRed;
	private float firePowerGreen;

	private SteamVR_TrackedController leftController;
	private SteamVR_TrackedController rightController;

	public bool HasFirePower(bool isRed)
	{
		return (isRed ? firePowerRed : firePowerGreen) > 0.0f;
	}

	public void ConsumeFirePower(bool isRed)
	{
		if (isRed)
		{
			firePowerRed = Mathf.Clamp(firePowerRed - Time.deltaTime * FirePowerConsumePerSecond, 0, MaxFirePower);
		}
		else
		{
			firePowerGreen = Mathf.Clamp(firePowerGreen - Time.deltaTime * FirePowerConsumePerSecond, 0, MaxFirePower);
		}
	}

	private void UpdateFirePower()
	{
		firePowerRed = Mathf.Clamp(firePowerRed + Time.deltaTime * FirePowerRechargePerSecond, 0, MaxFirePower);
		firePowerGreen = Mathf.Clamp(firePowerGreen + Time.deltaTime * FirePowerRechargePerSecond, 0, MaxFirePower);
	}

	public void Restart()
	{
		CurrentScore = 0;
		CurrentHealth = InitialHealth;

		firePowerRed = MaxFirePower;
		firePowerGreen = MaxFirePower;

		foreach (GameObject go in Pests)
		{
			go.SetActive(true);
			go.GetComponent<Pest>().Restart();
		}

		foreach (GameObject go in Collectibles)
		{
			go.SetActive(true);
			go.GetComponent<Collectible>().Restart();
		}

		ScoreAmount.text = "" + CurrentScore;
		HealthAmount.text = "" + (int)CurrentHealth;
	}

	public int CurrentScore
	{
		get;
		private set;
	}

	public float CurrentHealth
	{
		get;
		private set;
	}

	public void DealDamage(float damage)
	{
		CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, InitialHealth);
		HealthAmount.text = "" + (int)CurrentHealth;

		if (IsDead)
		{
			EndGame();
		}
	}

	private void EndGame()
	{
		foreach (GameObject go in Pests)
		{
			go.SetActive(false);
		}

		foreach (GameObject go in Collectibles)
		{
			go.SetActive(false);
		}
	}

	public bool IsDead
	{
		get
		{
			return CurrentHealth <= 0;
		}
	}

	public void AddScore(int amount)
	{
		CurrentScore += amount;
		ScoreAmount.text = "" + CurrentScore;
	}

	void Start()
	{
		instance = this;
		Restart();

		SteamVR_ControllerManager controllerManager = FindObjectOfType<SteamVR_ControllerManager>();
		if (controllerManager != null)
		{
			leftController = controllerManager.left.GetComponent<SteamVR_TrackedController>();
			rightController = controllerManager.right.GetComponent<SteamVR_TrackedController>();
		}

		HUD.transform.parent = leftController.transform;
	}
	
	void Update()
	{
		if (leftController.padPressed || rightController.padPressed)
		{
			Restart();
		}

		UpdateFirePower();
	}
}
