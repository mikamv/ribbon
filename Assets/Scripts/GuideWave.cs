using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideWave : MonoBehaviour
{
	public GameObject GuidePrefab;
	public float GuideIntervalDistance = 0.15f;
	public float RestartDistanceToGuide = 0.3f;
	public float RestartTime = 0.5f;

	public int ScaleEffectGuideCount = 3;
	public float ScaleEffectSize = 2.0f;

	private GuideWaveCreator guideWaveCreator;
	private List<GameObject> guideGameObjects = new List<GameObject>();
	private List<Guide> guides = new List<Guide>();
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
		int numGuides = (int)(linePath.Length / GuideIntervalDistance) + 1;
		for (int i = 0; i < numGuides; i++)
		{
			createGuide(linePath.Position, linePath.Direction);
			linePath.Advance(GuideIntervalDistance);
		}

		this.guideWaveCreator = guideWaveCreator;

		restart();
	}

	private void restart()
	{
		nextGuideIndex = 0;
		restartTimer = RestartTime;
		for (int i = 0; i < guides.Count; i++)
		{
			guideGameObjects[i].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			guides[i].setNeutral();
		}
		guides[0].setActive();
	}

	public void OnGuideHit(Guide guide)
	{
		for (int i = nextGuideIndex; i < Mathf.Clamp(nextGuideIndex + ScaleEffectGuideCount, 0, guides.Count - 1); i++)
		{
			if (guide == guides[i])
			{
				restartTimer = RestartTime;
				if (nextGuideIndex + 1 < guides.Count)
				{
					SetNextTarget(nextGuideIndex + 1);
				}
				else
				{
					// We're done
					completed();
				}
				break;
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

	private void SetNextTarget(int index)
	{
		// Dim ones that have been triggered
		for (int i = nextGuideIndex; i < index; i++)
		{
			guides[i].setDimmed();
		}

		nextGuideIndex = index;

		// Show next one as active
		guides[nextGuideIndex].setActive();

		// Scale smoothly around the active one
		for (int i = -ScaleEffectGuideCount; i <= ScaleEffectGuideCount; i++)
		{
			float scale = 1.0f + (1.0f - (Mathf.Abs(i) / (float)ScaleEffectGuideCount)) * ScaleEffectSize;
			int currentIndex = nextGuideIndex + i;
			if (currentIndex >= 0 & currentIndex < guideGameObjects.Count)
			{
				guideGameObjects[currentIndex].transform.localScale = new Vector3(scale, scale, scale);
			}
		}
	}

	public void Update()
	{
		if (nextGuideIndex > 0)
		{
			restartTimer -= Time.deltaTime;
			if (restartTimer < 0.0f)
			{
				restart();
			}
			/*
			if (targetRibbonController != null && (nextGuideToHit.transform.position - targetRibbonController.getPosition()).magnitude > RestartDistanceToGuide)
			{
				restart();
			}
			*/
		}
	}

	private Guide createGuide(Vector3 position, Vector3 direction)
	{
		GameObject go = Instantiate(GuidePrefab) as GameObject;
		go.transform.position = position;
		go.transform.localRotation = Quaternion.LookRotation(direction);
		go.transform.parent = transform;
		Guide guide = go.GetComponentInChildren<Guide>();
		guide.GuideWave = this;
		guide.HandTargetId = TargetHandIndex;
		guides.Add(guide);
		guideGameObjects.Add(go);
		guide.SetColor(TargetHandIndex == 0 ? Color.red : Color.green);
		return guide;
	}
}
