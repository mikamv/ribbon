using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{
	public Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		points.Clear();
		points.Add(p0);
		points.Add(p1);
		points.Add(p2);
		points.Add(p3);
	}

	public void init()
	{
		while (points.Count < 4)
		{
			points.Add(new Vector3());
		}
	}

	public void addPoints()
	{
		if (points.Count < 4)
		{
			init();
			return;
		}
		Vector3 endTangent = getTangent(1.0f);
		Vector3 last = points[points.Count - 1];
		Vector3 first = points[points.Count - 4];
		points.Add(last + endTangent * 0.35f);
		points.Add(last + (last - first));
		points.Add((points[points.Count - 1] + points[points.Count - 2]) * 0.5f);
	}

	public int getCurveCount()
	{
		return (points.Count - 1) / 3;
	}

	public Vector3 getLocalPosition(float inT)
	{
		int startIndex = (int)inT * 3;
		float t = inT - (int)inT;
		if (startIndex == points.Count - 1)
		{
			startIndex -= 3;
			t += 1.0f;
		}

		float u = 1.0f - t;
		float tSqr = t * t;
		float uSqr = u * u;
		float uQubic = uSqr * u;
		float tQubic = tSqr * t;

		return uQubic * points[startIndex] + 3 * uSqr * t * points[startIndex + 1] + 3 * u * tSqr * points[startIndex + 2] + tQubic * points[startIndex + 3];
	}

	public Vector3 getWorldPosition(float t)
	{
		return transform.TransformPoint(getLocalPosition(t));
	}

	public Vector3 getTangent(float inT)
	{
		int startIndex = (int)inT * 3;
		float t = inT - (int)inT;
		if (startIndex == points.Count - 1)
		{
			startIndex -= 3;
			t += 1.0f;
		}

		float u = 1.0f - t;
		float tSqr = t * t;
		float uSqr = u * u;

		Vector3 tangent = -3 * uSqr * points[startIndex] + 3 * uSqr * points[startIndex + 1] - 6 * t * u * points[startIndex + 1] - 3 * tSqr * points[startIndex + 2] + 6 * t * u * points[startIndex + 2] + 3 * tSqr * points[startIndex + 3];
		return tangent.normalized;
	}

	public LinePath getPath(int numPoints)
	{
		LinePath linePath = new LinePath();
		int curveCount = getCurveCount();
		for (int i = 0; i < numPoints; i++)
		{
			float t = Mathf.Clamp((float)i / (numPoints / curveCount), 0, curveCount);
			linePath.addPoint(getWorldPosition(t));
		}
		linePath.StartTracing();
		return linePath;
	}

	public List<Vector3> points = new List<Vector3>();
}
