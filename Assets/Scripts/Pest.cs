using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pest : MonoBehaviour
{
	public float ForceStrength = 1.0f;
	public float TargetReachedDistance = 0.5f;
	public float MaxDistanceToRibbon = 0.25f;
	public float RepulsionScale = 1.0f;
    public bool IsRed;

	private Rigidbody rb;
	private Vector3 targetPosition;
	private bool towardsHead = false;
	private RibbonSolve ribbonSolver;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		RibbonSolve[] ribbonSolvers = FindObjectsOfType<RibbonSolve>();
        for (int i = 0; i < ribbonSolvers.Length; i++)
        {
            if ((ribbonSolvers[i].isRed && IsRed) || (!ribbonSolvers[i].isRed && !IsRed))
            {
                ribbonSolver = ribbonSolvers[i];
            }
        }
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
		Vector3 closestPosition;
		float distance = ribbonSolver.getClosestPoint(transform.position, out closestPosition);
		if (distance <= MaxDistanceToRibbon)
		{
			Vector3 repulsionForce = transform.position - closestPosition;
			rb.AddForce(repulsionForce * RepulsionScale, ForceMode.Impulse);
            if (towardsHead)
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
