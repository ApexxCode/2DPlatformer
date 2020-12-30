using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(VA_AudioSource))]
public class VA_AudioSource_Editor : VA_Editor<VA_AudioSource>
{
	protected override void OnInspector()
	{
		DrawDefault("Position");
		
		if (Any(t => t.Position == true))
		{
			BeginIndent();
				BeginError(Any(t => t.PositionDampening < 0.0f));
					DrawDefault("PositionDampening");
				EndError();
				BeginError(Any(t => t.Shapes == null || t.Shapes.Count == 0 || t.Shapes.Exists(s => s == null)));
					DrawDefault("Shapes");
				EndError();
				BeginError(Any(t => t.ExcludedShapes != null && t.ExcludedShapes.Exists(s => s == null)));
					DrawDefault("ExcludedShapes");
				EndError();
			EndIndent();
		}

		Separator();

		DrawDefault("Blend");

		if (Any(t => t.Blend == true))
		{
			BeginIndent();
				BeginError(Any(t => t.BlendMinDistance < 0.0f || t.BlendMinDistance > t.BlendMaxDistance));
					DrawDefault("BlendMinDistance");
				EndError();
				BeginError(Any(t => t.BlendMaxDistance < 0.0f || t.BlendMinDistance > t.BlendMaxDistance));
					DrawDefault("BlendMaxDistance");
				EndError();
				DrawDefault("BlendCurve");
			EndIndent();
		}

		Separator();

		DrawDefault("Volume");

		if (Any(t => t.Volume == true))
		{
			BeginIndent();
				DrawDefault("BaseVolume");
				BeginDisabled();
					DrawDefault("Zone");
				EndDisabled();

				Separator();

				DrawDefault("Fade");

				if (Any(t => t.Fade == true))
				{
					BeginIndent();
						BeginError(Any(t => t.FadeMinDistance < 0.0f || t.FadeMinDistance > t.FadeMaxDistance));
							DrawDefault("FadeMinDistance");
						EndError();
						BeginError(Any(t => t.FadeMaxDistance < 0.0f || t.FadeMinDistance > t.FadeMaxDistance));
							DrawDefault("FadeMaxDistance");
						EndError();
						DrawDefault("FadeCurve");
					EndIndent();
				}

				Separator();

				DrawDefault("Occlude");

				if (Any(t => t.Occlude == true))
				{
					BeginIndent();
						DrawDefault("OccludeMethod");
						DrawDefault("OccludeMaterial");
						BeginError(Any(t => t.OccludeDampening <= 0.0f));
							DrawDefault("OccludeDampening");
						EndError();
						BeginError(Any(t => t.OccludeGroups == null || t.OccludeGroups.Count == 0));
							DrawDefault("OccludeGroups");
						EndError();
					EndIndent();
				}
			EndIndent();
		}

		if (Any(t => IsSoundWrong(t)))
		{
			EditorGUILayout.HelpBox("This sound's Spatial Blend isn't set to 3D, which is required if you're not using the Volume or Blend settings.", MessageType.Warning);
		}
	}

	private bool IsSoundWrong(VA_AudioSource a)
	{
		if (a.Fade == false && a.Blend == false)
		{
			var s = a.GetComponent<AudioSource>();

			if (s != null && s.spatialBlend != 1.0f)
			{
				return true;
			}
		}

		return false;
	}
}
#endif

[ExecuteInEditMode]
[AddComponentMenu("Volumetric Audio/VA Audio Source")]
public class VA_AudioSource : MonoBehaviour
{
	public enum OccludeType
	{
		Raycast,
		RaycastAll,
	}

	[System.Serializable]
	public class OccludeGroup
	{
		public LayerMask Layers;

		[Range(0.0f, 1.0f)]
		public float Volume;
	}

	[Tooltip("Should this sound have its position update?")]
	public bool Position = true;

	[Tooltip("The speed at which the sound position changes (0 = instant)")]
	public float PositionDampening;

	[Tooltip("The shapes you want the audio source to emit from")]
	public List<VA_Shape> Shapes;

	[Tooltip("The shapes you want the audio source to be excluded from")]
	public List<VA_VolumetricShape> ExcludedShapes;

	[Tooltip("Should this sound have its Spatial Blend update?")]
	public bool Blend;

	[Tooltip("The distance at which the sound becomes fuly mono")]
	public float BlendMinDistance = 0.0f;

	[Tooltip("The distance at which the sound becomes fuly stereo")]
	public float BlendMaxDistance = 5.0f;

	[Tooltip("The distribution of the mono to stereo ratio")]
	public AnimationCurve BlendCurve;

