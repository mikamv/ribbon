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
	public GameObject Beam;
	public float BeamTime = 0.2f;
	public float DamagePerSecond = 100.0f;

	private Rigidbody rb;
	private Vector3 targetPosition;
	private bool towardsHead = false;
	private RibbonSolve ribbonSolver;
	private float beamTimer;
	private float beamOffset;

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

		Restart();
	}

	public void Restart()
	{
		beamOffset = Random.Range(0.1f, 0.35f);
		Beam.SetActive(false);
		beamTimer = 0.0f;
		Vector3 headPosition = Camera.main.transform.position;
		Vector2 randomDir2d = Random.insideUnitCircle.normalized;
		transform.position = headPosition + new Vector3(randomDir2d.x * 4.0f, Random.Range(-0.5f, 0.5f), randomDir2d.y * 4.0f);
		towardsHead = true;
		setTarget(towardsHead);
	}

	private void FireBeam()
	{
		beamTimer = BeamTime;
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
		if (beamTimer > 0.0f)
		{
			Vector3 beamTargetPosition = Camera.main.transform.position + new Vector3(0.0f, Random.Range(-0.005f, 0.005f) - beamOffset, 0.0f);
			float distanceToBeamTarget = Vector3.Distance(beamTargetPosition, transform.position);

			beamTimer -= Time.deltaTime;
			if (beamTimer <= 0.0f || distanceToBeamTarget > 1.0f)
			{
				beamTimer = 0.0f;
				Beam.SetActive(false);
			}
			else
			{
				Beam.transform.rotation = Quaternion.LookRotation((beamTargetPosition - transform.position).normalized);
				Beam.transform.localScale = new Vector3(1.0f, 1.0f, distanceToBeamTarget);
				Beam.SetActive(true);
				PlayerManager.instance.DealDamage(Time.deltaTime * DamagePerSecond);
			}
		}

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
			if (towardsHead)
			{
				FireBeam();
			}
			towardsHead = !towardsHead;
			setTarget(towardsHead);
		}		

		Vector3 toTargetDir = toTarget.normalized;
		rb.AddForce(toTargetDir * ForceStrength);
	}
}
