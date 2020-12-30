using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_Zone))]
public class VA_Zone_Editor : VA_Editor<VA_Zone>
{
	protected override void OnInspector()
	{
		BeginError(Any(t => t.Radius <= 0.0f));
			DrawDefault("Radius");
		EndError();
		DrawDefault("DeactivateGameObjects");
		BeginError(Any(t => t.VolumeDampening <= 0.0f));
			DrawDefault("VolumeDampening");
		EndError();
		BeginError(Any(t => t.AudioSources == null || t.AudioSources.Count == 0 || t.AudioSources.Exists(s => s == null)));
			DrawDefault("AudioSources");
		EndError();

		if (Any(t => t.AudioSources != null && t.AudioSources.Exists(a => a != null && a.Volume == false) == true))
		{
			EditorGUILayout.HelpBox("At least one of these audio sources has its 'Volume' setting disabled. This means the volume cannot transition in/out.", MessageType.Warning);
		}
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Zone")]
public class VA_Zone : MonoBehaviour
{
	[Tooltip("The radius of this zone")]
	public float Radius = 1.0f;

	[Tooltip("Should the ")]
	public bool DeactivateGameObjects;

	[Tooltip("The speed at which the volume changes")]
	public float VolumeDampening = 10.0f;

	// Current volume of the zone
	// This transitions to 1 when inside the zone, and 0 when outside
	public float Volume;

	[Tooltip("The audio sources this zone is associated with")]
	public List<VA_AudioSource> AudioSources;

	protected virtual void Update()
	{
		// Make sure the listener exists
		var listenerPosition = default(Vector3);

		if (VA_Helper.GetListenerPosition(ref listenerPosition) == true)
		{
			// Calculate the target volume
			var targetVolume = 0.0f;

			if (Vector3.Distance(listenerPosition, transform.position) <= Radius)
			{
				targetVolume = 1.0f;
			}

			// Dampen volume to the target value
			Volume = VA_Helper.Dampen(Volume, targetVolume, VolumeDampening, Time.deltaTime, 0.1f);

			// Loop through all audio sources
			if (AudioSources != null)
			{
				for (var i = AudioSources.Count - 1; i >= 0; i--)
				{
					var audioSource = AudioSources[i];

					if (audioSource != null)
					{
						// Apply the zone so this volume can be set
						audioSource.Zone = this;

						// Enable volumes?
						if (Volume > 0.0f)
						{
							if (audioSource.gameObject.activeSelf == false)
							{
								audioSource.gameObject.SetActive(true);
							}

							if (audioSource.enabled == false)
							{
								audioSource.enabled = true;
							}
						}
						else
						{
							if (DeactivateGameObjects == true)
							{
								if (audioSource.gameObject.activeSelf == true)
								{
									audioSource.gameObject.SetActive(false);
								}
							}
							else
							{
								if (audioSource.enabled == true)
								{
									audioSource.enabled = false;
								}
							}
						}
					}
				}
			}
		}
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(transform.position, Radius);
	}
#endif
}