	[Tooltip("Should this sound have its volume update?")]
	public bool Volume = true;

	[Tooltip("The base volume of the audio source")]
	[Range(0.0f, 1.0f)]
	public float BaseVolume = 1.0f;

	[Tooltip("The zone this sound is associated with")]
	public VA_Zone Zone;

	[Tooltip("Should the volume fade based on distance?")]
	[FormerlySerializedAs("Volume")]
	public bool Fade;

	[Tooltip("The distance at which the sound fades to maximum volume")]
	[FormerlySerializedAs("VolumeMinDistance")]
	public float FadeMinDistance = 0.0f;

	[Tooltip("The distance at which the sound fades to minimum volume")]
	[FormerlySerializedAs("VolumeMaxDistance")]
	public float FadeMaxDistance = 5.0f;

	[Tooltip("The distribution of volume based on its scaled distance")]
	[FormerlySerializedAs("VolumeCurve")]
	public AnimationCurve FadeCurve;

	[Tooltip("Should this sound be blocked when behind other objects?")]
	public bool Occlude;

	[Tooltip("The raycast style against the occlusion groups")]
	public OccludeType OccludeMethod;

	[Tooltip("Check for VA_Material instances attached to the occlusion object")]
	public bool OccludeMaterial;

	[Tooltip("How quickly the sound fades in/out when behind an object")]
	[FormerlySerializedAs("OccludeSpeed")]
	public float OccludeDampening = 5.0f;

	[Tooltip("The amount of occlusion checks")]
	public List<OccludeGroup> OccludeGroups;

	public float OccludeAmount = 1.0f;

	// Cached AudioSource
	[System.NonSerialized]
	private AudioSource audioSource;

