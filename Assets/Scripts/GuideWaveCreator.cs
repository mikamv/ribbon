using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideWaveCreator : MonoBehaviour
{
	public GameObject Left;
	public GameObject Right;
	public List<Bezier> Beziers;

	public GameObject RibbonHandlePrefab;
	public GameObject GuideWavePrefab;
	
	public float WaveCooldown = 2.0f;

	private float createCooldown = 0.0f;
	private List<GuideWave> guideWaves = new List<GuideWave>();

	private GuideWave CreateGuideWave(Vector3 position, Quaternion localRotation, int targetHandIndex)
	{
		Bezier bezier = Beziers[Random.Range(0, Beziers.Count)];
		GameObject go = Instantiate(GuideWavePrefab) as GameObject;

		GuideWave guideWave = go.GetComponentInChildren<GuideWave>();
		guideWave.Init(this, bezier, targetHandIndex);

		go.transform.position = position;
		go.transform.localRotation = localRotation;
		
		guideWaves.Add(guideWave);
		return guideWave;
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
		
		Quaternion localRotation = Quaternion.AngleAxis(Random.Range(0.0f, 180.0f) + handTargetId * 180.0f, new Vector3(0.0f, 1.0f, 0.0f));

		CreateGuideWave(headPosition, localRotation, handTargetId);
	}

	public void OnGuideWaveCompleted(GuideWave guideWave)
	{
		guideWaves.Remove(guideWave);

		Destroy(guideWave.gameObject);
	}

	void Start()
	{
		RibbonController right = CreateRibbonHandle(Right);
		right.HandId = 0;
		right.SetColor(Color.red);
		RibbonController left = CreateRibbonHandle(Left);
		left.HandId = 1;
		left.SetColor(Color.green);
	}

	void Update()
	{
		createCooldown -= Time.deltaTime;
		if (guideWaves.Count < 2 && createCooldown <= 0.0f)
		{
			if (guideWaves.Count == 0)
			{
				CreateWave(0);
				CreateWave(1);
			}
			else
			{
				CreateWave((guideWaves[0].TargetHandIndex + 1) % 2);
			}
		}
	}
}
