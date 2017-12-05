using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[ RequireComponent( typeof( ParticleSystem ) ) ]
public class VOPParser : MonoBehaviour {

	/// <summary>
	/// Enabling this will make Unity automatically update the asset definition at regular intervals. This will (hopefully) deprecated in favor of a more dynamic solution
	/// </summary>
	public bool automaticUpdates = true;
	public float updateInterval = 5f;

	private ParticleSystem _pSystem;
	private HoudiniAssetOTL _assetOTL;
	private HoudiniApiAssetAccessor _assetAccessor;

	/// <summary>
	/// If the particle system is marked as dirty, it should be updated 
	/// </summary>
	private bool _isDirty = false;

	void Start () {
		_assetAccessor = HoudiniApiAssetAccessor.getAssetAccessor(gameObject);
		_assetOTL = GetComponent<HoudiniAssetOTL>();
		_pSystem = GetComponent<ParticleSystem>();
	}

	void OnEnable() {
		_isDirty = true;
	}

	void Update() {
		if (_isDirty) {
			_assetOTL.buildAll();
			InstantiateParticleSystem();
			_isDirty = false;
			StartCoroutine(checkForUpdates());
		}
	}

	void InstantiateParticleSystem() {
		if (_pSystem == null) {
			_pSystem = gameObject.AddComponent<ParticleSystem>();
		}

		_pSystem.gameObject.SetActive(false);

		MapParameters();

		_pSystem.gameObject.SetActive(true);
		_pSystem.Play(true);
	}

	IEnumerator checkForUpdates() {
		yield return new WaitForSecondsRealtime(updateInterval);
		_isDirty = true;
	}

	ParticleSystem.MinMaxCurve CurveFromString(string entry) {
		ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve();

		string parameter = _assetAccessor.getParmStringValue(entry, 0);
		string[] choppedString = parameter.Split(";".ToCharArray());

		switch (choppedString[0]) {
			case "constant":
				curve.mode = ParticleSystemCurveMode.Constant;
				if(choppedString[1] == "float") {
				curve.constant = Convert.ToSingle(choppedString[2]);
				} else
				if (choppedString[1] == "int") {
					curve.constant = Convert.ToInt32(choppedString[2]);
				} else
				if (choppedString[1] == "vector") {
				}
				break;

			case "randomConstant":
				if(choppedString[1] == "float") {
				curve.mode = ParticleSystemCurveMode.TwoConstants;
					curve.constantMin = Convert.ToSingle(choppedString[2]);
					curve.constantMax = Convert.ToSingle(choppedString[3]);
				}
				if (choppedString[1] == "int") {
					curve.constantMin = Convert.ToInt32(choppedString[2]);
					curve.constantMax = Convert.ToInt32(choppedString[3]);
				}
				break;

			case "curve":
				curve.mode = ParticleSystemCurveMode.Curve;
				curve.curveMultiplier = Convert.ToSingle(choppedString[3]);
				int samples = Convert.ToInt32(choppedString[2]);

				if(choppedString[1] == "float") {
					curve.curve = GenerateCurve(choppedString, samples);
				}
				break;

			case "randomCurve":
				curve.mode = ParticleSystemCurveMode.TwoCurves;
				break;
		}
		return curve;
	}

	ParticleSystem.MinMaxGradient GradientFromString(string entry) {
		string parameter = _assetAccessor.getParmStringValue(entry, 0);
		string[] choppedString = parameter.Split(";".ToCharArray());
		
		ParticleSystem.MinMaxGradient curve = new ParticleSystem.MinMaxGradient();
		switch(choppedString[0]) {
			case "constant":
				curve.mode = ParticleSystemGradientMode.Color;

				string color = choppedString[4];
				Debug.Log(color);
				color.Replace("{", "");
				color.Replace("}", "");
				string[] colorArray = color.Split(",".ToCharArray());
				curve.color = new Color(Convert.ToSingle(colorArray[0]), 
										Convert.ToSingle(colorArray[1]), 
										Convert.ToSingle(colorArray[2]), 
										Convert.ToSingle(colorArray[3]));
				Debug.Log(curve.color);
				break;
		}
		return curve;
	}

