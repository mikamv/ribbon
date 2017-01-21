using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideWave : MonoBehaviour
{
	public GameObject GuidePrefab;
	public float GuideIntervalDistance = 0.15f;
	public float RestartDistanceToGuide = 0.3f;
	public float RestartTime = 0.5f;
	public float CollectTime = 1.0f;

	public int ScaleEffectGuideCount = 3;
	public float ScaleEffectSize = 2.0f;

	public float MaxDistanceToRibbon = 0.2f;

	private GuideWaveCreator guideWaveCreator;
	private List<GameObject> guideGameObjects = new List<GameObject>();
	private List<Guide> guides = new List<Guide>();
	private int nextGuideIndex;
	private float restartTimer;
	private RibbonController targetRibbonController;

	private SteamVR_TrackedController leftController;
	private SteamVR_TrackedController rightController;

	private RibbonSolve[] ribbonSolvers;

	private float collectTimer;

	public void Start()
	{
		ribbonSolvers = FindObjectsOfType<RibbonSolve>();
		SteamVR_ControllerManager controllerManager = FindObjectOfType<SteamVR_ControllerManager>();
		if (controllerManager != null)
		{
            leftController = controllerManager.left.GetComponent<SteamVR_TrackedController>();
            rightController = controllerManager.right.GetComponent<SteamVR_TrackedController>();
		}
	}

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
	}

	public void OnGuideHit(Guide guide)
	{
	}

	private void collect()
	{
		if (ribbonSolvers != null)
		{
			for (int j = 0; j < guides.Count; j++)
			{
				bool closeEnough = false;
				for (int i = 0; i < ribbonSolvers.Length; i++)
				{
					RibbonSolve ribbonSolve = ribbonSolvers[i];
					float distance = ribbonSolve.getClosestDistance(guides[j].transform.position);
					closeEnough = closeEnough || (distance <= MaxDistanceToRibbon);
				}
				if (!closeEnough)
				{
					Destroy(guideGameObjects[j]);
				}
				else
				{
					guides[j].Collect();
				}
			}
		}

		guides.Clear();
		guideGameObjects.Clear();

		collectTimer = CollectTime;
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
		if (leftController.triggerPressed || rightController.triggerPressed)
		{
			collect();
			return;
		}

		if (collectTimer > 0.0f)
		{
			collectTimer -= Time.deltaTime;
			if (collectTimer <= 0.0f)
			{
				guideWaveCreator.OnGuideWaveCompleted(this);
			}
			return;
		}

		if (ribbonSolvers != null)
		{
			for (int j = 0; j < guides.Count; j++)
			{
				bool closeEnough = false;
				for (int i = 0; i < ribbonSolvers.Length; i++)
				{
					RibbonSolve ribbonSolve = ribbonSolvers[i];
					float distance = ribbonSolve.getClosestDistance(guides[j].transform.position);
					closeEnough = closeEnough || (distance <= MaxDistanceToRibbon);
				}
				if (closeEnough)
				{
					guides[j].setActive();
				}
				else
				{
					guides[j].setNeutral();
				}
			}
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
