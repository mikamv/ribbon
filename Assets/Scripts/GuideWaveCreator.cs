using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideWaveCreator : MonoBehaviour
{
	public GameObject Left;
	public GameObject Right;

	public GameObject RibbonHandlePrefab;
	public GameObject GuidePrefab;
	public int NumGuidesToCreate = 5;
	public float DistanceFromHead = 0.6f;
	public float WaveCooldown = 2.0f;

	private float createCooldown = 0.0f;
	private List<Guide> guides = new List<Guide>();

	private void CreateGuide(Vector3 position)
	{
		GameObject go = Instantiate(GuidePrefab) as GameObject;
		go.transform.position = position;
		Guide guide = go.GetComponent<Guide>();
		guide.GuideWaveCreator = this;
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
		for (int i = 0; i < NumGuidesToCreate; i++)
		{
			float fraction = (float)i / (float)(NumGuidesToCreate - 1);
			Vector2 rnd2d = Random.insideUnitCircle;
			Vector3 position = headPosition + new Vector3(rnd2d.x, Mathf.Sin(fraction), rnd2d.y) * DistanceFromHead;
			CreateGuide(position);
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
