using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
	public bool IsRed;
	public float MaxDistanceToRibbon = 0.25f;
	public float RepulsionScale = 1.0f;
	public float RespawnTime = 5.0f;
	public GameObject MeshGO;

	private RibbonSolve ribbonSolver;
	private Rigidbody rb;
	private float respawnTimer;

	public void Restart()
	{
		respawnTimer = RespawnTime;
		Vector3 headPosition = Camera.main.transform.position;
		Vector2 randomDir2d = Random.insideUnitCircle.normalized;
		transform.position = headPosition + new Vector3(randomDir2d.x * 1.5f, Random.Range(-0.5f, 0.5f), randomDir2d.y * 1.5f);
		MeshGO.SetActive(false);
	}

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
	
	void Update()
	{
		if (respawnTimer > 0.0f)
		{
			respawnTimer -= Time.deltaTime;
			if (respawnTimer <= 0.0f)
			{
				respawnTimer = 0.0f;
				MeshGO.SetActive(true);
			}
		}
		else
		{
			Vector3 closestPosition;
			float distance = ribbonSolver.getClosestPoint(transform.position, out closestPosition);
			if (distance <= MaxDistanceToRibbon)
			{
				Vector3 repulsionForce = transform.position - closestPosition;
				rb.AddForce(repulsionForce * RepulsionScale, ForceMode.Impulse);
				PlayerManager.instance.AddScore(1);
				Restart();
			}
		}
	}
}
