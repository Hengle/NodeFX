using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode]
[ RequireComponent( typeof( ParticleSystem ) ) ]
public class VOPParser : MonoBehaviour {

	public bool automaticUpdates = true;
	public float updateInterval = 5.0f;

	private ParticleSystem pSystem;
	private HoudiniAssetOTL assetOTL;
	private HoudiniApiAssetAccessor assetAccessor;
	private bool isDirty = false;

	void Start () {
		assetAccessor = HoudiniApiAssetAccessor.getAssetAccessor(gameObject);
		assetOTL = GetComponent<HoudiniAssetOTL>();
		pSystem = GetComponent<ParticleSystem>();
		StartCoroutine(checkForUpdates());
	}

	void Update() {
		if (isDirty) {
			assetOTL.buildAll();
			InstantiateParticleSystem();
			isDirty = false;
			StartCoroutine(checkForUpdates());
		}
	}

	void InstantiateParticleSystem() {
		pSystem.gameObject.SetActive(false);

		MapParameters();

		pSystem.gameObject.SetActive(true);
		pSystem.Play(true);
	}

	///	<Summary>
	///	Interprets custom node definitions, such as curves and gradients, and returns them in a format Unity is comfortable with
	///	</summary>
	void InterpretString(string parameter) {
		HoudiniApiAssetAccessor.ParmType type = assetAccessor.getParmType(parameter);
		string param = "";
		if (type == HoudiniApiAssetAccessor.ParmType.FLOAT) {
			if (assetAccessor.getParmSize(parameter) == 1)
			param = assetAccessor.getParmStringValue(parameter, 0);

			switch (param) {
				case "unityCurve":
				break;

				case "unityBurst":
				break;

				default:
				Debug.Log("Unhandled string case");
				break;
			}
		}
	}

	/// <summary>
	/// Reads parameters from a HoudiniAssetOTL and assigns them to a Unity ParticleSystem instance
	/// </summary>
	void MapParameters() {

		//	Emitter
		ParticleSystem.MainModule mainModule = pSystem.main;

		mainModule.duration = assetAccessor.getParmFloatValue("main_duration", 0);

		mainModule.loop = Convert.ToBoolean(assetAccessor.getParmIntValue("main_looping", 0));

		mainModule.prewarm = Convert.ToBoolean(assetAccessor.getParmIntValue("main_prewarm", 0));

		mainModule.startDelay = assetAccessor.getParmFloatValue("main_startDelay", 0);

		mainModule.startLifetime = assetAccessor.getParmFloatValue("main_startLifetime", 0);

		mainModule.startSpeed = assetAccessor.getParmFloatValue("main_startSpeed", 0);

		mainModule.startSize3D = Convert.ToBoolean(assetAccessor.getParmIntValue("main_3DStartSize", 0));

		mainModule.startSize = assetAccessor.getParmFloatValue("main_startSize", 0);

		mainModule.startRotation3D = Convert.ToBoolean(assetAccessor.getParmIntValue("main_3DStartRotation", 0));

		mainModule.startRotation = assetAccessor.getParmFloatValue("main_startRotation", 0);

		mainModule.randomizeRotationDirection = assetAccessor.getParmFloatValue("main_rotationVariance", 0);

		mainModule.startColor = new Color(assetAccessor.getParmFloatValue("main_startColor", 0), 
										assetAccessor.getParmFloatValue("main_startColor", 1), 
										assetAccessor.getParmFloatValue("main_startColor", 2), 
										assetAccessor.getParmFloatValue("main_startColor", 3));

		mainModule.gravityModifier = assetAccessor.getParmFloatValue("main_gravityModifier", 0);

		mainModule.simulationSpace = (ParticleSystemSimulationSpace) assetAccessor.getParmIntValue("main_simulationSpace", 0);

		mainModule.simulationSpeed = assetAccessor.getParmFloatValue("main_simulationSpeed", 0);

		mainModule.useUnscaledTime = Convert.ToBoolean(assetAccessor.getParmIntValue("main_deltaTime", 0));

		mainModule.scalingMode = (ParticleSystemScalingMode) assetAccessor.getParmIntValue("main_scalingMode", 0);

		assetAccessor.getParmType("main_deltaTime");

		mainModule.playOnAwake = Convert.ToBoolean(assetAccessor.getParmIntValue("main_playOnAwake", 0));

		mainModule.emitterVelocityMode = (ParticleSystemEmitterVelocityMode) assetAccessor.getParmIntValue("main_emitterVelocity", 0);

		mainModule.maxParticles = assetAccessor.getParmIntValue("main_maxParticles", 0);

		// Emission
		ParticleSystem.EmissionModule emissionModule = pSystem.emission;
		
		emissionModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("emission_enabled", 0));

