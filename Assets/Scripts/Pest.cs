using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pest : MonoBehaviour
{
	public float ForceStrength = 1.0f;
	public float TargetReachedDistance = 0.5f;
	public float MaxDistanceToRibbon = 0.25f;
	public float RepulsionScale = 1.0f;

	private Rigidbody rb;
	private Vector3 targetPosition;
	private bool towardsHead = false;
	private RibbonSolve[] ribbonSolvers;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		ribbonSolvers = FindObjectsOfType<RibbonSolve>();
	}

	void setTarget(bool towardsHead)
	{
		Vector3 headPosition = Camera.main.transform.position;
		if (towardsHead)
		{
			targetPosition = headPosition + new Vector3(0.0f, Random.Range(-0.5f, 0.5f), 0.0f);
		}
		else
		{
			targetPosition = headPosition + new Vector3(Random.Range(-4.0f, 4.0f), Random.Range(-0.5f, 0.5f), Random.Range(-4.0f, 4.0f));
		}
	}

	void Update()
	{
		if (towardsHead)
		{
			bool closeEnough = false;
			for (int i = 0; i < ribbonSolvers.Length; i++)
			{
				RibbonSolve ribbonSolve = ribbonSolvers[i];
				Vector3 closestPosition;
				float distance = ribbonSolve.getClosestPoint(transform.position, out closestPosition);
				if (distance <= MaxDistanceToRibbon)
				{
					closeEnough = true;
					Vector3 repulsionForce = transform.position - closestPosition;
					rb.AddForce(repulsionForce * RepulsionScale, ForceMode.Impulse);
					break;
				}
			}
			if (closeEnough)
			{
				towardsHead = false;
				setTarget(towardsHead);
			}
		}

		Vector3 toTarget = (targetPosition - transform.position);
		if (toTarget.sqrMagnitude < TargetReachedDistance * TargetReachedDistance)
		{
			towardsHead = !towardsHead;
			setTarget(towardsHead);
		}		

		Vector3 toTargetDir = toTarget.normalized;
		rb.AddForce(toTargetDir * ForceStrength);
	}
}
