using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	public static PlayerManager instance;

	public List<GameObject> Pests;
	public List<GameObject> Collectibles;

	public float InitialHealth = 1000;
	
	public void Restart()
	{
		CurrentScore = 0;
		CurrentHealth = InitialHealth;

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
		CurrentHealth -= damage;

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
	}

	void Start()
	{
		instance = this;
		Restart();
	}
	
	void Update()
	{
		
	}
}
