using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
	public GuideWaveCreator GuideWaveCreator;

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Controller"))
		{
			GuideWaveCreator.OnGuideCollected(this);
			Destroy(gameObject);
		}
	}

	void Update()
	{
		
	}
}
