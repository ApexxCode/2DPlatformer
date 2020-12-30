using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_AudioListener))]
public class VA_AudioListener_Editor : VA_Editor<VA_AudioListener>
{
	protected override void OnInspector()
	{
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Audio Listener")]
public class VA_AudioListener : MonoBehaviour
{
	public static List<VA_AudioListener> Instances = new List<VA_AudioListener>();

	protected virtual void OnEnable()
	{
		Instances.Add(this);

		if (Instances.Count > 1)
		{
			Debug.LogWarning("Your scene already contains an active and enabled VA_AudioListener", Instances[0]);
		}
	}

	protected virtual void OnDisable()
	{
		Instances.Remove(this);
	}
}