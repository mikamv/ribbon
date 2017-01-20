using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideWaveCreator : MonoBehaviour
{
	public GameObject Left;
	public GameObject Right;

	public GameObject RibbonHandlePrefab;
	public GameObject GuidePrefab;
	public float ArcLength = 90.0f;
	public float DistanceFromHead = 0.6f;
	public float HeightVariation = 0.25f;
	public float WaveCooldown = 2.0f;

	private float createCooldown = 0.0f;
	private List<Guide> guides = new List<Guide>();

	private void CreateGuide(Vector3 position, float orderFraction)
	{
		GameObject go = Instantiate(GuidePrefab) as GameObject;
		go.transform.position = position;
		Guide guide = go.GetComponentInChildren<Guide>();
		guide.GuideWaveCreator = this;
		guide.OrderFraction = orderFraction;
		guides.Add(guide);
	}

	private void CreateRibbonHandle(GameObject parentGO)
	{
		GameObject go = Instantiate(RibbonHandlePrefab) as GameObject;
		go.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		go.transform.SetParent(parentGO.transform, false);
	}

	private void CreateWave()
	{
		Vector3 headPosition = Camera.main.transform.position;
		int NumGuidesToCreate = 20;
		float startAngle = -ArcLength * 0.5f;
		float arcStep = ArcLength / NumGuidesToCreate;
		float currentAngle = startAngle;
		for (int i = 0; i < NumGuidesToCreate; i++)
		{
			float fraction = (float)i / (float)(NumGuidesToCreate - 1);
			Vector3 position = headPosition + new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(fraction * 8.0f) * HeightVariation, Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * DistanceFromHead;
			CreateGuide(position, fraction);
			currentAngle += arcStep;
		}
	}

	public void OnGuideCollected(Guide guide)
	{
		guides.Remove(guide);
		if (guides.Count == 0)
		{
			createCooldown = WaveCooldown;
		}
	}

	void Start()
	{
		CreateRibbonHandle(Left);
		CreateRibbonHandle(Right);
	}

	void Update()
	{
		createCooldown -= Time.deltaTime;
		if (guides.Count == 0 && createCooldown <= 0.0f)
		{
			CreateWave();
		}
	}
}
