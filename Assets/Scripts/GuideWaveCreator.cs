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
	private Guide[] nextGuideToHit = new Guide[2];

	private Guide CreateGuide(Vector3 position, float orderFraction)
	{
		GameObject go = Instantiate(GuidePrefab) as GameObject;
		go.transform.position = position;
		Guide guide = go.GetComponentInChildren<Guide>();
		guide.GuideWaveCreator = this;
		guide.OrderFraction = orderFraction;
		guides.Add(guide);
		return guide;
	}

	private RibbonController CreateRibbonHandle(GameObject parentGO)
	{
		GameObject go = Instantiate(RibbonHandlePrefab) as GameObject;
		go.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
		go.transform.SetParent(parentGO.transform, false);
		return go.GetComponentInChildren<RibbonController>();
	}

	private void CreateWave(int handTargetId)
	{
		Vector3 headPosition = Camera.main.transform.position;
		int NumGuidesToCreate = 20;
		float startAngle = -ArcLength * 0.5f;
		float angleOfs = Random.Range(-25.0f, 25.0f);
		float arcStep = ArcLength / NumGuidesToCreate;
		float currentAngle = startAngle;
		float yOfs = Random.Range(-0.1f, 0.1f);
		Quaternion localRotation = Quaternion.AngleAxis(Random.Range(-45.0f, 45.0f), new Vector3(1.0f, 0.0f, 0.0f));
		for (int i = 0; i < NumGuidesToCreate; i++)
		{
			float fraction = (float)i / (float)(NumGuidesToCreate - 1);
			float angle = currentAngle + angleOfs;
			Vector3 position = headPosition + localRotation * new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(fraction * 8.0f) * HeightVariation + yOfs, Mathf.Sin(angle * Mathf.Deg2Rad)) * DistanceFromHead;
			Guide guide = CreateGuide(position, fraction);
			guide.HandTargetId = handTargetId;
			guide.SetColor(handTargetId == 0 ? Color.red : Color.green);
			if (i == 0)
			{
				nextGuideToHit[handTargetId] = guide;
			}
			currentAngle += arcStep;
		}
	}

	public bool CanDestroyGuide(Guide guide)
	{
		return nextGuideToHit[guide.HandTargetId] == guide;
	}

	public void OnGuideCollected(Guide guide)
	{
		guides.Remove(guide);

		if (guides.Count == 0)
		{
			createCooldown = WaveCooldown;
		}
		else
		{
			for (int i = 0; i < guides.Count; i++)
			{
				if (guides[i].HandTargetId == guide.HandTargetId)
				{
					nextGuideToHit[guide.HandTargetId] = guides[i];
					break;
				}
			}
			
		}
	}

	void Start()
	{
		RibbonController right = CreateRibbonHandle(Right);
		right.HandId = 0;
		RibbonController left = CreateRibbonHandle(Left);
		left.HandId = 1;
	}

	void Update()
	{
		createCooldown -= Time.deltaTime;
		if (guides.Count == 0 && createCooldown <= 0.0f)
		{
			CreateWave(0);
			CreateWave(1);
		}
	}
}
