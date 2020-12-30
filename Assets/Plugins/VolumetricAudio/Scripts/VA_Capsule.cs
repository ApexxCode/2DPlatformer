using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Capsule))]
public class VA_Capsule_Editor : VA_Editor<VA_Capsule>
{
	protected override void OnInspector()
	{
		DrawDefault("CapsuleCollider");

		if (Any(t => t.CapsuleCollider == null))
		{
			DrawDefault("Center");
			DrawDefault("Radius");
			DrawDefault("Height");
			DrawDefault("Direction");
		}

		DrawDefault("IsHollow");
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Capsule")]
public class VA_Capsule : VA_VolumetricShape
{
	[Tooltip("If you set this, then all shape settings will automatically be copied from the collider")]
	public CapsuleCollider CapsuleCollider;

	[Tooltip("The center of the capsule shape (if you set CapsuleCollider, this will be automatically overwritten)")]
	public Vector3 Center;

	[Tooltip("The radius of the capsule shape (if you set CapsuleCollider, this will be automatically overwritten)")]
	public float Radius = 1.0f;

	[Tooltip("The height of the capsule shape (if you set CapsuleCollider, this will be automatically overwritten)")]
	public float Height = 2.0f;

	[Tooltip("The direction of the capsule shape (if you set CapsuleCollider, this will be automatically overwritten)")]
	[VA_PopupAttribute("X-Axis", "Y-Axis", "Z-Axis")]
	public int Direction = 1;

	private static Quaternion RotationX = Quaternion.Euler(0.0f, 0.0f, 90.0f);

	private static Quaternion RotationY = Quaternion.identity;

	private static Quaternion RotationZ = Quaternion.Euler(90.0f, 0.0f, 0.0f);

	private Matrix4x4 cachedMatrix = Matrix4x4.identity;

	public void RebuildMatrix()
	{
		var position = transform.TransformPoint(Center);
		var rotation = transform.rotation;
		var scale    = transform.lossyScale;

		switch (Direction)
		{
			case 0: rotation *= RotationX; break;
			case 1: rotation *= RotationY; break;
			case 2: rotation *= RotationZ; break;
		}

		scale.x = scale.y = scale.z = Mathf.Max(scale.x, scale.z);

		VA_Helper.MatrixTrs(position, rotation, scale, ref cachedMatrix);
		//matrix = VA_Helper.TranslationMatrix(position) * VA_Helper.RotationMatrix(rotation) * matrix * VA_Helper.ScalingMatrix(scale);
	}

	public override bool LocalPointInShape(Vector3 localPoint)
	{
		var halfHeight = Mathf.Max(0.0f, Height * 0.5f - Radius);

		return LocalPointInCapsule(localPoint, halfHeight);
	}

	protected virtual void Awake()
	{
		CapsuleCollider = GetComponent<CapsuleCollider>();
	}

#if UNITY_EDITOR
	protected virtual void Reset()
	{
		Awake();
	}
#endif

	protected override void LateUpdate()
	{
		base.LateUpdate();

		// Make sure the listener exists
		var listenerPosition = default(Vector3);

		if (VA_Helper.GetListenerPosition(ref listenerPosition) == true)
		{
			UpdateFields();
			RebuildMatrix();

			var worldPoint = listenerPosition;
			var localPoint = cachedMatrix.inverse.MultiplyPoint(worldPoint);
			var scale      = transform.lossyScale;
			var squash     = VA_Helper.Divide(scale.y, Mathf.Max(scale.x, scale.z));
			var halfHeight = Mathf.Max(0.0f, Height * squash * 0.5f - Radius);

			if (IsHollow == true)
			{
				localPoint = SnapLocalPoint(localPoint, halfHeight);
				worldPoint = cachedMatrix.MultiplyPoint(localPoint);

				SetOuterPoint(worldPoint);
			}
			else
			{
				if (LocalPointInCapsule(localPoint, halfHeight) == true)
				{
					SetInnerPoint(worldPoint, true);

					localPoint = SnapLocalPoint(localPoint, halfHeight);
					worldPoint = cachedMatrix.MultiplyPoint(localPoint);

					SetOuterPoint(worldPoint);
				}
				else
				{
					localPoint = SnapLocalPoint(localPoint, halfHeight);
					worldPoint = cachedMatrix.MultiplyPoint(localPoint);

					SetInnerOuterPoint(worldPoint, false);
				}
			}
		}
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		if (VA_Helper.Enabled(this) == true)
		{
			UpdateFields();
			RebuildMatrix();

			var scale      = transform.lossyScale;
			var squash     = VA_Helper.Divide(scale.y, Mathf.Max(scale.x, scale.z));
			var halfHeight = Mathf.Max(0.0f, Height * squash * 0.5f - Radius);
			var point1     = Vector3.up *  halfHeight;
			var point2     = Vector3.up * -halfHeight;

			Gizmos.color  = Color.red;
			Gizmos.matrix = cachedMatrix;
			Gizmos.DrawWireSphere(point1, Radius);
			Gizmos.DrawWireSphere(point2, Radius);
			Gizmos.DrawLine(point1 + Vector3.right   * Radius, point2 + Vector3.right   * Radius);
			Gizmos.DrawLine(point1 - Vector3.right   * Radius, point2 - Vector3.right   * Radius);
			Gizmos.DrawLine(point1 + Vector3.forward * Radius, point2 + Vector3.forward * Radius);
			Gizmos.DrawLine(point1 - Vector3.forward * Radius, point2 - Vector3.forward * Radius);
		}
	}
#endif

	private void UpdateFields()
	{
		if (CapsuleCollider != null)
		{
			Center    = CapsuleCollider.center;
			Radius    = CapsuleCollider.radius;
			Height    = CapsuleCollider.height;
			Direction = CapsuleCollider.direction;
		}
	}

	private bool LocalPointInCapsule(Vector3 localPoint, float halfHeight)
	{
		// Top
		if (localPoint.y > halfHeight)
		{
			localPoint.y -= halfHeight;

			return localPoint.sqrMagnitude < Radius * Radius;
		}
		// Bottom
		else if (localPoint.y < -halfHeight)
		{
			localPoint.y += halfHeight;

			return localPoint.sqrMagnitude < Radius * Radius;
		}
		// Middle
		else
		{
			localPoint.y = 0.0f;

			return localPoint.sqrMagnitude < Radius * Radius;
		}
	}

	private Vector3 SnapLocalPoint(Vector3 localPoint, float halfHeight)
	{
		// Top
		if (localPoint.y > halfHeight)
		{
			localPoint.y -= halfHeight;

			localPoint = localPoint.normalized * Radius;
			localPoint.y += halfHeight;
		}
		// Bottom
		else if (localPoint.y < -halfHeight)
		{
			localPoint.y += halfHeight;

			localPoint = localPoint.normalized * Radius;
			localPoint.y -= halfHeight;
		}
		// Middle
		else
		{
			var oldY = localPoint.y; localPoint.y = 0.0f;

			localPoint = localPoint.normalized * Radius;
			localPoint.y = oldY;
		}

		return localPoint;
	}
}