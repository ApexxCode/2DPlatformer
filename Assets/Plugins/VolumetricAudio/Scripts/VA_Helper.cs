using UnityEngine;

public static partial class VA_Helper
{
	public static int MeshVertexLimit = 65000;

	private static AudioListener cachedAudioListener;

	public static bool GetListenerPosition(ref Vector3 position)
	{
		if (VA_AudioListener.Instances.Count > 0)
		{
			position = VA_AudioListener.Instances[0].transform.position; return true;
		}

		if (Enabled(cachedAudioListener) == false)
		{
			var audioListeners = Object.FindObjectsOfType<AudioListener>();

			for (var i = audioListeners.Length - 1; i >= 0; i--)
			{
				var audioListener = audioListeners[i];

				if (audioListener.isActiveAndEnabled == true)
				{
					cachedAudioListener = audioListener; break;
				}
			}
		}

		if (Enabled(cachedAudioListener) == true)
		{
			position = cachedAudioListener.transform.position; return true;
		}

		return false;
	}

	// Return the current camera, or the main camera
	public static Camera GetCamera(Camera camera = null)
	{
		if (camera == null || camera.isActiveAndEnabled == false)
		{
			camera = Camera.main;
		}

		return camera;
	}

	public static Vector2 SinCos(float a)
	{
		return new Vector2(Mathf.Sin(a), Mathf.Cos(a));
	}

	public static void Destroy(Object o)
	{
#if UNITY_EDITOR
		if (Application.isPlaying == false)
		{
			Object.DestroyImmediate(o, true); return;
		}
#endif
		Object.Destroy(o);
	}

	public static bool Enabled(Behaviour b)
	{
		return b != null && b.isActiveAndEnabled == true;
	}

	public static float Divide(float a, float b)
	{
		return Zero(b) == false ? a / b : 0.0f;
	}

	public static float Reciprocal(float v)
	{
		return Zero(v) == false ? 1.0f / v : 0.0f;
	}

	public static bool Zero(float v)
	{
		return Mathf.Approximately(v, 0.0f);
	}

	public static void MatrixTrs(Vector3 t, Quaternion q, Vector3 s, ref Matrix4x4 m)
	{
		var num   = q.x * 2.0f;
		var num2  = q.y * 2.0f;
		var num3  = q.z * 2.0f;
		var num4  = q.x * num;
		var num5  = q.y * num2;
		var num6  = q.z * num3;
		var num7  = q.x * num2;
		var num8  = q.x * num3;
		var num9  = q.y * num3;
		var num10 = q.w * num;
		var num11 = q.w * num2;
		var num12 = q.w * num3;

		m.m00 = 1.0f - (num5 + num6);
		m.m10 = num7 + num12;
		m.m20 = num8 - num11;
		m.m30 = 0.0f;
		m.m01 = num7 - num12;
		m.m11 = 1.0f - (num4 + num6);
		m.m21 = num9 + num10;
		m.m31 = 0.0f;
		m.m02 = num8 + num11;
		m.m12 = num9 - num10;
		m.m22 = 1.0f - (num4 + num5);
		m.m32 = 0.0f;
		m.m03 = t.x;
		m.m13 = t.y;
		m.m23 = t.z;
		m.m33 = 1.0f;

		m.m00 *= s.x; m.m10 *= s.x; m.m20 *= s.x;
		m.m01 *= s.y; m.m11 *= s.y; m.m21 *= s.y;
		m.m02 *= s.z; m.m12 *= s.z; m.m22 *= s.z;
	}

	public static Matrix4x4 RotationMatrix(Quaternion q)
	{
		return Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
	}

	public static Matrix4x4 TranslationMatrix(Vector3 xyz)
	{
		return TranslationMatrix(xyz.x, xyz.y, xyz.z);
	}

	public static Matrix4x4 TranslationMatrix(float x, float y, float z)
	{
		var matrix = Matrix4x4.identity;

		matrix.m03 = x;
		matrix.m13 = y;
		matrix.m23 = z;

		return matrix;
	}

	public static Matrix4x4 ScalingMatrix(float xyz)
	{
		return ScalingMatrix(xyz, xyz, xyz);
	}

	public static Matrix4x4 ScalingMatrix(Vector3 xyz)
	{
		return ScalingMatrix(xyz.x, xyz.y, xyz.z);
	}

	public static Matrix4x4 ScalingMatrix(float x, float y, float z)
	{
		var matrix = Matrix4x4.identity;

		matrix.m00 = x;
		matrix.m11 = y;
		matrix.m22 = z;

		return matrix;
	}

	public static float DampenFactor(float dampening, float elapsed)
	{
#if UNITY_EDITOR
		if (Application.isPlaying == false)
		{
			return 1.0f;
		}
#endif
		return 1.0f - Mathf.Pow((float)System.Math.E, - dampening * elapsed);
	}

	public static Quaternion Dampen(Quaternion current, Quaternion target, float dampening, float elapsed, float minStep = 0.0f)
	{
		var factor   = DampenFactor(dampening, elapsed);
		var maxDelta = Quaternion.Angle(current, target) * factor + minStep * elapsed;

		return MoveTowards(current, target, maxDelta);
	}

	public static float Dampen(float current, float target, float dampening, float elapsed, float minStep = 0.0f)
	{
		var factor   = DampenFactor(dampening, elapsed);
		var maxDelta = Mathf.Abs(target - current) * factor + minStep * elapsed;

		return MoveTowards(current, target, maxDelta);
	}

	public static Vector3 Dampen3(Vector3 current, Vector3 target, float dampening, float elapsed, float minStep = 0.0f)
	{
		var factor   = DampenFactor(dampening, elapsed);
		var maxDelta = Mathf.Abs((target - current).magnitude) * factor + minStep * elapsed;

		return Vector3.MoveTowards(current, target, maxDelta);
	}

	public static Quaternion MoveTowards(Quaternion current, Quaternion target, float maxDelta)
	{
		var delta = Quaternion.Angle(current, target);

		return Quaternion.Slerp(current, target, Divide(maxDelta, delta));
	}

	public static float MoveTowards(float current, float target, float maxDelta)
	{
		if (target > current)
		{
			current = System.Math.Min(target, current + maxDelta);
		}
		else
		{
			current = System.Math.Max(target, current - maxDelta);
		}

		return current;
	}

	public static Vector3 ClosestPointToLineSegment(Vector3 a, Vector3 b, Vector3 point)
	{
		var l = (b - a).magnitude;
		var d = (b - a).normalized;

		return a + Mathf.Clamp(Vector3.Dot(point - a, d), 0.0f, l) * d;
	}

	public static bool PointLeftOfLine(Vector2 a, Vector2 b, Vector2 p) // NOTE: CCW
	{
		return ((b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y)) >= 0.0f;
	}

	public static bool PointLeftOfLine(float ax, float ay, float bx, float by, float px, float py) // NOTE: CCW
	{
		return ((bx - ax) * (py - ay) - (px - ax) * (by - ay)) >= 0.0f;
	}

	public static bool PointRightOfLine(Vector2 a, Vector2 b, Vector2 p) // NOTE: CCW
	{
		return ((b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y)) <= 0.0f;
	}

	public static Vector2 VectorXY(Vector3 xyz)
	{
		return new Vector2(xyz.x, xyz.y);
	}
}