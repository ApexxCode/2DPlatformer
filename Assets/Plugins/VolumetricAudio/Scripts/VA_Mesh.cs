using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Mesh))]
public class VA_Mesh_Editor : VA_Editor<VA_Mesh>
{
	protected override void OnInspector()
	{
		BeginError(Any(t => t.MeshCollider == null && t.IsHollow == false));
			DrawDefault("MeshCollider");
		EndError();
		DrawDefault("MeshFilter");
		BeginError(Any(t => t.Mesh == null));
			DrawDefault("Mesh");
		EndError();

		if (Any(t => t.Mesh != null && t.Mesh.isReadable == false))
		{
			EditorGUILayout.HelpBox("This mesh is not readable.", MessageType.Error);
		}

		if (Any(t => t.Mesh != null && t.Mesh.vertexCount > 2000 && t.IsBaked == false))
		{
			EditorGUILayout.HelpBox("This mesh has a lot of vertices, so it may run slowly. If this mesh isn't dynamic then click Bake below.", MessageType.Warning);
		}

		DrawDefault("MeshUpdateInterval");

		DrawDefault("IsHollow");

		if (Any(t => t.IsHollow == false && t.MeshCollider == null))
		{
			EditorGUILayout.HelpBox("Non hollow meshes require a MeshCollider to be set.", MessageType.Error);
		}

		if (Any(t => t.IsHollow == false))
		{
			BeginError(Any(t => t.RaySeparation <= 0.0f));
				DrawDefault("RaySeparation");
			EndError();
		}

		Separator();

		if (Any(t => t.Mesh != null))
		{
			if (Button("Bake Mesh") == true)
			{
				DirtyEach(t => t.Bake());
			}
		}

		if (Any(t => t.IsBaked == true))
		{
			if (Button("Clear Baked Mesh") == true)
			{
				DirtyEach(t => t.ClearBake());
			}

			EditorGUILayout.HelpBox("This mesh has been baked for faster computation. If your mesh has been modified then press 'Bake Mesh' again.", MessageType.Info);
		}
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Mesh")]
public class VA_Mesh : VA_VolumetricShape
{
	[Tooltip("If you set this, then all shape settings will automatically be copied from the collider")]
	public MeshCollider MeshCollider;

	[Tooltip("If you set this, then all shape settings will automatically be copied from the collider")]
	public MeshFilter MeshFilter;

	[Tooltip("The mesh of the mesh shape (if you set MeshCollider or MeshFilter, this will be automatically overwritten)")]
	public Mesh Mesh;

	[Tooltip("The interval between each mesh update in seconds (-1 = no updates)")]
	public float MeshUpdateInterval = -1.0f;

	[Tooltip("How far apart each volume checking ray should be separated to avoid miscalculations. This value should be based on the size of your mesh, but be kept quite low")]
	public float RaySeparation = 0.1f;

	[SerializeField]
	private VA_MeshTree tree;

	[System.NonSerialized]
	private VA_MeshLinear linear;

	[System.NonSerialized]
	private float meshUpdateCooldown;

	public bool IsBaked
	{
		get
		{
			return tree != null && tree.Nodes != null && tree.Nodes.Count > 0;
		}
	}

	public void ClearBake()
	{
		if (tree != null)
		{
			tree.Clear();
		}
	}

	public void Bake()
	{
		if (tree == null) tree = new VA_MeshTree();

		tree.Update(Mesh);

		if (linear != null)
		{
			linear.Clear();
		}
	}

	public override bool LocalPointInShape(Vector3 localPoint)
	{
		var worldPoint = transform.TransformPoint(localPoint);

		return PointInMesh(localPoint, worldPoint);
	}

	protected virtual void Reset()
	{
		IsHollow     = true; // NOTE: This is left as true by default to prevent applying volume to meshes with holes
		MeshCollider = GetComponent<MeshCollider>();
		MeshFilter   = GetComponent<MeshFilter>();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();

		// Update mesh?
		if (meshUpdateCooldown > MeshUpdateInterval)
		{
			meshUpdateCooldown = MeshUpdateInterval;
		}

		if (MeshUpdateInterval >= 0.0f)
		{
			meshUpdateCooldown -= Time.deltaTime;

			if (meshUpdateCooldown <= 0.0f)
			{
				meshUpdateCooldown = MeshUpdateInterval;

				if (IsBaked == true)
				{
					if (tree != null)
					{
						tree.Update(Mesh);
					}
				}
				else
				{
					if (linear != null)
					{
						linear.Update(Mesh);
					}
				}
			}
		}

		// Make sure the listener exists
		var listenerPosition = default(Vector3);

		if (VA_Helper.GetListenerPosition(ref listenerPosition) == true)
		{
			UpdateFields();

			var worldPoint = listenerPosition;
			var localPoint = transform.InverseTransformPoint(worldPoint);

			if (Mesh != null)
			{
				if (IsHollow == true)
				{
					localPoint = SnapLocalPoint(localPoint);
					worldPoint = transform.TransformPoint(localPoint);

					SetOuterPoint(worldPoint);
				}
				else
				{
					if (PointInMesh(localPoint, worldPoint) == true)
					{
						SetInnerPoint(worldPoint, true);

						localPoint = SnapLocalPoint(localPoint);
						worldPoint = transform.TransformPoint(localPoint);

						SetOuterPoint(worldPoint);
					}
					else
					{
						localPoint = SnapLocalPoint(localPoint);
						worldPoint = transform.TransformPoint(localPoint);

						SetInnerOuterPoint(worldPoint, false);
					}
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

			Gizmos.color  = Color.red;
			Gizmos.matrix = transform.localToWorldMatrix;

			if (IsBaked == true)
			{
				for (var i = tree.Triangles.Count - 1; i >= 0; i--)
				{
					var triangle = tree.Triangles[i];

					Gizmos.DrawLine(triangle.A, triangle.B);
					Gizmos.DrawLine(triangle.B, triangle.C);
					Gizmos.DrawLine(triangle.C, triangle.A);
				}
			}
			else
			{
				if (Mesh != null)
				{
					var positions = Mesh.vertices;

					for (var i = 0; i < Mesh.subMeshCount; i++)
					{
						switch (Mesh.GetTopology(i))
						{
							case MeshTopology.Triangles:
							{
								var indices = Mesh.GetTriangles(i);

								for (var j = 0; j < indices.Length; j += 3)
								{
									var point1 = positions[indices[j + 0]];
									var point2 = positions[indices[j + 1]];
									var point3 = positions[indices[j + 2]];

									Gizmos.DrawLine(point1, point2);
									Gizmos.DrawLine(point2, point3);
									Gizmos.DrawLine(point3, point1);
								}
							}
							break;
						}
					}
				}
			}
		}
	}
#endif

	private Vector3 FindClosestLocalPoint(Vector3 localPoint)
	{
		// Tree search?
		if (IsBaked == true)
		{
			return tree.FindClosestPoint(localPoint);
		}
		// Linear search?
		else
		{
			if (linear == null)
			{
				linear = new VA_MeshLinear();
			}

			if (linear.HasTriangles == false)
			{
				linear.Update(Mesh);
			}

			return linear.FindClosestPoint(localPoint);
		}
	}

	private void UpdateFields()
	{
		if (MeshCollider != null)
		{
			Mesh = MeshCollider.sharedMesh;
		}
		else if (MeshFilter != null)
		{
			Mesh = MeshFilter.sharedMesh;
		}
	}

	private int RaycastHitCount(Vector3 origin, Vector3 direction, float separation)
	{
		var hitCount = 0;

		if (MeshCollider != null && separation > 0.0f)
		{
			var meshSize = Vector3.Magnitude(MeshCollider.bounds.size);
			var lengthA  = meshSize;
			var lengthB  = meshSize;
			var rayA     = new Ray(origin, direction);
			var rayB     = new Ray(origin + direction * meshSize, -direction);
			var hit      = default(RaycastHit);

			for (var i = 0; i < 50; i++)
			{
				if (MeshCollider.Raycast(rayA, out hit, lengthA) == true)
				{
					lengthA -= hit.distance + separation;

					rayA.origin = hit.point + rayA.direction * separation; hitCount += 1;
				}
				else
				{
					break;
				}
			}

			for (var i = 0; i < 50; i++)
			{
				if (MeshCollider.Raycast(rayB, out hit, lengthB) == true)
				{
					lengthB -= hit.distance + separation;

					rayB.origin = hit.point + rayB.direction * separation; hitCount += 1;
				}
				else
				{
					break;
				}
			}
		}

		return hitCount;
	}

	private bool PointInMesh(Vector3 localPoint, Vector3 worldPoint)
	{
		if (Mesh.bounds.Contains(localPoint) == false) return false;

		var hitCount = RaycastHitCount(worldPoint, Vector3.up, RaySeparation);

		if (hitCount == 0 || hitCount % 2 == 0) return false;

		return true;
	}

	private Vector3 SnapLocalPoint(Vector3 localPoint)
	{
		return FindClosestLocalPoint(localPoint);
	}
}