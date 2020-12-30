using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Material))]
public class VA_Material_Editor : VA_Editor<VA_Material>
{
	protected override void OnInspector()
	{
		DrawDefault("OcclusionVolume");
	}
}
#endif

public class VA_Material : MonoBehaviour
{
	[Tooltip("The volume multiplier when this material is blocking the VA_AudioSource")]
	[Range(0.0f, 1.0f)]
	public float OcclusionVolume = 0.1f;
}