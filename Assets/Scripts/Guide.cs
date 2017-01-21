using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guide : MonoBehaviour
{
	public GuideWave GuideWave;
	public int HandTargetId;
	private Vector3 originalPosition;
	private Color baseColor;

	public Renderer[] Renderers;

	void OnTriggerEnter(Collider collider)
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
		originalPosition = transform.localPosition;
	}

	void Update()
	{
		transform.localPosition = originalPosition + new Vector3(0.0f, Mathf.Sin(Time.time * 3.0f), 0.0f) * 0.03f;
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
		setMaterialColor(new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f));
		transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
	}

	public void setNeutral()
	{
		setMaterialColor(new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f));
		transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
	}

	public void setActive()
	{
		setMaterialColor(new Color(baseColor.r, baseColor.g, baseColor.b, 1.0f));
		transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
	}
}
