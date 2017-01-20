using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
	public GuideWaveCreator GuideWaveCreator;

	void OnTriggerEnter(Collider collider)
	{
		Debug.Log("Collision! " + collider.gameObject.layer);
		if (collider.gameObject.layer == LayerMask.NameToLayer("Controller"))
		{
			GuideWaveCreator.OnGuideCollected(this);
			Destroy(gameObject);
		}
	}

	void Update()
	{
		
	}
}