	private static Keyframe[] defaultBlendCurveKeys = new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f) };

	private static Keyframe[] defaultVolumeCurveKeys = new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f) };

	public bool HasVolumetricShape
	{
		get
		{
			for (var i = Shapes.Count - 1; i >= 0; i--)
			{
				var shape = Shapes[i];

				if (shape != null)
				{
					var  sphereShape = shape as VA_Sphere;  if ( sphereShape != null &&  sphereShape.IsHollow == false) return true;
					var     boxShape = shape as VA_Box;     if (    boxShape != null &&     boxShape.IsHollow == false) return true;
					var capsuleShape = shape as VA_Capsule; if (capsuleShape != null && capsuleShape.IsHollow == false) return true;
					var    meshShape = shape as VA_Mesh;    if (   meshShape != null &&    meshShape.IsHollow == false) return true;
				}
			}

			return false;
		}
	}

	protected virtual void Start()
	{
		if (BlendCurve == null)
		{
			BlendCurve = new AnimationCurve(defaultBlendCurveKeys);
		}

		if (FadeCurve == null)
		{
			FadeCurve = new AnimationCurve(defaultVolumeCurveKeys);
		}
	}

	protected virtual void LateUpdate()
	{
		// Make sure the listener exists
		var listenerPosition = default(Vector3);

		if (VA_Helper.GetListenerPosition(ref listenerPosition) == true)
		{
			if (Position == true)
			{
				var closestDistance = float.PositiveInfinity;
				var closestShape    = default(VA_Shape);
				var closestPoint    = default(Vector3);

				// Find closest point to all shapes
				if (Shapes != null)
				{
					for (var i = Shapes.Count - 1; i >= 0; i--)
					{
						var shape = Shapes[i];

						if (VA_Helper.Enabled(shape) == true && shape.FinalPointSet == true && shape.FinalPointDistance < closestDistance)
						{
							closestDistance = shape.FinalPointDistance;
							closestPoint    = shape.FinalPoint;
							closestShape    = shape;
						}
					}
				}

				// If the closest point is closer than the excluded point, then make the excluded point the closest
				if (ExcludedShapes != null)
				{
					for (var i = ExcludedShapes.Count - 1; i >= 0; i--)
					{
						var excludedShape = ExcludedShapes[i];

						if (VA_Helper.Enabled(excludedShape) == true && excludedShape.IsHollow == false && excludedShape.InnerPointInside == true)
						{
							if (excludedShape.OuterPointSet == true && excludedShape.OuterPointDistance > closestDistance)
							{
								closestDistance = excludedShape.OuterPointDistance;
								closestPoint    = excludedShape.OuterPoint;
								closestShape    = excludedShape;

								break;
							}
						}
					}
				}

				if (closestShape != null)
				{
					if (PositionDampening <= 0.0f)
					{
						transform.position = closestPoint;
					}
					else
					{
						transform.position = VA_Helper.Dampen3(transform.position, closestPoint, PositionDampening, Time.deltaTime);
					}
				}
				else
				{
					closestPoint    = transform.position;
					closestDistance = Vector3.Distance(closestPoint, listenerPosition);
				}
			}

			// Modify the blend?
			if (Blend == true)
			{
				var distance   = Vector3.Distance(transform.position, listenerPosition);
				var distance01 = Mathf.InverseLerp(BlendMinDistance, BlendMaxDistance, distance);

				SetPanLevel(BlendCurve.Evaluate(distance01));
			}

			// Modify the volume?
			if (Volume == true)
			{
				var finalVolume = BaseVolume;

				// Modify via zone?
				if (Zone != null)
				{
					finalVolume *= Zone.Volume;
				}

				// Modify via distance?
				if (Fade == true)
				{
					var distance   = Vector3.Distance(transform.position, listenerPosition);
					var distance01 = Mathf.InverseLerp(FadeMinDistance, FadeMaxDistance, distance);

					finalVolume *= FadeCurve.Evaluate(distance01);
				}

				// Modify via occlusion?
				if (Occlude == true)
				{
					var direction    = listenerPosition - transform.position;
					var targetAmount = 1.0f;

					if (OccludeGroups != null)
					{
						for (var i = OccludeGroups.Count - 1; i >= 0; i--)
						{
							var group = OccludeGroups[i];

							switch (OccludeMethod)
							{
								case OccludeType.Raycast:
								{
									var hit = default(RaycastHit);

									if (Physics.Raycast(transform.position, direction, out hit, direction.magnitude, group.Layers) == true)
									{
										targetAmount *= GetOcclusionVolume(group, hit);
									}
								}
								break;

								case OccludeType.RaycastAll:
								{
									var hits = Physics.RaycastAll(transform.position, direction, direction.magnitude, group.Layers);

									for (var j = hits.Length - 1; j >= 0; j--)
									{
										targetAmount *= GetOcclusionVolume(group, hits[j]);
									}
								}
								break;
							}
						}
					}

					OccludeAmount = VA_Helper.Dampen(OccludeAmount, targetAmount, OccludeDampening, Time.deltaTime, 0.1f);

					finalVolume *= OccludeAmount;
				}

				SetVolume(finalVolume);
			}
		}
	}

	private float GetOcclusionVolume(OccludeGroup group, RaycastHit hit)
	{
		if (OccludeMaterial == true)
		{
			var material = hit.collider.GetComponentInParent<VA_Material>();

			if (material != null)
			{
				return material.OcclusionVolume;
			}
		}

		return group.Volume;
	}

	// If you're not using Unity's built-in audio system, then modify the code below, or make a new component that inherits VA_AudioSource and overrides this method
	protected virtual void SetPanLevel(float newPanLevel)
	{
		if (audioSource == null) audioSource = GetComponent<AudioSource>();

		if (audioSource != null)
		{
			audioSource.spatialBlend = newPanLevel;
		}
	}

	// If you're not using Unity's built-in audio system, then modify the code below, or make a new component that inherits VA_AudioSource and overrides this method
	protected virtual void SetVolume(float newVolume)
	{
		if (audioSource == null) audioSource = GetComponent<AudioSource>();

		if (audioSource != null)
		{
			audioSource.volume = newVolume;
		}
	}

#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		if (VA_Helper.Enabled(this) == true)
		{
			// Draw red lines from this audio source to all shapes
			Gizmos.color = Color.red;

			if (Shapes != null)
			{
				for (var i = Shapes.Count - 1; i >= 0; i--)
				{
					var shape = Shapes[i];

					if (VA_Helper.Enabled(shape) == true && shape.FinalPointSet == true)
					{
						Gizmos.DrawLine(transform.position, shape.FinalPoint);
					}
				}
			}

			// Draw green spheres for blend distances
			if (Blend == true)
			{
				for (var i = 0; i <= 50; i++)
				{
					var frac = i * 0.02f;

					Gizmos.color = new Color(0.0f, 1.0f, 0.0f, BlendCurve.Evaluate(frac));

					Gizmos.DrawWireSphere(transform.position, Mathf.Lerp(BlendMinDistance, BlendMaxDistance, frac));
				}
			}

			// Draw blue spheres for volume distances
			if (Fade == true)
			{
				for (var i = 0; i <= 50; i++)
				{
					var frac = i * 0.02f;

					Gizmos.color = new Color(0.0f, 0.0f, 1.0f, BlendCurve.Evaluate(frac));

					Gizmos.DrawWireSphere(transform.position, Mathf.Lerp(FadeMinDistance, FadeMaxDistance, frac));
				}
			}
		}
	}
#endif
}