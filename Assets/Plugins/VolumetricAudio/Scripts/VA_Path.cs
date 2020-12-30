using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Path))]
public class VA_Path_Editor : VA_Editor<VA_Path>
{
	protected override void OnInspector()
	{
		serializedObject.Update();
		BeginError(Any(t => t.Points.Count < 2));
			DrawDefault("Points");
		EndError();
	}

	protected override void OnScene()
	{
		var matrix  = Target.transform.localToWorldMatrix;
		var inverse = Target.transform.worldToLocalMatrix;

		for (var i = 0; i < Target.Points.Count; i++)
		{
			var oldPoint = matrix.MultiplyPoint(Target.Points[i]);

			Handles.matrix = VA_Helper.TranslationMatrix(oldPoint) * VA_Helper.ScalingMatrix(0.8f) * VA_Helper.TranslationMatrix(oldPoint * -1.0f);

			var newPoint = Handles.PositionHandle(oldPoint, Quaternion.identity);

			if (oldPoint != newPoint)
			{
				Undo.RecordObject(Target, "Move Path Point");
				Target.Points[i] = inverse.MultiplyPoint(newPoint);

				EditorUtility.SetDirty(Target);
			}
		}

		Handles.color  = Color.red;
		Handles.matrix = Target.transform.localToWorldMatrix;

		for (var i = 1; i < Target.Points.Count; i++)
		{
			Handles.DrawLine(Target.Points[i - 1], Target.Points[i]);
		}

		Handles.BeginGUI();
		{
			for (var i = 0; i < Target.Points.Count; i++)
			{
				var point     = Target.Points[i];
				var pointName = "Point " + i;
				var scrPoint  = Camera.current.WorldToScreenPoint(matrix.MultiplyPoint(point));
				var rect      = new Rect(0.0f, 0.0f, 50.0f, 20.0f); rect.center = new Vector2(scrPoint.x, Screen.height - scrPoint.y - 35.0f);
				var rect1     = rect; rect.x += 1.0f;
				var rect2     = rect; rect.x -= 1.0f;
				var rect3     = rect; rect.y += 1.0f;
				var rect4     = rect; rect.y -= 1.0f;

				GUI.Label(rect1, pointName, EditorStyles.miniBoldLabel);
				GUI.Label(rect2, pointName, EditorStyles.miniBoldLabel);
				GUI.Label(rect3, pointName, EditorStyles.miniBoldLabel);
				GUI.Label(rect4, pointName, EditorStyles.miniBoldLabel);
				GUI.Label(rect, pointName, EditorStyles.whiteMiniLabel);
			}

			for (var i = 1; i < Target.Points.Count; i++)
			{
				var pointA   = Target.Points[i - 1];
				var pointB   = Target.Points[i];
				var midPoint = (pointA + pointB) * 0.5f;
				var scrPoint = Camera.current.WorldToScreenPoint(matrix.MultiplyPoint(midPoint));
				
				if (GUI.Button(new Rect(scrPoint.x - 5.0f, Screen.height - scrPoint.y - 45.0f, 20.0f, 20.0f), "+") == true)
				{
					Undo.RecordObject(Target, "Split Path");
					Target.Points.Insert(i, midPoint); GUI.changed = true;
				}
			}
		}
		Handles.EndGUI();
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Path")]
public class VA_Path : VA_Shape
{
	[Tooltip("The local space points for the path")]
	public List<Vector3> Points = new List<Vector3>();

	protected override void LateUpdate()
	{
		base.LateUpdate();

		// Make sure the listener exists
		var listenerPosition = default(Vector3);

		if (VA_Helper.GetListenerPosition(ref listenerPosition) == true)
		{
			if (Points.Count > 1)
			{
				var worldPoint        = listenerPosition;
				var localPoint        = transform.InverseTransformPoint(worldPoint);
				var closestDistanceSq = float.PositiveInfinity;
				var closestPoint      = Vector3.zero;

				for (var i = 1; i < Points.Count; i++)
				{
					var closePoint      = VA_Helper.ClosestPointToLineSegment(Points[i - 1], Points[i], localPoint);
					var closeDistanceSq = (closePoint - localPoint).sqrMagnitude;

					if (closeDistanceSq < closestDistanceSq)
					{
						closestDistanceSq = closeDistanceSq;
						closestPoint      = closePoint;
					}
				}

				worldPoint = transform.TransformPoint(closestPoint);

				SetOuterPoint(worldPoint);
			}
		}
	}
}