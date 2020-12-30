using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VA_MeshTree
{
	[System.Serializable]
	public class Node
	{
		public Bounds Bound;

		public int PositiveIndex;
		public int NegativeIndex;

		public int TriangleIndex;
		public int TriangleCount;
	}

	public List<Node> Nodes = new List<Node>();

	public List<VA_Triangle> Triangles = new List<VA_Triangle>();

	// Cached search values
	private static List<VA_Triangle> searchResults = new List<VA_Triangle>();
	private static float             searchRange;
	private static float             searchMaximum;
	private static Vector3           searchPoint;

	public void Clear()
	{
		Nodes.Clear();
		
		Triangles.Clear();
	}

	public void Update(Mesh mesh)
	{
		Clear();

		if (mesh != null)
		{
			var rootNode = new Node(); Nodes.Add(rootNode);
			var tris     = GetAllTriangles(mesh);

			Pack(rootNode, tris);
		}
	}

	public Vector3 FindClosestPoint(Vector3 point)
	{
		if (Nodes.Count > 0)
		{
			searchResults.Clear();

			// Set search data and begin from root node
			searchRange = float.PositiveInfinity;
			searchPoint = point;

			Search(Nodes[0]);

			// Loop through search results and find the closest
			var closestDistance = float.PositiveInfinity;
			var closestPoint    = point;

			for (var i = searchResults.Count - 1; i >= 0; i--)
			{
				var triangle      = searchResults[i];
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

			return closestPoint;
		}

		return point;
	}

	private void Search(Node node)
	{
		// If this node is a leaf, add all triangles and return
		if (node.TriangleCount > 0)
		{
			AddToResults(node); return;
		}

		var bound = node.Bound;

		// If this node's nearest point distance is more than the search radius then reject
		var nodeNear = bound.SqrDistance(searchPoint);

		if (nodeNear - 0.001f > searchRange)
		{
			return;
		}

		// If this node's farthest point distance is less than the search radius then replace the search distance
		var nodeFar = MaximumDistance(bound.min, bound.max);

		if (nodeFar + 0.001f < searchRange)
		{
			searchRange = nodeFar + 0.01f;
		}

		// Search children
		if (node.PositiveIndex != 0) Search(Nodes[node.PositiveIndex]);
		if (node.NegativeIndex != 0) Search(Nodes[node.NegativeIndex]);
	}

	// Compute the sqrDistance to each node corner and return the highest
	private float MaximumDistance(Vector3 min, Vector3 max)
	{
		searchMaximum = 0.0f;

		FarSqrDistance(min.x, min.y, min.z);
		FarSqrDistance(max.x, min.y, min.z);
		FarSqrDistance(min.x, min.y, max.z);
		FarSqrDistance(max.x, min.y, max.z);

		FarSqrDistance(min.x, max.y, min.z);
		FarSqrDistance(max.x, max.y, min.z);
		FarSqrDistance(min.x, max.y, max.z);
		FarSqrDistance(max.x, max.y, max.z);

		return searchMaximum;
	}

	private static void FarSqrDistance(float x, float y, float z)
	{
		x -= searchPoint.x;
		y -= searchPoint.y;
		z -= searchPoint.z;

		var sqr = x * x + y * y + z * z;

		if (sqr > searchMaximum)
		{
			searchMaximum = sqr;
		}
	}

	private void AddToResults(Node node)
	{
		for (var i = node.TriangleIndex; i < node.TriangleIndex + node.TriangleCount; i++)
		{
			searchResults.Add(Triangles[i]);
		}
	}

	private List<VA_Triangle> GetAllTriangles(Mesh mesh)
	{
		var tris = new List<VA_Triangle>();
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
						var triangle = new VA_Triangle(); tris.Add(triangle);

						triangle.A = positions[indices[j + 0]];
						triangle.B = positions[indices[j + 1]];
						triangle.C = positions[indices[j + 2]];
						triangle.CalculatePlanes();
					}
				}
				break;
			}
		}

		return tris;
	}

	private void Pack(Node node, List<VA_Triangle> tris)
	{
		CalculateBound(node, tris);

		if (tris.Count < 5)
		{
			node.TriangleIndex = Triangles.Count;
			node.TriangleCount = tris.Count;

			Triangles.AddRange(tris);
		}
		else
		{
			var positiveTris = new List<VA_Triangle>();
			var negativeTris = new List<VA_Triangle>();
			var axis         = default(int);
			var pivot        = default(float);

			CalculateAxisAndPivot(tris, ref axis, ref pivot);

			// Switch axis
			switch (axis)
			{
				case 0:
				{
					foreach (var triangle in tris)
					{
						if (triangle.MidX >= pivot) positiveTris.Add(triangle); else negativeTris.Add(triangle);
					}
				}
				break;

				case 1:
				{
					foreach (var triangle in tris)
					{
						if (triangle.MidY >= pivot) positiveTris.Add(triangle); else negativeTris.Add(triangle);
					}
				}
				break;

				case 2:
				{
					foreach (var triangle in tris)
					{
						if (triangle.MidZ >= pivot) positiveTris.Add(triangle); else negativeTris.Add(triangle);
					}
				}
				break;
			}

			// Overlapping triangles?
			if (positiveTris.Count == 0 || negativeTris.Count == 0)
			{
				positiveTris.Clear();
				negativeTris.Clear();

				var split = tris.Count / 2;

				for (var i = 0; i < split; i++)
				{
					positiveTris.Add(tris[i]);
				}

				for (var i = split; i < tris.Count; i++)
				{
					negativeTris.Add(tris[i]);
				}
			}

			node.PositiveIndex = Nodes.Count; var positiveNode = new Node(); Nodes.Add(positiveNode); Pack(positiveNode, positiveTris);
			node.NegativeIndex = Nodes.Count; var negativeNode = new Node(); Nodes.Add(negativeNode); Pack(negativeNode, negativeTris);
		}
	}

	private void CalculateBound(Node node, List<VA_Triangle> tris)
	{
		if (tris.Count > 0)
		{
			var min = tris[0].Min;
			var max = tris[0].Max;

			foreach (var tri in tris)
			{
				min = Vector3.Min(min, tri.Min);
				max = Vector3.Max(max, tri.Max);
			}

			node.Bound.SetMinMax(min, max);
		}
	}

	private void CalculateAxisAndPivot(List<VA_Triangle> tris, ref int axis, ref float pivot)
	{
		var min = tris[0].Min;
		var max = tris[0].Max;
		var mid = Vector3.zero;

		foreach (var tri in tris)
		{
			min  = Vector3.Min(min, tri.Min);
			max  = Vector3.Max(max, tri.Max);
			mid += tri.A + tri.B + tri.C;
		}

		var size = max - min;

		if (size.x > size.y && size.x > size.z)
		{
			axis  = 0;
			pivot = VA_Helper.Divide(mid.x, tris.Count * 3.0f);
		}
		else if (size.y > size.x && size.y > size.z)
		{
			axis  = 1;
			pivot = VA_Helper.Divide(mid.y, tris.Count * 3.0f);
		}
		else
		{
			axis  = 2;
			pivot = VA_Helper.Divide(mid.z, tris.Count * 3.0f);
		}
	}
}