	///	<Summary>
	///	Reads a list of values and returns a float curve. The number of samples decide the resolution of the resulting curve
	///	</summary>
	AnimationCurve GenerateCurve(string[] parameter, int samples) {
		AnimationCurve curve = new AnimationCurve();

		for (int i = 0; i < samples; i++) {
			float position = (float) i / (float) samples;
			float value = Convert.ToSingle(parameter[i+4]);
			curve.AddKey(position, value);
		}
		return curve;
	}

	/// <summary>
	/// Reads parameters from a HoudiniAssetOTL and assigns them to a Unity ParticleSystem instance
	/// </summary>
	void MapParameters() {

		//	Emitter
		ParticleSystem.MainModule mainModule = _pSystem.main;

		mainModule.duration = _assetAccessor.getParmFloatValue("main_duration", 0);

		mainModule.loop = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_looping", 0));

		mainModule.prewarm = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_prewarm", 0));

		mainModule.startDelay = _assetAccessor.getParmFloatValue("main_startDelay", 0);

		mainModule.startLifetime = CurveFromString("main_startLifetime");

		mainModule.startSpeed = CurveFromString("main_startSpeed");

		mainModule.startSize3D = _assetAccessor.getParmSize("main_startSize") > 1 ? true : false;

		mainModule.startSize = CurveFromString("main_startSize");

		mainModule.startRotation3D = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_3DStartRotation", 0));

		mainModule.startRotation = CurveFromString("main_startRotation");

		mainModule.randomizeRotationDirection = _assetAccessor.getParmFloatValue("main_rotationVariance", 0);

		mainModule.startColor = GradientFromString("main_startColor");

		mainModule.gravityModifier = CurveFromString("main_gravityModifier");

		mainModule.simulationSpace = (ParticleSystemSimulationSpace) _assetAccessor.getParmIntValue("main_simulationSpace", 0);

		mainModule.simulationSpeed = _assetAccessor.getParmFloatValue("main_simulationSpeed", 0);

		mainModule.useUnscaledTime = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_deltaTime", 0));

		mainModule.scalingMode = (ParticleSystemScalingMode) _assetAccessor.getParmIntValue("main_scalingMode", 0);

		_assetAccessor.getParmType("main_deltaTime");

		mainModule.playOnAwake = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_playOnAwake", 0));

		mainModule.emitterVelocityMode = (ParticleSystemEmitterVelocityMode) _assetAccessor.getParmIntValue("main_emitterVelocity", 0);

		mainModule.maxParticles = _assetAccessor.getParmIntValue("main_maxParticles", 0);

		// Emission
		ParticleSystem.EmissionModule emissionModule = _pSystem.emission;
		
		emissionModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("emission_enabled", 0));

		emissionModule.rateOverTime = _assetAccessor.getParmFloatValue("emission_rateOverTime", 0);

		emissionModule.rateOverDistance = _assetAccessor.getParmFloatValue("emission_rateOverDistance", 0);

		//	Shape
		ParticleSystem.ShapeModule shapeModule = _pSystem.shape;

		shapeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("shape_enabled", 0));

		shapeModule.shapeType = (ParticleSystemShapeType) _assetAccessor.getParmIntValue("shape_shape", 0);

		shapeModule.radius = _assetAccessor.getParmFloatValue("shape_radius", 0);

		shapeModule.radiusThickness = _assetAccessor.getParmFloatValue("shape_radiusThickness", 0);

		shapeModule.position = new Vector3(_assetAccessor.getParmFloatValue("shape_position", 0), 
											_assetAccessor.getParmFloatValue("shape_position", 1),
											_assetAccessor.getParmFloatValue("shape_position", 2));

		shapeModule.rotation = new Vector3(_assetAccessor.getParmFloatValue("shape_rotation", 0), 
											_assetAccessor.getParmFloatValue("shape_rotation", 1),
											_assetAccessor.getParmFloatValue("shape_rotation", 2));

		shapeModule.scale = new Vector3(_assetAccessor.getParmFloatValue("shape_scale", 0), 
											_assetAccessor.getParmFloatValue("shape_scale", 1),
											_assetAccessor.getParmFloatValue("shape_scale", 2));

		//	Velocity Over Lifetime
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = _pSystem.velocityOverLifetime;

		velocityOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("velocityOverLifetime_enabled", 0));

		velocityOverLifetimeModule.x = _assetAccessor.getParmFloatValue("velocityOverLifetime_velocity", 0);
		velocityOverLifetimeModule.y = _assetAccessor.getParmFloatValue("velocityOverLifetime_velocity", 1);
		velocityOverLifetimeModule.z = _assetAccessor.getParmFloatValue("velocityOverLifetime_velocity", 2);

		//	Limit Velocity Over Lifetime
		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = _pSystem.limitVelocityOverLifetime;

		limitVelocityOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("limitVelocityOverLifetime_enabled", 0));

		limitVelocityOverLifetimeModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("limitVelocityOverLifetime_separateAxes", 0));

		limitVelocityOverLifetimeModule.limit = _assetAccessor.getParmFloatValue("limitVelocityOverLifetime_limit", 0);

		limitVelocityOverLifetimeModule.dampen = _assetAccessor.getParmFloatValue("limitVelocityOverLifetime_dampen", 0);

		//	Inherit Velocity
		ParticleSystem.InheritVelocityModule inheritVelocityModule = _pSystem.inheritVelocity;

		inheritVelocityModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("inheritVelocity_enabled", 0));

		inheritVelocityModule.mode = (ParticleSystemInheritVelocityMode) _assetAccessor.getParmIntValue("inheritVelocity_mode", 0);

		inheritVelocityModule.curve = _assetAccessor.getParmFloatValue("inheritVelocity_multiplier", 0);

		//	Force Over Lifetime
		ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = _pSystem.forceOverLifetime;

		forceOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("forceOverLifetime_enabled", 0));

		forceOverLifetimeModule.x = _assetAccessor.getParmFloatValue("forceOverLifetime_force", 0);
		forceOverLifetimeModule.y = _assetAccessor.getParmFloatValue("forceOverLifetime_force", 1);
		forceOverLifetimeModule.z = _assetAccessor.getParmFloatValue("forceOverLifetime_force", 2);

		forceOverLifetimeModule.randomized = Convert.ToBoolean(_assetAccessor.getParmIntValue("forceOverLifetime_randomized", 0));

		//	Color Over Lifetime
		ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = _pSystem.colorOverLifetime;

		colorOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("colorOverLifetime_enabled", 0));

		colorOverLifetimeModule.color = new Color(_assetAccessor.getParmFloatValue("colorOverLifetime_color", 0),
													_assetAccessor.getParmFloatValue("colorOverLifetime_color", 1),
													_assetAccessor.getParmFloatValue("colorOverLifetime_color", 2),
													_assetAccessor.getParmFloatValue("colorOverLifetime_color", 3));

		//	Color By Speed
		ParticleSystem.ColorBySpeedModule colorBySpeedModule = _pSystem.colorBySpeed;

		colorBySpeedModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("colorBySpeed_enabled", 0));

		colorBySpeedModule.color = new Color(_assetAccessor.getParmFloatValue("colorBySpeed_color", 0),
												_assetAccessor.getParmFloatValue("colorBySpeed_color", 1),
												_assetAccessor.getParmFloatValue("colorBySpeed_color", 2),
												_assetAccessor.getParmFloatValue("colorBySpeed_color", 3));

		colorBySpeedModule.range = new Vector2(_assetAccessor.getParmFloatValue("colorBySpeed_range", 0),
												_assetAccessor.getParmFloatValue("colorBySpeed_range", 1));

		//	Size Over Lifetime
		ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = _pSystem.sizeOverLifetime;

		sizeOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeOverLifetimeModule_enabled", 0));

		sizeOverLifetimeModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeOverLifetime_separateAxes", 0));

		sizeOverLifetimeModule.size = _assetAccessor.getParmFloatValue("sizeOverLifetime_size", 0);

		//	Size By Speed
		ParticleSystem.SizeBySpeedModule sizeBySpeedModule = _pSystem.sizeBySpeed;

		sizeBySpeedModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeBySpeed_enabled", 0));

		sizeBySpeedModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeBySpeed_separateAxes", 0));

		sizeBySpeedModule.size = _assetAccessor.getParmFloatValue("sizeBySpeed_size", 0);

		sizeBySpeedModule.range = new Vector2(_assetAccessor.getParmFloatValue("sizeBySpeed_range", 0),
												_assetAccessor.getParmFloatValue("sizeBySpeed_range", 1));

		//	Rotation Over Lifetime
		ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = _pSystem.rotationOverLifetime;

		rotationOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationOverLifetime_enabled", 0));

		rotationOverLifetimeModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationOverLifetime_separateAxes", 0));

		rotationOverLifetimeModule.x = _assetAccessor.getParmFloatValue("rotationOverLifetime_angularVelocity", 0);
		rotationOverLifetimeModule.y = _assetAccessor.getParmFloatValue("rotationOverLifetime_angularVelocity", 1);
		rotationOverLifetimeModule.z = _assetAccessor.getParmFloatValue("rotationOverLifetime_angularVelocity", 2);

		// Rotation By Speed
		ParticleSystem.RotationBySpeedModule rotationBySpeedModule = _pSystem.rotationBySpeed;

		rotationBySpeedModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationBySpeed_enabled", 0));

		rotationBySpeedModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationBySpeed_separateAxes", 0)); 

		rotationBySpeedModule.x = _assetAccessor.getParmFloatValue("rotationBySpeed_angularVelocity", 0);
		rotationBySpeedModule.y = _assetAccessor.getParmFloatValue("rotationBySpeed_angularVelocity", 1);
		rotationBySpeedModule.z = _assetAccessor.getParmFloatValue("rotationBySpeed_angularVelocity", 2);

		rotationBySpeedModule.range = new Vector2(_assetAccessor.getParmFloatValue("rotationBySpeed_range", 0),
													_assetAccessor.getParmFloatValue("rotationBySpeed_range", 1));

		// External Forces
		ParticleSystem.ExternalForcesModule externalForcesModule = _pSystem.externalForces;

		// Noise
		ParticleSystem.NoiseModule noiseModule = _pSystem.noise;

		// Collision
		ParticleSystem.CollisionModule collisionModule = _pSystem.collision;

		// Triggers
		ParticleSystem.TriggerModule triggerModule = _pSystem.trigger;

		// Sub Emitters
		ParticleSystem.SubEmittersModule subEmittersModule = _pSystem.subEmitters;

		// Texture Sheet Animation
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = _pSystem.textureSheetAnimation;

		textureSheetAnimationModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("textureSheetAnimation_enabled", 0));

		textureSheetAnimationModule.mode = (ParticleSystemAnimationMode) _assetAccessor.getParmIntValue("textureSheetAnimation_mode", 0);

		textureSheetAnimationModule.animation = (ParticleSystemAnimationType) _assetAccessor.getParmIntValue("textureSheetAnimation_animation", 0);

		textureSheetAnimationModule.frameOverTime = _assetAccessor.getParmFloatValue("textureSheetAnimation_frame", 0);

		textureSheetAnimationModule.startFrame = _assetAccessor.getParmIntValue("textureSheetAnimation_startFrame", 0);

		textureSheetAnimationModule.cycleCount = _assetAccessor.getParmIntValue("textureSheetAnimation_cycles", 0);

		textureSheetAnimationModule.flipU = _assetAccessor.getParmFloatValue("textureSheetAnimation_flipU", 0);

		textureSheetAnimationModule.flipU = _assetAccessor.getParmFloatValue("textureSheetAnimation_flipV", 0);

		// Lights
		ParticleSystem.LightsModule lightsModule = _pSystem.lights;

		// Trails
		ParticleSystem.TrailModule trailModule = _pSystem.trails;

		// Custom Data
		ParticleSystem.CustomDataModule customDataModule = _pSystem.customData;

		// Renderer
		// ParticleSystem
	}
}
