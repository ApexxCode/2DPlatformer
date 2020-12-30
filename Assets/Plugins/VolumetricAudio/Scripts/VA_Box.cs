using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Box))]
public class VA_Box_Editor : VA_Editor<VA_Box>
{
	protected override void OnInspector()
	{
		DrawDefault("BoxCollider");

		if (Any(t => t.BoxCollider == null))
		{
			DrawDefault("Center");
			DrawDefault("Size");
		}

		DrawDefault("IsHollow");
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Box")]
public class VA_Box : VA_VolumetricShape
{
	[Tooltip("If you set this, then all shape settings will automatically be copied from the collider")]
	public BoxCollider BoxCollider;

	[Tooltip("The center of the box shape (if you set BoxCollider, this will be automatically overwritten)")]
	public Vector3 Center;

	[Tooltip("The size of the box shape (if you set BoxCollider, this will be automatically overwritten)")]
	public Vector3 Size = Vector3.one;

	private Matrix4x4 cachedMatrix = Matrix4x4.identity;

	public void RebuildMatrix()
	{
		var position = transform.TransformPoint(Center);
		var rotation = transform.rotation;
		var scale    = transform.lossyScale;

		scale.x *= Size.x;
		scale.y *= Size.y;
		scale.z *= Size.z;

		VA_Helper.MatrixTrs(position, rotation, scale, ref cachedMatrix);
		//return VA_Helper.TranslationMatrix(position) * VA_Helper.RotationMatrix(rotation) * VA_Helper.ScalingMatrix(scale);
	}

	public override bool LocalPointInShape(Vector3 localPoint)
	{
		return LocalPointInBox(localPoint);
	}

	protected virtual void Reset()
	{
		BoxCollider = GetComponent<BoxCollider>();
	}

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
				if (LocalPointInBox(localPoint) == true)
				{
					SetInnerPoint(worldPoint, true);

					localPoint = SnapLocalPoint(localPoint);
					worldPoint = cachedMatrix.MultiplyPoint(localPoint);

					SetOuterPoint(worldPoint);
				}
				else
				{
					localPoint = ClipLocalPoint(localPoint);
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
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
#endif

	private void UpdateFields()
	{
		if (BoxCollider != null)
		{
			Center = BoxCollider.center;
			Size   = BoxCollider.size;
		}
	}

	private bool LocalPointInBox(Vector3 localPoint)
	{
		if (localPoint.x < -0.5f) return false;
		if (localPoint.x >  0.5f) return false;

		if (localPoint.y < -0.5f) return false;
		if (localPoint.y >  0.5f) return false;

		if (localPoint.z < -0.5f) return false;
		if (localPoint.z >  0.5f) return false;

		return true;
	}

	private Vector3 SnapLocalPoint(Vector3 localPoint)
	{
		var x = Mathf.Abs(localPoint.x);
		var y = Mathf.Abs(localPoint.y);
		var z = Mathf.Abs(localPoint.z);

		// X largest?
		if (x > y && x > z)
		{
			localPoint *= VA_Helper.Reciprocal(x * 2.0f);
		}
		// Y largest?
		else if (y > x && y > z)
		{
			localPoint *= VA_Helper.Reciprocal(y * 2.0f);
		}
		// Z largest?
		else
		{
			localPoint *= VA_Helper.Reciprocal(z * 2.0f);
		}

		return localPoint;
	}

	private Vector3 ClipLocalPoint(Vector3 localPoint)
	{
		if (localPoint.x < -0.5f) localPoint.x = -0.5f;
		if (localPoint.x >  0.5f) localPoint.x =  0.5f;

		if (localPoint.y < -0.5f) localPoint.y = -0.5f;
		if (localPoint.y >  0.5f) localPoint.y =  0.5f;

		if (localPoint.z < -0.5f) localPoint.z = -0.5f;
		if (localPoint.z >  0.5f) localPoint.z =  0.5f;

		return localPoint;
	}
}