using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
	public GuideWaveCreator GuideWaveCreator;
	public float OrderFraction;
	private Vector3 originalPosition;

	void OnTriggerEnter(Collider collider)
	{
		Debug.Log("Collision! " + collider.gameObject.layer);
		if (collider.gameObject.layer == LayerMask.NameToLayer("Controller"))
		{
			GuideWaveCreator.OnGuideCollected(this);
			Destroy(gameObject);
		}
	}

	void Start()
	{
		originalPosition = transform.position;
	}

	void Update()
	{
		transform.position = originalPosition + new Vector3(0.0f, Mathf.Sin(Time.time * (1.0f + OrderFraction* 0.1f - 0.05f) + OrderFraction), 0.0f) * 0.1f;
	}
}
