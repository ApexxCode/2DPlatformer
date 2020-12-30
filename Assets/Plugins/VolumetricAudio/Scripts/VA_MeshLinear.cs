using UnityEngine;
using System.Collections.Generic;

public class VA_MeshLinear
{
	private List<VA_Triangle> triangles;

	public bool HasTriangles
	{
		get
		{
			return triangles != null && triangles.Count > 0;
		}
	}

	public void Clear()
	{
		if (triangles != null)
		{
			triangles.Clear();
		}
	}

	public void Update(Mesh mesh)
	{
		var triangleCount = 0;

		if (triangles == null)
		{
			triangles = new List<VA_Triangle>();
		}

		if (mesh != null)
		{
			var positions = mesh.vertices;

			for (var i = 0; i < mesh.subMeshCount; i++)
			{
				switch (mesh.GetTopology(i))
				{
					case MeshTopology.Triangles:
					{
						var indices = mesh.GetTriangles(i);

						for (var j = 0; j < indices.Length; j += 3)
						{
							var triangle = GetTriangle(triangleCount++);

							triangle.A = positions[indices[j + 0]];
							triangle.B = positions[indices[j + 1]];
							triangle.C = positions[indices[j + 2]];

							triangle.CalculatePlanes();
						}
					}
					break;
				}
			}
		}

		triangles.RemoveRange(triangleCount, triangles.Count - triangleCount);
	}

	public Vector3 FindClosestPoint(Vector3 point)
	{
		var closestPoint = point;

		if (triangles != null)
		{
			var closestDistance = float.PositiveInfinity;

			for (var i = triangles.Count - 1; i >= 0; i--)
			{
				var triangle      = triangles[i];
				var trianglePoint = triangle.ClosestTo(point);
				var distanceX     = trianglePoint.x - point.x;
				var distanceY     = trianglePoint.y - point.y;
				var distanceZ     = trianglePoint.z - point.z;
				var sqrDistance   = distanceX * distanceX + distanceY * distanceY + distanceZ * distanceZ; 

				if (sqrDistance < closestDistance)
				{
					closestDistance = sqrDistance;
					closestPoint    = trianglePoint;
				}
			}
		}

		return closestPoint;
	}

	private VA_Triangle GetTriangle(int triangleCount)
	{
		if (triangleCount == triangles.Count)
		{
			var triangle = new VA_Triangle();

			triangles.Add(triangle);

			return triangle;
		}

		return triangles[triangleCount];
	}
}