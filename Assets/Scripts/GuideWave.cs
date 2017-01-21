using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideWave : MonoBehaviour
{
	public GameObject GuidePrefab;
	public float GuideIntervalDistance = 0.15f;
	public float RestartDistanceToGuide = 0.3f;
	public float RestartTime = 0.5f;

	private GuideWaveCreator guideWaveCreator;
	private List<GameObject> guideGameObjects = new List<GameObject>();
	private List<Guide> guides = new List<Guide>();
	private Guide nextGuideToHit;
	private int nextGuideIndex;
	private float restartTimer;
	private RibbonController targetRibbonController;

	public int TargetHandIndex
	{
		get;
		private set;
	}

	public void Init(GuideWaveCreator guideWaveCreator, Bezier bezier, int targetHandIndex)
	{
		RibbonController[] ribbonControllers = FindObjectsOfType<RibbonController>();
		for (int i = 0; i < ribbonControllers.Length; i++)
		{
			if (ribbonControllers[i].HandId == targetHandIndex)
			{
				targetRibbonController = ribbonControllers[i];
				break;
			}
		}

		TargetHandIndex = targetHandIndex;
		LinePath linePath = bezier.getPath(100);
		int numGuides = (int)(linePath.Length / GuideIntervalDistance);
		for (int i = 0; i < numGuides; i++)
		{
			createGuide(linePath.Position);
			linePath.Advance(GuideIntervalDistance);
		}

		this.guideWaveCreator = guideWaveCreator;

		restart();
	}

	private void restart()
	{
		nextGuideIndex = 0;
		nextGuideToHit = guides[0];
		restartTimer = RestartTime;
		for (int i = 0; i < guides.Count; i++)
		{
			guides[i].setNeutral();
		}
		nextGuideToHit.setActive();
	}

	public void OnGuideHit(Guide guide)
	{
		if (guide == nextGuideToHit)
		{
			restartTimer = RestartTime;
			nextGuideIndex++;
			if (nextGuideIndex < guides.Count)
			{
				nextGuideToHit.setDimmed();
				nextGuideToHit = guides[nextGuideIndex];
				nextGuideToHit.setActive();
			}
			else
			{
				// We're done
				completed();
			}
		}
	}

	private void completed()
	{
		for (int i = 0; i < guideGameObjects.Count; i++)
		{
			Destroy(guideGameObjects[i]);
		}
		guideGameObjects.Clear();

		guideWaveCreator.OnGuideWaveCompleted(this);
	}

	public void Update()
	{
		if (nextGuideToHit != null && nextGuideIndex > 0)
		{
			restartTimer -= Time.deltaTime;
			if (restartTimer < 0.0f)
			{
				restart();
			}

			if (targetRibbonController != null && (nextGuideToHit.transform.position - targetRibbonController.getPosition()).magnitude > RestartDistanceToGuide)
			{
				restart();
			}
		}
	}

	private Guide createGuide(Vector3 position)
	{
		GameObject go = Instantiate(GuidePrefab) as GameObject;
		go.transform.position = position;
		go.transform.parent = transform;
		Guide guide = go.GetComponentInChildren<Guide>();
		guide.GuideWave = this;
		guides.Add(guide);
		guideGameObjects.Add(go);
		guide.SetColor(TargetHandIndex == 0 ? Color.red : Color.green);
		return guide;
	}
}
