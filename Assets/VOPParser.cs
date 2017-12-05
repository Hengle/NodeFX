using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[ RequireComponent( typeof( ParticleSystem ) ) ]
public class VOPParser : MonoBehaviour {

	public bool automaticUpdates = true;

	private ParticleSystem pSystem;
	private HoudiniAssetOTL assetOTL;
	private HoudiniApiAssetAccessor assetAccessor;
	private bool isDirty = false;

	void Start () {
		assetAccessor = HoudiniApiAssetAccessor.getAssetAccessor(gameObject);
		assetOTL = GetComponent<HoudiniAssetOTL>();
		pSystem = GetComponent<ParticleSystem>();
	}

	void OnEnable() {
		isDirty = true;
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

	IEnumerator checkForUpdates() {
		yield return new WaitForSecondsRealtime(5f);
		isDirty = true;
	}

	ParticleSystem.MinMaxCurve CurveFromString(string entry) {
		string parameter = assetAccessor.getParmStringValue(entry, 0);
		string[] choppedString = parameter.Split(";".ToCharArray());
		ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve();

		switch (choppedString[0]) {
			
			//	Constant
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

			//	Random between MinMax
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

			//	Curve
			case "curve":
				curve.mode = ParticleSystemCurveMode.Curve;
				curve.curveMultiplier = Convert.ToSingle(choppedString[3]);
				int samples = Convert.ToInt32(choppedString[2]);

				if(choppedString[1] == "float") {
					curve.curve = GenerateCurve(choppedString, samples);
				}
				break;

			//	Random between Curve MinMax
			case "randomCurve":
				curve.mode = ParticleSystemCurveMode.TwoCurves;
				break;
		}
	
		return curve;
	}

	///	<Summary>
	///	Interprets custom node definitions, such as curves and gradients, and returns them in a format Unity is comfortable with
	///	</summary>
	AnimationCurve GenerateCurve(string[] parameter, int samples) {
		AnimationCurve curve = new AnimationCurve();

		for (int i = 0; i < samples; i++) {
			float position = (float) i / (float) samples;
			// float position = Convert.ToSingle(parameter[i]);
			float value = Convert.ToSingle(parameter[i+4]);
			curve.AddKey(position, value);
		}

		return curve;
	}

	ParticleSystem.MinMaxGradient GradientFromString(string entry) {
		string parameter = assetAccessor.getParmStringValue(entry, 0);
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

		mainModule.startLifetime = CurveFromString("main_startLifetime");

		mainModule.startSpeed = CurveFromString("main_startSpeed");

		mainModule.startSize3D = assetAccessor.getParmSize("main_startSize") > 1 ? true : false;

		mainModule.startSize = CurveFromString("main_startSize");

		mainModule.startRotation3D = Convert.ToBoolean(assetAccessor.getParmIntValue("main_3DStartRotation", 0));

		mainModule.startRotation = CurveFromString("main_startRotation");

		mainModule.randomizeRotationDirection = assetAccessor.getParmFloatValue("main_rotationVariance", 0);

		mainModule.startColor = GradientFromString("main_startColor");

		mainModule.gravityModifier = CurveFromString("main_gravityModifier");

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

		limitVelocityOverLifetimeModule.separateAxes = Convert.ToBoolean(assetAccessor.getParmIntValue("limitVelocityOverLifetime_separateAxes", 0));

		limitVelocityOverLifetimeModule.limit = assetAccessor.getParmFloatValue("limitVelocityOverLifetime_limit", 0);

		limitVelocityOverLifetimeModule.dampen = assetAccessor.getParmFloatValue("limitVelocityOverLifetime_dampen", 0);

		//	Inherit Velocity
		ParticleSystem.InheritVelocityModule inheritVelocityModule = pSystem.inheritVelocity;

		inheritVelocityModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("inheritVelocity_enabled", 0));

		inheritVelocityModule.mode = (ParticleSystemInheritVelocityMode) assetAccessor.getParmIntValue("inheritVelocity_mode", 0);

		inheritVelocityModule.curve = assetAccessor.getParmFloatValue("inheritVelocity_multiplier", 0);

		//	Force Over Lifetime
		ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = pSystem.forceOverLifetime;

		forceOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("forceOverLifetime_enabled", 0));

		forceOverLifetimeModule.x = assetAccessor.getParmFloatValue("forceOverLifetime_force", 0);
		forceOverLifetimeModule.y = assetAccessor.getParmFloatValue("forceOverLifetime_force", 1);
		forceOverLifetimeModule.z = assetAccessor.getParmFloatValue("forceOverLifetime_force", 2);

		forceOverLifetimeModule.randomized = Convert.ToBoolean(assetAccessor.getParmIntValue("forceOverLifetime_randomized", 0));

		//	Color Over Lifetime
		ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = pSystem.colorOverLifetime;

		colorOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("colorOverLifetime_enabled", 0));

		colorOverLifetimeModule.color = new Color(assetAccessor.getParmFloatValue("colorOverLifetime_color", 0),
													assetAccessor.getParmFloatValue("colorOverLifetime_color", 1),
													assetAccessor.getParmFloatValue("colorOverLifetime_color", 2),
													assetAccessor.getParmFloatValue("colorOverLifetime_color", 3));

		//	Color By Speed
		ParticleSystem.ColorBySpeedModule colorBySpeedModule = pSystem.colorBySpeed;

		colorBySpeedModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("colorBySpeed_enabled", 0));

		colorBySpeedModule.color = new Color(assetAccessor.getParmFloatValue("colorBySpeed_color", 0),
												assetAccessor.getParmFloatValue("colorBySpeed_color", 1),
												assetAccessor.getParmFloatValue("colorBySpeed_color", 2),
												assetAccessor.getParmFloatValue("colorBySpeed_color", 3));

		colorBySpeedModule.range = new Vector2(assetAccessor.getParmFloatValue("colorBySpeed_range", 0),
												assetAccessor.getParmFloatValue("colorBySpeed_range", 1));

		//	Size Over Lifetime
		ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = pSystem.sizeOverLifetime;

		sizeOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("sizeOverLifetimeModule_enabled", 0));

		sizeOverLifetimeModule.separateAxes = Convert.ToBoolean(assetAccessor.getParmIntValue("sizeOverLifetime_separateAxes", 0));

		sizeOverLifetimeModule.size = assetAccessor.getParmFloatValue("sizeOverLifetime_size", 0);

		//	Size By Speed
		ParticleSystem.SizeBySpeedModule sizeBySpeedModule = pSystem.sizeBySpeed;

		sizeBySpeedModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("sizeBySpeed_enabled", 0));

		sizeBySpeedModule.separateAxes = Convert.ToBoolean(assetAccessor.getParmIntValue("sizeBySpeed_separateAxes", 0));

		sizeBySpeedModule.size = assetAccessor.getParmFloatValue("sizeBySpeed_size", 0);

		sizeBySpeedModule.range = new Vector2(assetAccessor.getParmFloatValue("sizeBySpeed_range", 0),
												assetAccessor.getParmFloatValue("sizeBySpeed_range", 1));

		//	Rotation Over Lifetime
		ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = pSystem.rotationOverLifetime;

		rotationOverLifetimeModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("rotationOverLifetime_enabled", 0));

		rotationOverLifetimeModule.separateAxes = Convert.ToBoolean(assetAccessor.getParmIntValue("rotationOverLifetime_separateAxes", 0));

		rotationOverLifetimeModule.x = assetAccessor.getParmFloatValue("rotationOverLifetime_angularVelocity", 0);
		rotationOverLifetimeModule.y = assetAccessor.getParmFloatValue("rotationOverLifetime_angularVelocity", 1);
		rotationOverLifetimeModule.z = assetAccessor.getParmFloatValue("rotationOverLifetime_angularVelocity", 2);

		// Rotation By Speed
		ParticleSystem.RotationBySpeedModule rotationBySpeedModule = pSystem.rotationBySpeed;

		rotationBySpeedModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("rotationBySpeed_enabled", 0));

		rotationBySpeedModule.separateAxes = Convert.ToBoolean(assetAccessor.getParmIntValue("rotationBySpeed_separateAxes", 0)); 

		rotationBySpeedModule.x = assetAccessor.getParmFloatValue("rotationBySpeed_angularVelocity", 0);
		rotationBySpeedModule.y = assetAccessor.getParmFloatValue("rotationBySpeed_angularVelocity", 1);
		rotationBySpeedModule.z = assetAccessor.getParmFloatValue("rotationBySpeed_angularVelocity", 2);

		rotationBySpeedModule.range = new Vector2(assetAccessor.getParmFloatValue("rotationBySpeed_range", 0),
													assetAccessor.getParmFloatValue("rotationBySpeed_range", 1));

		// External Forces
		ParticleSystem.ExternalForcesModule externalForcesModule = pSystem.externalForces;

		// Noise
		ParticleSystem.NoiseModule noiseModule = pSystem.noise;

		// Collision
		ParticleSystem.CollisionModule collisionModule = pSystem.collision;

		// Triggers
		ParticleSystem.TriggerModule triggerModule = pSystem.trigger;

		// Sub Emitters
		ParticleSystem.SubEmittersModule subEmittersModule = pSystem.subEmitters;

		// Texture Sheet Animation
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = pSystem.textureSheetAnimation;

		textureSheetAnimationModule.enabled = Convert.ToBoolean(assetAccessor.getParmIntValue("textureSheetAnimation_enabled", 0));

		textureSheetAnimationModule.mode = (ParticleSystemAnimationMode) assetAccessor.getParmIntValue("textureSheetAnimation_mode", 0);

		textureSheetAnimationModule.animation = (ParticleSystemAnimationType) assetAccessor.getParmIntValue("textureSheetAnimation_animation", 0);

		textureSheetAnimationModule.frameOverTime = assetAccessor.getParmFloatValue("textureSheetAnimation_frame", 0);

		textureSheetAnimationModule.startFrame = assetAccessor.getParmIntValue("textureSheetAnimation_startFrame", 0);

		textureSheetAnimationModule.cycleCount = assetAccessor.getParmIntValue("textureSheetAnimation_cycles", 0);

		textureSheetAnimationModule.flipU = assetAccessor.getParmFloatValue("textureSheetAnimation_flipU", 0);

		textureSheetAnimationModule.flipU = assetAccessor.getParmFloatValue("textureSheetAnimation_flipV", 0);

		// Lights
		ParticleSystem.LightsModule lightsModule = pSystem.lights;

		// Trails
		ParticleSystem.TrailModule trailModule = pSystem.trails;

		// Custom Data
		ParticleSystem.CustomDataModule customDataModule = pSystem.customData;

		// Renderer
		// ParticleSystem
	}
}
