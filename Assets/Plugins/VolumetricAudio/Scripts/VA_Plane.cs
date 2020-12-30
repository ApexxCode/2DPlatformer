using UnityEngine;

// UnityEngine.Plane doesn't serialize, so re-create it with what we need here
[System.Serializable]
public class VA_Plane
{
	public Vector3 Normal;
	public float   Distance;

	public VA_Plane(Vector3 a, Vector3 b, Vector3 c)
	{
		Normal   = Vector3.Normalize(Vector3.Cross(b - a, c - a));
		Distance = -Vector3.Dot(Normal, a);
	}

	public bool SideOf(Vector3 p)
	{
		//return Vector3.Dot(Normal, p) + Distance > 0.0f;
		return Normal.x * p.x + Normal.y * p.y + Normal.z * p.z + Distance > 0.0f;
	}

	public float DistanceTo(Vector3 p)
	{
		return Vector3.Dot(Normal, p) + Distance;
	}

	public Vector3 ClosestTo(Vector3 p)
	{
		return p - Normal * (Vector3.Dot(Normal, p) + Distance);
	}
}