		emissionModule.rateOverTime = assetAccessor.getParmFloatValue("emission_rateOverTime", 0);

		emissionModule.rateOverDistance = assetAccessor.getParmFloatValue("emission_rateOverDistance", 0);

		//	Shape
		ParticleSystem.ShapeModule shapeModule = pSystem.shape;

		shapeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("shape_enabled", 0));

		shapeModule.shapeType = (ParticleSystemShapeType) assetAccessor.getParmIntValue("shape_shape", 0);

		shapeModule.radius = assetAccessor.getParmFloatValue("shape_radius", 0);

		shapeModule.radiusThickness = assetAccessor.getParmFloatValue("shape_radiusThickness", 0);

		shapeModule.position = new Vector3(assetAccessor.getParmFloatValue("shape_position", 0), 
										assetAccessor.getParmFloatValue("shape_position", 1),
										assetAccessor.getParmFloatValue("shape_position", 2));

		shapeModule.rotation = new Vector3(assetAccessor.getParmFloatValue("shape_rotation", 0), 
										assetAccessor.getParmFloatValue("shape_rotation", 1),
										assetAccessor.getParmFloatValue("shape_rotation", 2));

		shapeModule.scale = new Vector3(assetAccessor.getParmFloatValue("shape_scale", 0), 
										assetAccessor.getParmFloatValue("shape_scale", 1),
										assetAccessor.getParmFloatValue("shape_scale", 2));

		//	Velocity Over Lifetime
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = pSystem.velocityOverLifetime;

		velocityOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("velocityOverLifetime_enabled", 0));

		velocityOverLifetimeModule.x = assetAccessor.getParmFloatValue("velocityOverLifetime_velocity", 0);
		velocityOverLifetimeModule.y = assetAccessor.getParmFloatValue("velocityOverLifetime_velocity", 1);
		velocityOverLifetimeModule.z = assetAccessor.getParmFloatValue("velocityOverLifetime_velocity", 2);

		//	Limit Velocity Over Lifetime
		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = pSystem.limitVelocityOverLifetime;

		limitVelocityOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("limitVelocityOverLifetime_enabled", 0));

		limitVelocityOverLifetimeModule.dampen = assetAccessor.getParmFloatValue("limitVelocityOverLifetime_dampening", 0);

		//	Inherit Velocity
		ParticleSystem.InheritVelocityModule inheritVelocityModule = pSystem.inheritVelocity;

		//	Force Over Lifetime
		ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = pSystem.forceOverLifetime;

		//	Color Over Lifetime
		ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = pSystem.colorOverLifetime;

		colorOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("colorOverLifetime_enabled", 0));

		colorOverLifetimeModule.color = new Color(assetAccessor.getParmFloatValue("colorOverLifetime_color", 0),
												assetAccessor.getParmFloatValue("colorOverLifetime_color", 1),
												assetAccessor.getParmFloatValue("colorOverLifetime_color", 2),
												assetAccessor.getParmFloatValue("colorOverLifetime_color", 3));

		//	Color By Speed
		ParticleSystem.ColorBySpeedModule colorBySpeedModule = pSystem.colorBySpeed;

		//	Size Over Lifetime
		ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = pSystem.sizeOverLifetime;

		sizeOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("sizeOverLifetimeModule_enabled", 0));

		//	Size By Speed
		ParticleSystem.SizeBySpeedModule sizeBySpeedModule = pSystem.sizeBySpeed;

		//	Rotation Over Lifetime
		ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = pSystem.rotationOverLifetime;

		// //	Rotation By Speed
		ParticleSystem.RotationBySpeedModule rotationBySpeedModule = pSystem.rotationBySpeed;

		// //	External Forces
		ParticleSystem.ExternalForcesModule externalForcesModule = pSystem.externalForces;

		// //	Noise
		ParticleSystem.NoiseModule noiseModule = pSystem.noise;

		// //	Collision
		ParticleSystem.CollisionModule collisionModule = pSystem.collision;

		// //	Triggers
		ParticleSystem.TriggerModule triggerModule = pSystem.trigger;

		// //	Sub Emitters
		ParticleSystem.SubEmittersModule subEmittersModule = pSystem.subEmitters;

		// //	Texture Sheet Animation
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = pSystem.textureSheetAnimation;

		// //	Lights
		ParticleSystem.LightsModule lightsModule = pSystem.lights;

		// //	Trails
		ParticleSystem.TrailModule trailModule = pSystem.trails;

		// //	Custom Data
		ParticleSystem.CustomDataModule customDataModule = pSystem.customData;

		// //	Renderer
		// ParticleSystem
	}

	IEnumerator checkForUpdates() {
		yield return new WaitForSecondsRealtime(updateInterval);
		isDirty = true;
	}
}
