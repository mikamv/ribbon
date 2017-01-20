using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
	public GuideWaveCreator GuideWaveCreator;
	public float OrderFraction;
	public int HandTargetId;
	private Vector3 originalPosition;

	void OnTriggerEnter(Collider collider)
	{
		RibbonController ribbonController = collider.gameObject.GetComponent<RibbonController>();
		if (ribbonController != null && ribbonController.HandId == HandTargetId && GuideWaveCreator.CanDestroyGuide(this) && collider.gameObject.layer == LayerMask.NameToLayer("Controller"))
		{
			GuideWaveCreator.OnGuideCollected(this);
			GameObject parent = gameObject;
			while (parent.transform.parent != null)
			{
				parent = parent.transform.parent.gameObject;
			}
			Destroy(parent);
		}
	}

	public void SetColor(Color color)
	{
		GetComponent<Renderer>().material.color = color;
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
