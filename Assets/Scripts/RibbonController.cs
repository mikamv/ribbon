using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RibbonController : MonoBehaviour
{
	public int HandId;

	public Vector3 getPosition()
	{
		return transform.position;
	}

	public void SetColor(Color color)
	{
		GetComponent<Renderer>().material.color = color;
	}
}
