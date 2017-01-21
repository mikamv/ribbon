using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePath
{
	public void addPoint(Vector3 position)
	{
		positions.Add(position);
		if (positions.Count > 1)
		{
			pathLength += (positions[positions.Count - 1] - positions[positions.Count - 2]).magnitude;
		}
		distances.Add(pathLength);
	}

	public float Length
	{
		get
		{
			return pathLength;
		}
	}

	public void StartTracing()
	{
		if (positions.Count > 0)
		{
			currentPosition = positions[0];
			if (positions.Count > 1)
			{
				currentDirection = (positions[1] - positions[0]).normalized;
			}
			currentDistance = 0.0f;
			currentIndex = 0;
		}
	}

	public Vector3 Position
	{
		get
		{
			return currentPosition;
		}
	}

	public Vector3 Direction
	{
		get
		{
			return currentDirection;
		}
	}

	public void Advance(float distance)
	{
		currentDistance = Mathf.Clamp(currentDistance + distance, 0, pathLength);
		while (currentIndex < positions.Count)
		{
			if (currentIndex > 0 && distances[currentIndex] >= currentDistance)
			{
				float segmentLength = distances[currentIndex] - distances[currentIndex - 1];
				float distanceAtSegment = currentDistance - distances[currentIndex - 1];
				float segmentFraction = distanceAtSegment / segmentLength;
				currentPosition = Vector3.Lerp(positions[currentIndex - 1], positions[currentIndex], segmentFraction);
				currentDirection = (positions[currentIndex] - positions[currentIndex - 1]).normalized;
				return;
			}
			currentIndex++;
		}

		if (positions.Count > 0)
		{
			currentPosition = positions[positions.Count - 1];
			if (positions.Count > 1)
			{
				currentDirection = (positions[1] - positions[0]).normalized;
			}
		}
	}

	private float pathLength;
	private float currentDistance;
	private int currentIndex;
	private Vector3 currentPosition;
	private Vector3 currentDirection;
	private List<Vector3> positions = new List<Vector3>();
	private List<float> distances = new List<float>();
}
