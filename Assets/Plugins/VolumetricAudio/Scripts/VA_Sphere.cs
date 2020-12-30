using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Sphere))]
public class VA_Sphere_Editor : VA_Editor<VA_Sphere>
{
	protected override void OnInspector()
	{
		DrawDefault("SphereCollider");

		if (Any(t => t.SphereCollider == null))
		{
			DrawDefault("Center");
			DrawDefault("Radius");
		}

		DrawDefault("IsHollow");
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Sphere")]
public class VA_Sphere : VA_VolumetricShape
{
	[Tooltip("If you set this, then all shape settings will automatically be copied from the collider")]
	public SphereCollider SphereCollider;

	[Tooltip("The center of the sphere shape (if you set SphereCollider, this will be automatically overwritten)")]
	public Vector3 Center;

	[Tooltip("The radius of the sphere shape (if you set SphereCollider, this will be automatically overwritten)")]
	public float Radius = 1.0f;

	private Matrix4x4 cachedMatrix = Matrix4x4.identity;

	public void RebuildMatrix()
	{
		var position = transform.TransformPoint(Center);
		var rotation = transform.rotation;
		var scale    = transform.lossyScale;

		scale.x = scale.y = scale.z = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);

		VA_Helper.MatrixTrs(position, rotation, scale, ref cachedMatrix);
		//return VA_Helper.TranslationMatrix(position) * VA_Helper.RotationMatrix(rotation) * VA_Helper.ScalingMatrix(scale);
	}

	public override bool LocalPointInShape(Vector3 localPoint)
	{
		return LocalPointInSphere(localPoint);
	}

	protected virtual void Awake()
	{
		SphereCollider = GetComponent<SphereCollider>();
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

			if (IsHollow == true)
			{
				localPoint = SnapLocalPoint(localPoint);
				worldPoint = cachedMatrix.MultiplyPoint(localPoint);

				SetOuterPoint(worldPoint);
			}
			else
			{
				if (LocalPointInSphere(localPoint) == true)
				{
					SetInnerPoint(worldPoint, true);

					localPoint = SnapLocalPoint(localPoint);
					worldPoint = cachedMatrix.MultiplyPoint(localPoint);

					SetOuterPoint(worldPoint);
				}
				else
				{
					localPoint = SnapLocalPoint(localPoint);
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

			Gizmos.color  = Color.red;
			Gizmos.matrix = cachedMatrix;
			Gizmos.DrawWireSphere(Vector3.zero, Radius);
		}
	}
#endif

	private void UpdateFields()
	{
		if (SphereCollider != null)
		{
			Center = SphereCollider.center;
			Radius = SphereCollider.radius;
		}
	}

	private bool LocalPointInSphere(Vector3 localPoint)
	{
		return localPoint.sqrMagnitude < Radius * Radius;
	}

	private Vector3 SnapLocalPoint(Vector3 localPoint)
	{
		return localPoint.normalized * Radius;
	}
}