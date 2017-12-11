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
	public bool timedUpdates = false;
	public float updateInterval = 5f;

	private ParticleSystem _particleSystem;
	private HoudiniAssetOTL _assetOTL;
	private HoudiniApiAssetAccessor _assetAccessor;

	/// <summary>
	/// If the particle system is marked as dirty, it should be updated 
	/// </summary>
	private bool _isDirty = false;

    void Start () {
		_assetAccessor = HoudiniApiAssetAccessor.getAssetAccessor(gameObject);
		_assetOTL = GetComponent<HoudiniAssetOTL>();
		_particleSystem = GetComponent<ParticleSystem>();
	}

	void OnEnable() {
		_isDirty = true;
	}

	void OnApplicationFocus(bool hasFocus) {		
		if (timedUpdates == false) {
		_isDirty = !hasFocus;
		}
	}

	void Update() {
		if (timedUpdates == true) {
			StartCoroutine(checkForUpdates());
		}

		if (_isDirty) {
			_assetOTL.buildAll();			//	Load asset definition from disk
			InstantiateParticleSystem();	//	Construct particle system from said definition
			_isDirty = false;
			//updateInterval = _assetAccessor.getParmFloatValue("main_duration", 0);
		}
	}

	IEnumerator checkForUpdates() {
		yield return new WaitForSeconds(updateInterval);
		_isDirty = true;
	}

	void InstantiateParticleSystem() {
		if (_particleSystem == null) {
			_particleSystem = gameObject.AddComponent<ParticleSystem>();
		}
		
		_particleSystem.gameObject.SetActive(false);	//	Having the game object disabled while mapping the parameters greatly speeds up the process

		//	Here we go
		MapMainParameters();
		MapEmissionParameters();
		MapShapeParameters();
		MapVelocityOverLifetimeParameters();
		MapLimitVelocityOverLifetimeParameters();
		MapInheritVelocityOverLifetimeParameters();
		MapForceOverLifetimeParameters();
		MapColorOverLifetimeParameters();
		MapColorBySpeedParameters();
		MapSizeOverLifetimeParameters();
		MapSizeBySpeedParameters();
		MapRotationOverLifetimeParameters();
		MapRotationBySpeedParameters();
		MapExternalForcesParameters();
		MapNoiseParameters();
		MapCollisionParameters();
		MapTriggerParameters();
		MapSubEmitterParameters();
		MapTextureSheetAnimationParameters();
		MapLightParamters();
		MapTrailParameters();
		MapCustomDataParameters();
		MapRendererParameters();

		_particleSystem.gameObject.SetActive(true);
		_particleSystem.Play(true);
	}

	/// <summary>
	/// Not used, since I can't figure out how to get it working. Kept for future reference when I'll want to fetch attributes directly, without having to go through parameters first.
	/// </summary>
	void GetDetailAttributes() {
		HAPI_AttributeInfo attributeInfo = new HAPI_AttributeInfo();
		attributeInfo.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		attributeInfo.count = 1;
		attributeInfo.tupleSize = 1;
		attributeInfo.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL;
		// HoudiniGeoAttribute attribute = new HoudiniGeoAttribute();
		//	HAPI_GetAttributeIntData();
		//	HAPI_GetAttributeFloatData();
		//	HAPI_GetAttributeStringData();
	}

	ParticleSystem.MinMaxCurve InterpretStringToCurve(string entry) {
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
				curve.mode = ParticleSystemCurveMode.TwoConstants;
				if(choppedString[1] == "float") {
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

				if(choppedString[1] == "float") {
					curve.curve = GenerateCurve(choppedString);
				}
				break;

			case "randomCurve":
				curve.mode = ParticleSystemCurveMode.TwoCurves;
				curve.curveMultiplier = Convert.ToSingle(choppedString[3]);

				if(choppedString[1] == "float") {
					curve.curveMin = GenerateCurve(choppedString);
					curve.curveMax = GenerateCurve(choppedString, 68);
				}
				break;
		}
		return curve;
	}

	ParticleSystem.MinMaxGradient InterpretStringToGradient(string entry) {
		ParticleSystem.MinMaxGradient curve = new ParticleSystem.MinMaxGradient();

		string parameter = _assetAccessor.getParmStringValue(entry, 0);
		string[] choppedString = parameter.Split(";".ToCharArray());
		string color;
		string[] colorArray;

		switch(choppedString[0]) {
			case "constant":
				curve.mode = ParticleSystemGradientMode.Color;

				color = choppedString[2];
				color = color.Replace("{", "");
				color = color.Replace("}", "");
				colorArray = color.Split(",".ToCharArray());

				curve.color = new Color(Convert.ToSingle(colorArray[0]), 
										Convert.ToSingle(colorArray[1]), 
										Convert.ToSingle(colorArray[2]), 
										Convert.ToSingle(colorArray[3]));
				break;
			
			case "randomConstant":
				break;

			case "gradient":
				curve.mode = ParticleSystemGradientMode.Gradient;
				curve.gradient = GenerateGradient(choppedString);
				break;

			case "randomGradient":
				break; 
		}
		return curve;
	}

	///	<Summary>
	///	Reads a list of values and returns a float curve. The number of samples decide the resolution of the resulting curve. We need the offset to be able to handle curve pairs (such as the "random between curves" mode).
	///	</summary>
	private AnimationCurve GenerateCurve(string[] parameter, int offset = 4) {
		AnimationCurve curve = new AnimationCurve();
		int samples = Convert.ToInt32(parameter[2]);

		for (int i = 0; i < samples; i++) {
			float position = (float) i / (float) samples;
			float value = Convert.ToSingle(parameter[i+offset]);
			curve.AddKey(position, value);
		}
		return curve;
	}

	///	<Summary>
	///	Reads a list of values and returns a gradient. The number of samples decide the resolution of the resulting gradient (although this should in most cases be kept at 8). We need the offset to be able to handle gradient pairs (such as the "random between gradients" mode).
	///	</summary>
	private Gradient GenerateGradient(string[] parameter, int offset = 4) {
		Gradient gradient = new Gradient();
		int samples = Convert.ToInt32(parameter[2]);
		gradient.mode = (GradientMode) Convert.ToInt32(parameter[3]);
		GradientColorKey[] colorKeys = new GradientColorKey[samples];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[samples];
		
		for (int i = 0; i < samples; i++) {
			float position = (float) i / (samples - 1.0f);
			string color = parameter[i+offset];

			color = color.Replace("{", "");
			color = color.Replace("}", "");
			string[] colorArray = color.Split(",".ToCharArray());

			Color currentColor = new Color(Convert.ToSingle(colorArray[0]), 
											Convert.ToSingle(colorArray[1]), 
											Convert.ToSingle(colorArray[2]), 
											Convert.ToSingle(colorArray[3]));

			GradientColorKey colorKey = new GradientColorKey(currentColor, position);
			GradientAlphaKey alphaKey = new GradientAlphaKey(currentColor.a, position);
			colorKeys[i] = colorKey;
			alphaKeys[i] = alphaKey;
		}

		gradient.SetKeys(colorKeys,alphaKeys);
		return gradient;
	}

    private void MapMainParameters() {
		ParticleSystem.MainModule mainModule = _particleSystem.main;

		try {
			mainModule.duration = _assetAccessor.getParmFloatValue("main_duration", 0);
		}
		catch (HoudiniErrorNotFound e) {
			Debug.LogException(e);
			return;
		}

		mainModule.loop = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_looping", 0));

		mainModule.prewarm = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_prewarm", 0));

		mainModule.startDelay = _assetAccessor.getParmFloatValue("main_startDelay", 0);

		mainModule.startLifetime = InterpretStringToCurve("main_startLifetime");

		mainModule.startSpeed = InterpretStringToCurve("main_startSpeed");

		mainModule.startSize3D = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_3DStartSize", 0));

		mainModule.startSize = InterpretStringToCurve("main_startSize");

		mainModule.startSizeX = InterpretStringToCurve("main_startSize_x");

		mainModule.startSizeY = InterpretStringToCurve("main_startSize_y");

		mainModule.startSizeZ = InterpretStringToCurve("main_startSize_z");

		mainModule.startRotation3D = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_3DStartRotation", 0));

		mainModule.startRotation = InterpretStringToCurve("main_startRotation");

		mainModule.startRotationX = InterpretStringToCurve("main_startRotation_x");

		mainModule.startRotationY = InterpretStringToCurve("main_startRotation_y");

		mainModule.startRotationZ = InterpretStringToCurve("main_startRotation_z");

		mainModule.randomizeRotationDirection = _assetAccessor.getParmFloatValue("main_rotationVariance", 0);

		mainModule.startColor = InterpretStringToGradient("main_startColor");

		mainModule.gravityModifier = InterpretStringToCurve("main_gravityModifier");

		mainModule.simulationSpace = (ParticleSystemSimulationSpace) _assetAccessor.getParmIntValue("main_simulationSpace", 0);

		mainModule.simulationSpeed = _assetAccessor.getParmFloatValue("main_simulationSpeed", 0);

		mainModule.useUnscaledTime = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_deltaTime", 0));

		mainModule.scalingMode = (ParticleSystemScalingMode) _assetAccessor.getParmIntValue("main_scalingMode", 0);

		_assetAccessor.getParmType("main_deltaTime");

		mainModule.playOnAwake = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_playOnAwake", 0));

		mainModule.emitterVelocityMode = (ParticleSystemEmitterVelocityMode) _assetAccessor.getParmIntValue("main_emitterVelocity", 0);

		mainModule.maxParticles = _assetAccessor.getParmIntValue("main_maxParticles", 0);

		_particleSystem.useAutoRandomSeed = Convert.ToBoolean(_assetAccessor.getParmIntValue("main_autoRandomSeed",0));
	}

	private void MapEmissionParameters() {

		ParticleSystem.EmissionModule emissionModule = _particleSystem.emission;
		
		try {
			emissionModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("emission_enabled", 0));
		}
		catch (HoudiniErrorNotFound) {
			Debug.LogWarning("EmissionModule not found in VOP");
			emissionModule.enabled = false;
			return;
		}

		emissionModule.rateOverTime = InterpretStringToCurve("emission_rateOverTime");

		emissionModule.rateOverDistance = InterpretStringToCurve("emission_rateOverDistance");

		int numBursts = _assetAccessor.getParmSize("emission_bursts");

		for (int i = 0; i < numBursts; i++) {
			ParticleSystem.Burst burst = new ParticleSystem.Burst();
			burst.time = Convert.ToSingle(_assetAccessor.getParmStringValue("emission_bursts", 1 * i));
			burst.minCount = Convert.ToInt16(_assetAccessor.getParmStringValue("emission_bursts", 2 * i));
			burst.maxCount = Convert.ToInt16(_assetAccessor.getParmStringValue("emission_bursts", 3 * i));
			burst.cycleCount = Convert.ToInt16(_assetAccessor.getParmStringValue("emission_bursts", 4 * i));
			burst.repeatInterval = Convert.ToSingle(_assetAccessor.getParmStringValue("emission_bursts", 5 * i));

		}
	}

	private void MapShapeParameters() {
		ParticleSystem.ShapeModule shapeModule = _particleSystem.shape;

		try {
			shapeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("shape_enabled", 0));
		}
		catch (HoudiniErrorNotFound) {
			Debug.LogWarning("ShapeModule not found in VOP");
			shapeModule.enabled = false;
			return;
		}

		shapeModule.shapeType = (ParticleSystemShapeType) _assetAccessor.getParmIntValue("shape_shape", 0);

		shapeModule.radius = _assetAccessor.getParmFloatValue("shape_radius", 0);
		
		shapeModule.radiusThickness = _assetAccessor.getParmFloatValue("shape_radius_thickness", 0);
	
		
		shapeModule.alignToDirection = Convert.ToBoolean(_assetAccessor.getParmIntValue("shape_alignToDirection", 0));
		
		shapeModule.randomDirectionAmount = _assetAccessor.getParmFloatValue("shape_randomizeDirection",0);

		shapeModule.sphericalDirectionAmount = _assetAccessor.getParmFloatValue("shape_spherizeDirection", 0);

		shapeModule.randomPositionAmount = _assetAccessor.getParmFloatValue("shape_randomizePosition", 0);

		shapeModule.angle = _assetAccessor.getParmFloatValue("shape_angle", 0);

		shapeModule.arc = _assetAccessor.getParmFloatValue("shape_arc", 0);

		shapeModule.arcMode = (ParticleSystemShapeMultiModeValue) _assetAccessor.getParmIntValue("shape_arcMode", 0);

		shapeModule.arcSpread = _assetAccessor.getParmFloatValue("shape_arcSpread", 0);

		shapeModule.arcSpeed = InterpretStringToCurve("shape_arcSpeed");

		shapeModule.length = _assetAccessor.getParmFloatValue("shape_length", 0);

		shapeModule.donutRadius = _assetAccessor.getParmFloatValue("shape_donutRadius", 0);

		shapeModule.boxThickness = new Vector3(_assetAccessor.getParmFloatValue("shape_boxThickness", 0),
												_assetAccessor.getParmFloatValue("shape_boxThickness", 1),
												_assetAccessor.getParmFloatValue("shape_boxThickness", 2));

		shapeModule.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(_assetAccessor.getParmStringValue("shape_mesh", 0));

		shapeModule.meshRenderer = AssetDatabase.LoadAssetAtPath<MeshRenderer>(_assetAccessor.getParmStringValue("shape_meshRenderer", 0));

		shapeModule.skinnedMeshRenderer = AssetDatabase.LoadAssetAtPath<SkinnedMeshRenderer>(_assetAccessor.getParmStringValue("shape_skinnedMeshRenderer", 0));

		shapeModule.meshShapeType = (ParticleSystemMeshShapeType) _assetAccessor.getParmIntValue("shape_meshShapeType", 0);

		shapeModule.useMeshMaterialIndex = Convert.ToBoolean(_assetAccessor.getParmIntValue("shape_singleMaterial", 0));

		shapeModule.meshMaterialIndex = _assetAccessor.getParmIntValue("shape_meshMaterialIndex",0);

		shapeModule.normalOffset = _assetAccessor.getParmFloatValue("shape_normalOffset", 0);

		shapeModule.useMeshColors = Convert.ToBoolean(_assetAccessor.getParmIntValue("shape_useMeshColors",0));

		shapeModule.position = new Vector3(_assetAccessor.getParmFloatValue("shape_position", 0), 
											_assetAccessor.getParmFloatValue("shape_position", 1),
											_assetAccessor.getParmFloatValue("shape_position", 2));

		shapeModule.rotation = new Vector3(_assetAccessor.getParmFloatValue("shape_rotation", 0), 
											_assetAccessor.getParmFloatValue("shape_rotation", 1),
											_assetAccessor.getParmFloatValue("shape_rotation", 2));
		
		shapeModule.scale = new Vector3(_assetAccessor.getParmFloatValue("shape_scale", 0), 
											_assetAccessor.getParmFloatValue("shape_scale", 1),
											_assetAccessor.getParmFloatValue("shape_scale", 2));
    }

    private void MapRendererParameters() {
		ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();

		try {
			renderer.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("renderer_enabled", 0));
		}
		catch (HoudiniErrorNotFound) {
			Debug.LogWarning("RendererModule not found in VOP");
			renderer.enabled = false;
			return;
		}

		renderer.alignment = (ParticleSystemRenderSpace) _assetAccessor.getParmIntValue("renderer_Alignment", 0);
		
		renderer.shadowCastingMode = (UnityEngine.Rendering.ShadowCastingMode) _assetAccessor.getParmIntValue("renderer_castShadows", 0);
		
		renderer.renderMode = (ParticleSystemRenderMode) _assetAccessor.getParmIntValue("renderer_mode", 0);
		
		renderer.lightProbeUsage = (UnityEngine.Rendering.LightProbeUsage)_assetAccessor.getParmIntValue("renderer_lightProbes", 0);
		
		renderer.maskInteraction = (SpriteMaskInteraction) _assetAccessor.getParmIntValue("renderer_masking", 0);
		
		renderer.maxParticleSize = _assetAccessor.getParmFloatValue("renderer_maxParticleSize", 0);
		
		renderer.minParticleSize = _assetAccessor.getParmFloatValue("renderer_minParticleSize", 0);
		
		renderer.motionVectorGenerationMode = (MotionVectorGenerationMode) _assetAccessor.getParmIntValue("renderer_motionVectors", 0);
		
		renderer.reflectionProbeUsage = (UnityEngine.Rendering.ReflectionProbeUsage)_assetAccessor.getParmIntValue("renderer_reflectionProbes", 0);
		
		renderer.receiveShadows = Convert.ToBoolean(_assetAccessor.getParmIntValue("renderer_receiveShadows", 0));
		
		renderer.material = AssetDatabase.LoadAssetAtPath<Material>(_assetAccessor.getParmStringValue("renderer_material", 0));
		
		renderer.trailMaterial = AssetDatabase.LoadAssetAtPath<Material>(_assetAccessor.getParmStringValue("renderer_trailMaterial", 0));
		
		renderer.pivot = new Vector3(_assetAccessor.getParmFloatValue("renderer_pivot", 0),
										_assetAccessor.getParmFloatValue("renderer_pivot", 1),
										_assetAccessor.getParmFloatValue("renderer_pivot", 2));
		
		renderer.sortMode = (ParticleSystemSortMode) _assetAccessor.getParmIntValue("renderer_sortMode", 0);
		
		renderer.sortingFudge = _assetAccessor.getParmFloatValue("renderer_sortingFudge", 0);
		
		renderer.sortingOrder = _assetAccessor.getParmIntValue("renderer_orderInLayer", 0);
		
		renderer.normalDirection = _assetAccessor.getParmFloatValue("renderer_normalDirection", 0);
    }

    private void MapCustomDataParameters() {
		ParticleSystem.CustomDataModule customDataModule = _particleSystem.customData;

		try {
			customDataModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("customData_enabled", 0));
		}
		catch (HoudiniErrorNotFound) {
			Debug.LogWarning("customDataModule not found in VOP");
			customDataModule.enabled = false;
			return;
		}

		customDataModule.SetMode(ParticleSystemCustomData.Custom1,ParticleSystemCustomDataMode.Vector);
		
		customDataModule.SetMode(ParticleSystemCustomData.Custom2,ParticleSystemCustomDataMode.Vector);
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom1, 0, _assetAccessor.getParmFloatValue("customData_1", 0));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom1, 1, _assetAccessor.getParmFloatValue("customData_1", 1));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom1, 2, _assetAccessor.getParmFloatValue("customData_1", 2));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom1, 3, _assetAccessor.getParmFloatValue("customData_1", 3));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom2, 0, _assetAccessor.getParmFloatValue("customData_2", 0));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom2, 1, _assetAccessor.getParmFloatValue("customData_2", 1));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom2, 2, _assetAccessor.getParmFloatValue("customData_2", 2));
		
		customDataModule.SetVector (ParticleSystemCustomData.Custom2, 3, _assetAccessor.getParmFloatValue("customData_2", 3));
    }

    private void MapTrailParameters() {
		ParticleSystem.TrailModule trailModule = _particleSystem.trails;

		try {
			trailModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_enabled", 0));
		}
		catch (HoudiniErrorNotFound) {
			Debug.LogWarning("Trail not found in VOP");
			trailModule.enabled = false;
			return;
		}

		trailModule.colorOverTrail = InterpretStringToGradient("trails_colorOverTrail");

		trailModule.colorOverLifetime = InterpretStringToGradient("trails_colorOverLifetime");

		trailModule.dieWithParticles = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_dieWithParticles", 0));

		trailModule.generateLightingData = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_generateLightingData", 0));

		trailModule.inheritParticleColor = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_inheritParticleColor", 0));

		trailModule.lifetime = InterpretStringToCurve("trails_lifetime");

		trailModule.minVertexDistance = _assetAccessor.getParmFloatValue("trails_minimumVertexDistance", 0);

		trailModule.ratio = _assetAccessor.getParmFloatValue("trails_ratio", 0);

		trailModule.sizeAffectsLifetime = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_sizeAffectsLifetime", 0));

		trailModule.sizeAffectsWidth = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_sizeAffectsWidth", 0));

		trailModule.textureMode = (ParticleSystemTrailTextureMode) _assetAccessor.getParmIntValue("trails_textureMode", 0);

		trailModule.widthOverTrail = InterpretStringToCurve("trails_widthOverTrail");

		trailModule.worldSpace = Convert.ToBoolean(_assetAccessor.getParmIntValue("trails_worldSpace", 0));
    }

    private void MapLightParamters() {
		ParticleSystem.LightsModule lightsModule = _particleSystem.lights;
    }

    private void MapTextureSheetAnimationParameters() {
		ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = _particleSystem.textureSheetAnimation;

		try {
			textureSheetAnimationModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("textureSheetAnimation_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("TextureSheetAnimationModule not found in VOP");
			textureSheetAnimationModule.enabled = false;
			return;
		}

		textureSheetAnimationModule.mode = (ParticleSystemAnimationMode) _assetAccessor.getParmIntValue("textureSheetAnimation_mode", 0);

		textureSheetAnimationModule.animation = (ParticleSystemAnimationType) _assetAccessor.getParmIntValue("textureSheetAnimation_animation", 0);

		textureSheetAnimationModule.frameOverTime = InterpretStringToCurve("textureSheetAnimation_frame");

		textureSheetAnimationModule.startFrame = InterpretStringToCurve("textureSheetAnimation_startFrame");

		textureSheetAnimationModule.cycleCount = _assetAccessor.getParmIntValue("textureSheetAnimation_cycles", 0);

		textureSheetAnimationModule.flipU = _assetAccessor.getParmFloatValue("textureSheetAnimation_flipU", 0);

		textureSheetAnimationModule.flipU = _assetAccessor.getParmFloatValue("textureSheetAnimation_flipV", 0);
    }

    private void MapSubEmitterParameters() {
		ParticleSystem.SubEmittersModule subEmittersModule = _particleSystem.subEmitters;
    }

    private void MapTriggerParameters() {
		ParticleSystem.TriggerModule triggerModule = _particleSystem.trigger;
    }

    private void MapCollisionParameters() {    
		ParticleSystem.CollisionModule collisionModule = _particleSystem.collision;

		try {
			collisionModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("collision_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("CollisionModule not found in VOP");
			collisionModule.enabled = false;
			return;
		}

		collisionModule.bounce = InterpretStringToCurve("collision_bounce");

		collisionModule.colliderForce = _assetAccessor.getParmFloatValue("collision_colliderForce", 0);

		collisionModule.dampen = InterpretStringToCurve("collision_dampen");

		collisionModule.enableDynamicColliders = Convert.ToBoolean(_assetAccessor.getParmIntValue("collision_enableDynamicColliders", 0));

		collisionModule.lifetimeLoss = InterpretStringToCurve("collision_lifetimeLoss");

		collisionModule.maxKillSpeed = _assetAccessor.getParmFloatValue("collision_maxKillSpeed", 0);

		collisionModule.minKillSpeed = _assetAccessor.getParmFloatValue("collision_minKillSpeed", 0);

		collisionModule.mode = (ParticleSystemCollisionMode) _assetAccessor.getParmIntValue("collision_mode", 0);

		collisionModule.multiplyColliderForceByCollisionAngle = Convert.ToBoolean(_assetAccessor.getParmIntValue("collision_multiplyByCollisionAngle", 0));

		collisionModule.multiplyColliderForceByParticleSpeed = Convert.ToBoolean(_assetAccessor.getParmIntValue("collision_multiplyByParticleSpeed", 0));

		collisionModule.multiplyColliderForceByParticleSpeed = Convert.ToBoolean(_assetAccessor.getParmIntValue("collision_multiplyByParticleSize", 0));

		collisionModule.quality = (ParticleSystemCollisionQuality) _assetAccessor.getParmIntValue("collision_quality", 0);

		collisionModule.radiusScale = _assetAccessor.getParmFloatValue("collision_radiusScale", 0);

		collisionModule.sendCollisionMessages = Convert.ToBoolean(_assetAccessor.getParmIntValue("collision_sendCollisionMessages", 0));

		collisionModule.type = (ParticleSystemCollisionType) _assetAccessor.getParmIntValue("collision_type", 0);
    }

    private void MapNoiseParameters() {
		ParticleSystem.NoiseModule noiseModule = _particleSystem.noise;

		try {
			noiseModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("noise_enabled", 0));
		}
		catch (HoudiniErrorNotFound) { 
			Debug.LogWarning("NoiseModule not found in VOP");
			noiseModule.enabled = false;
			return;
		}

		noiseModule.frequency = _assetAccessor.getParmFloatValue("noise_frequency", 0);

		noiseModule.octaveMultiplier = _assetAccessor.getParmFloatValue("noise_octaveMultiplier",0);

		noiseModule.octaveCount = _assetAccessor.getParmIntValue("noise_octaves",0);

		noiseModule.octaveScale = _assetAccessor.getParmFloatValue("noise_octaveScale",0);

		noiseModule.quality = (ParticleSystemNoiseQuality) _assetAccessor.getParmIntValue("noise_quality",0);

		noiseModule.remapEnabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("noise_remap",0));

		noiseModule.remap = InterpretStringToCurve("noise_remapCurve");
		
		noiseModule.positionAmount = InterpretStringToCurve("noise_positionAmount");
		
		noiseModule.rotationAmount = InterpretStringToCurve("noise_rotationAmount");

		noiseModule.sizeAmount = InterpretStringToCurve("noise_scaleAmount");

		noiseModule.scrollSpeed = InterpretStringToCurve("noise_scrollSpeed");

		noiseModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("noise_separateAxes", 0));
		noiseModule.strength = InterpretStringToCurve("noise_strength");

		noiseModule.strengthX = InterpretStringToCurve("noise_strength_x");

		noiseModule.strengthY = InterpretStringToCurve("noise_strength_y");

		noiseModule.strengthZ = InterpretStringToCurve("noise_strength_z");
    }

    private void MapExternalForcesParameters() {

		ParticleSystem.ExternalForcesModule externalForcesModule = _particleSystem.externalForces;

		try {
			externalForcesModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("externalForces_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("ExternalForcesModule not found in VOP");
			externalForcesModule.enabled = false;
			return;
		}

		externalForcesModule.multiplier = _assetAccessor.getParmFloatValue("externalForces_multiplier",0);
    }

    private void MapRotationBySpeedParameters() {
		ParticleSystem.RotationBySpeedModule rotationBySpeedModule = _particleSystem.rotationBySpeed;

		try {
			rotationBySpeedModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationBySpeed_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("RotationBySpeedModule not found in VOP");
			rotationBySpeedModule.enabled = false;
			return;
		}

		rotationBySpeedModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationBySpeed_separateAxes", 0)); 

		rotationBySpeedModule.x = InterpretStringToCurve("rotationBySpeed_angularVelocity_x");
		rotationBySpeedModule.y = InterpretStringToCurve("rotationBySpeed_angularVelocity_y");
		rotationBySpeedModule.z = InterpretStringToCurve("rotationBySpeed_angularVelocity_z");

		rotationBySpeedModule.range = new Vector2(_assetAccessor.getParmFloatValue("rotationBySpeed_range", 0),
													_assetAccessor.getParmFloatValue("rotationBySpeed_range", 1));
    }

    private void MapRotationOverLifetimeParameters() {
		ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = _particleSystem.rotationOverLifetime;

		try {
			rotationOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationOverLifetime_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("RotationOverLifetimeModule not found in VOP");
			rotationOverLifetimeModule.enabled = false;
			return;
		}

		rotationOverLifetimeModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("rotationOverLifetime_separateAxes", 0));

		rotationOverLifetimeModule.x = InterpretStringToCurve("rotationOverLifetime_angularVelocity_x");
		rotationOverLifetimeModule.y = InterpretStringToCurve("rotationOverLifetime_angularVelocity_y");
		rotationOverLifetimeModule.z = InterpretStringToCurve("rotationOverLifetime_angularVelocity_z");
    }

    private void MapSizeBySpeedParameters() {
		ParticleSystem.SizeBySpeedModule sizeBySpeedModule = _particleSystem.sizeBySpeed;

		try {
			sizeBySpeedModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeBySpeed_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("SizeBySpeedModule not found in VOP");
			sizeBySpeedModule.enabled = false;
			return;
		}

		sizeBySpeedModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeBySpeed_separateAxes", 0));

		sizeBySpeedModule.size = InterpretStringToCurve("sizeBySpeed_size");

		sizeBySpeedModule.x = InterpretStringToCurve("sizeBySpeed_size_x");
		sizeBySpeedModule.y = InterpretStringToCurve("sizeBySpeed_size_y");
		sizeBySpeedModule.z = InterpretStringToCurve("sizeBySpeed_size_z");

		sizeBySpeedModule.range = new Vector2(_assetAccessor.getParmFloatValue("sizeBySpeed_range", 0),
												_assetAccessor.getParmFloatValue("sizeBySpeed_range", 1));
    }

    private void MapSizeOverLifetimeParameters() {
		ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = _particleSystem.sizeOverLifetime;

		try {
			sizeOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeOverLifetime_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("SizeOverLifetimeModule not found in VOP");
			sizeOverLifetimeModule.enabled = false;
			return;
		}

		sizeOverLifetimeModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("sizeOverLifetime_separateAxes", 0));

		sizeOverLifetimeModule.size = InterpretStringToCurve("sizeOverLifetime_size");

		sizeOverLifetimeModule.x = InterpretStringToCurve("sizeOverLifetime_size_x");
		sizeOverLifetimeModule.y = InterpretStringToCurve("sizeOverLifetime_size_y");
		sizeOverLifetimeModule.z = InterpretStringToCurve("sizeOverLifetime_size_z");
    }

    private void MapColorBySpeedParameters() {
		ParticleSystem.ColorBySpeedModule colorBySpeedModule = _particleSystem.colorBySpeed;

		try {
			colorBySpeedModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("colorBySpeed_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("ColorBySpeedModule not found in VOP");
			colorBySpeedModule.enabled = false;
			return;
		}

		colorBySpeedModule.color = InterpretStringToGradient("colorBySpeed_color");

		colorBySpeedModule.range = new Vector2(_assetAccessor.getParmFloatValue("colorBySpeed_range", 0),
												_assetAccessor.getParmFloatValue("colorBySpeed_range", 1));
    }

    private void MapColorOverLifetimeParameters() {
		ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = _particleSystem.colorOverLifetime;

		try {
			colorOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("colorOverLifetime_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("ColorOverLifetimeModule not found in VOP");
			colorOverLifetimeModule.enabled = false;
			return;
		}

		colorOverLifetimeModule.color = InterpretStringToGradient("colorOverLifetime_color");
    }

    private void MapForceOverLifetimeParameters() {
		ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = _particleSystem.forceOverLifetime;

		try {
			forceOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("forceOverLifetime_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("ForceOverLifetimeModule not found in VOP");
			forceOverLifetimeModule.enabled = false;
			return;
		}

		forceOverLifetimeModule.x = InterpretStringToCurve("forceOverLifetime_force_x");
		forceOverLifetimeModule.y = InterpretStringToCurve("forceOverLifetime_force_y");
		forceOverLifetimeModule.z = InterpretStringToCurve("forceOverLifetime_force_z");

		forceOverLifetimeModule.randomized = Convert.ToBoolean(_assetAccessor.getParmIntValue("forceOverLifetime_randomized", 0));
    }

    private void MapInheritVelocityOverLifetimeParameters() {
		ParticleSystem.InheritVelocityModule inheritVelocityModule = _particleSystem.inheritVelocity;

		try {
			inheritVelocityModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("inheritVelocity_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("InheritVelocityOverLifetimeModule not found in VOP");
			inheritVelocityModule.enabled = false;
			return;
		}

		inheritVelocityModule.mode = (ParticleSystemInheritVelocityMode) _assetAccessor.getParmIntValue("inheritVelocity_mode", 0);

		inheritVelocityModule.curve = InterpretStringToCurve("inheritVelocity_multiplier");
    }

    private void MapLimitVelocityOverLifetimeParameters() {
		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = _particleSystem.limitVelocityOverLifetime;

		try {
			limitVelocityOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("limitVelocityOverLifetime_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("LimitVelocityOverLifetime not found in VOP");
			limitVelocityOverLifetimeModule.enabled = false;
			return;
		}

		limitVelocityOverLifetimeModule.separateAxes = Convert.ToBoolean(_assetAccessor.getParmIntValue("limitVelocityOverLifetime_separateAxes", 0));

		limitVelocityOverLifetimeModule.limit = InterpretStringToCurve("limitVelocityOverLifetime_speed");

		limitVelocityOverLifetimeModule.limitX = InterpretStringToCurve("limitVelocityOverLifetime_speed_x");
		limitVelocityOverLifetimeModule.limitY = InterpretStringToCurve("limitVelocityOverLifetime_speed_y");
		limitVelocityOverLifetimeModule.limitZ = InterpretStringToCurve("limitVelocityOverLifetime_speed_z");

		limitVelocityOverLifetimeModule.dampen = _assetAccessor.getParmFloatValue("limitVelocityOverLifetime_dampen", 0);
    }

    private void MapVelocityOverLifetimeParameters() {
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = _particleSystem.velocityOverLifetime;

		try {
			velocityOverLifetimeModule.enabled = Convert.ToBoolean(_assetAccessor.getParmIntValue("velocityOverLifetime_enabled", 0));
		}
		catch (HoudiniError) {
			Debug.LogWarning("VelocityOverLifetimeModule not found in VOP");
			velocityOverLifetimeModule.enabled = false;
			return;
		}

		velocityOverLifetimeModule.x = InterpretStringToCurve("velocityOverLifetime_velocity_x");
		velocityOverLifetimeModule.y = InterpretStringToCurve("velocityOverLifetime_velocity_y");
		velocityOverLifetimeModule.z = InterpretStringToCurve("velocityOverLifetime_velocity_z");
    }
}
