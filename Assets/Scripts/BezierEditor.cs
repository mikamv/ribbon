using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Bezier))]
public class BezierEditor : Editor
{
	private Bezier curve;
	private Transform handleTransform;
	private Quaternion handleRotation;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		curve = target as Bezier;
		if (GUILayout.Button("Add point"))
		{
			curve.addPoints();
		}
	}

	private void OnSceneGUI()
	{
		curve = target as Bezier;
		if (curve != null)
		{
			curve.init();
		}
		handleTransform = curve.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		Handles.color = Color.white;
		for (int i = 0; i < curve.points.Count - 1; i++)
		{
			Vector3 a = ShowPoint(i);
			Vector3 b = ShowPoint(i + 1);
			Handles.DrawLine(a, b);
		}

		int curveCount = curve.getCurveCount();
		int numSegments = 32 * curveCount;
		float step = (float)curveCount / numSegments;
		for (int i = 0; i < numSegments; i++)
		{
			float t = Mathf.Clamp((float)i / (numSegments / curveCount), 0, curveCount);
			Vector3 a = curve.getWorldPosition(t);
			Vector3 b = curve.getWorldPosition(t + step);

			Handles.color = Color.green;
			Handles.DrawLine(a, a + curve.getTangent(t) * 0.1f);

			Handles.color = Color.magenta;
			Handles.DrawLine(a, b);
		}
	}

	private Vector3 ShowPoint(int index)
	{
		Vector3 point = handleTransform.TransformPoint(curve.points[index]);
		EditorGUI.BeginChangeCheck();
		point = Handles.DoPositionHandle(point, handleRotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(curve, "Move Point");
			EditorUtility.SetDirty(curve);
			curve.points[index] = handleTransform.InverseTransformPoint(point);
		}
		return point;
	}
}