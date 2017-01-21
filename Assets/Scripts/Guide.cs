using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
	public GuideWave GuideWave;
	public int HandTargetId;
	private Quaternion originalRotation;
	private Vector3 originalPosition;
	private Color baseColor;

	public float CollectTime = 1.0f;
	public float CollectEffectDistance = 0.2f;

	private float collectTimer;

	public Renderer[] Renderers;

	void OnTriggerEnter(Collider collider)
	{
		processCollision(collider);
	}

	void OnTriggerStay(Collider collider)
	{
		processCollision(collider);
	}

	void processCollision(Collider collider)
	{
		RibbonController ribbonController = collider.gameObject.GetComponent<RibbonController>();
		if (ribbonController != null && ribbonController.HandId == HandTargetId && collider.gameObject.layer == LayerMask.NameToLayer("Controller"))
		{
			GuideWave.OnGuideHit(this);
		}
	}

	public void SetColor(Color color)
	{
		baseColor = color;
	}

	void Start()
	{
		originalRotation = transform.localRotation;
		originalPosition = transform.localPosition;
	}

	public void Collect()
	{
		collectTimer = CollectTime;
	}

	void Update()
	{
		transform.localRotation = originalRotation * Quaternion.AngleAxis(Mathf.Sin(Time.time * 3.0f) * 20.0f, new Vector3(0.0f, 0.0f, 1.0f));

		if (collectTimer > 0.0f)
		{
			float t = collectTimer / CollectTime;
			transform.localPosition = originalPosition + new Vector3(0.0f, 1.0f, 0.0f) * (1 - t) * CollectEffectDistance;

			collectTimer -= Time.deltaTime;
			if (collectTimer <= 0.0f)
			{
				Destroy(gameObject);
			}
		}
	}

	private void setMaterialColor(Color color)
	{
		if (Renderers != null)
		{
			for (int i = 0; i < Renderers.Length; i++)
			{
				Renderers[i].material.color = color;
			}
		}
	}

	public void setDimmed()
	{
		setMaterialColor(new Color(baseColor.r, baseColor.g, baseColor.b, 0.05f));
	}

	public void setNeutral()
	{
		setMaterialColor(new Color(baseColor.r, baseColor.g, baseColor.b, 0.2f));
	}

	public void setActive()
	{
		setMaterialColor(new Color(baseColor.r, baseColor.g, baseColor.b, 1.0f));
	}
}
