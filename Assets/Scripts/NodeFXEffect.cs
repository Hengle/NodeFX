using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;


namespace NodeFX {
    [ExecuteInEditMode]
	public class NodeFXEffect : MonoBehaviour {

		public TextAsset effectDefinition;
		public string path;
        public string source;

		public bool enableAutomaticRefresh  = true;
        public bool refreshAtInterval       = false;
		public bool refreshOnFocus          = true;
        public bool refreshOnFileChange     = true;
        public float updateInterval         = 1f;
        private bool _isDirty = false;
        private FileSystemWatcher _fileSystemWatcher;

		private XmlDocument doc;
		private ParticleSystem _particleSystem;

        private const string DEFAULT_EFFECT_PATH = "Assets/Effects/Definitions/DefaultEffect.xml";

        void OnEnable() {
            Application.runInBackground = true;     // Enable this so that the editor updates even when not in focus

            if (effectDefinition == null) {
                LoadDefaultDefinition();
            }
        }

        /// <summary>
        /// Attempts to load whatever effect is defined by DEFAULT_EFFECT_PATH
        /// </summary>
        private void LoadDefaultDefinition()
        {
            effectDefinition = AssetDatabase.LoadAssetAtPath<TextAsset>(DEFAULT_EFFECT_PATH);
            
            if (effectDefinition != null) {
                Refresh();
            } 
        }

        void Update() {
            if (_isDirty == true) {
                _isDirty = false;
                Refresh();
            }

            if (refreshAtInterval == true) {
			    StartCoroutine(checkForUpdates());
            }

            if (_fileSystemWatcher == null && effectDefinition != null && String.IsNullOrEmpty(path) == false) {
                _fileSystemWatcher = NodeFXUtilities.CreateFileWatcher(path, effectDefinition.name);
            }

            if (_fileSystemWatcher != null) {
                _fileSystemWatcher.EnableRaisingEvents = refreshOnFileChange;
            }
        }

        public void Refresh() {
			Debug.Log("Effect: Refreshing");
            path = AssetDatabase.GetAssetOrScenePath(effectDefinition);
			if(!string.IsNullOrEmpty(path)) {
                try {
                    LoadXML(path);
                }
                catch (IOException) {
                    //  This will occasionally occur when the file is attemping to be read and written to at the same time.
                    return;
                }
                InstantiateParticleSystem();
			}
        }

		private void LoadXML(string path) {
			doc = new XmlDocument();
			doc.Load(path);
            source = NodeFXUtilities.GetSource(doc);
		}

        private void InstantiateParticleSystem() {

			DeleteOldParticleSystems();
            
			for (int i = 0; i < NodeFXUtilities.GetEmitterCount(doc); i++) {
                
                ParticleSystem pSystem;
                string emitterName = effectDefinition.name + "_" + GetStringParam(i, "main_emitterName");

                //  For the first emitter we want to add the particlesystem component to our root gameobject. For all subsequent emitters we want to create child gameobjects.
                if (i == 0) {
                    pSystem = gameObject.AddComponent<ParticleSystem>();
                    gameObject.name = emitterName;
                } else {
                    GameObject Emitter = new GameObject(emitterName);
                    Emitter.transform.parent = transform;
				    pSystem = Emitter.AddComponent<ParticleSystem>();
                }

                pSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pSystem.gameObject.SetActive(false);
                
				MapMainParameters                       (pSystem, i);
				MapEmissionParameters                   (pSystem, i);
				MapShapeParameters                      (pSystem, i);
				MapVelocityOverLifetimeParameters       (pSystem, i);
				MapLimitVelocityOverLifetimeParameters  (pSystem, i);
				MapInheritVelocityParameters            (pSystem, i);
				MapForceOverLifetimeParameters          (pSystem, i);
				MapColorOverLifetimeParameters          (pSystem, i);
				MapColorBySpeedParameters               (pSystem, i);
				MapSizeOverLifetimeParameters           (pSystem, i);
				MapSizeBySpeedParameters                (pSystem, i);
                MapRotationOverLifetimeParameters       (pSystem, i);
				MapRotationBySpeedParameters            (pSystem, i);
				MapExternalForcesParameters             (pSystem, i);
				MapNoiseParameters                      (pSystem, i);
				MapCollisionParameters                  (pSystem, i);
				MapTriggerParameters                    (pSystem, i);
				MapSubEmitterParameters                 (pSystem, i);
				MapTextureSheetAnimationParameters      (pSystem, i);
				MapLightParameters                      (pSystem, i);
				MapTrailParameters                      (pSystem, i);
				MapCustomDataParameters                 (pSystem, i);
				MapRendererParameters                   (pSystem, i);

                pSystem.gameObject.SetActive(true);
                
				pSystem.Play(true);
			}
		}

        /// <summary>
        /// Since we're only allowed one instance of every component, we must delete the old particlesystem components before adding new ones.
        /// </summary>
        private void DeleteOldParticleSystems() {
            if (GetComponent<ParticleSystem>() != null) {
                GetComponent<ParticleSystem>().Stop();
                GetComponent<ParticleSystem>().gameObject.SetActive(false);
                DestroyImmediate(GetComponent<ParticleSystem>());
            }

            foreach (ParticleSystem ps in gameObject.GetComponentsInChildren<ParticleSystem>()) {
                if (ps.gameObject != gameObject) {
                    DestroyImmediate(ps.gameObject);
                }
            }
        }

        private void MapMainParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.MainModule mainModule = pSystem.main;
            mainModule.stopAction                   = (ParticleSystemStopAction)            GetIntParam(i, "main_stopAction");
            mainModule.emitterVelocityMode          = (ParticleSystemEmitterVelocityMode)   GetIntParam(i, "main_emitterVelocity");
            mainModule.scalingMode                  = (ParticleSystemScalingMode)           GetIntParam(i, "main_scalingMode");
            mainModule.simulationSpace              = (ParticleSystemSimulationSpace)       GetIntParam(i, "main_simulationSpace");
            mainModule.maxParticles                 = GetIntParam(i, "main_maxParticles");
            mainModule.loop                         = GetBoolParam(i, "main_looping");
            mainModule.prewarm                      = GetBoolParam(i, "main_prewarm");
            mainModule.startSize3D                  = GetBoolParam(i, "main_3DStartSize");
            mainModule.startRotation3D              = GetBoolParam(i, "main_3DStartRotation");
            mainModule.useUnscaledTime              = GetBoolParam(i, "main_deltaTime");
            mainModule.playOnAwake                  = GetBoolParam(i, "main_playOnAwake");
            pSystem.useAutoRandomSeed               = GetBoolParam(i, "main_autoRandomSeed");
            mainModule.flipRotation                 = GetFloatParam(i, "main_rotationVariance");
            mainModule.simulationSpeed              = GetFloatParam(i, "main_simulationSpeed");
            mainModule.duration                     = GetFloatParam(i, "main_duration");
            mainModule.startDelay                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startDelay"));
            mainModule.startLifetime                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startLifetime"));
            mainModule.startSpeed                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSpeed"));
            mainModule.startSize                    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize"));
            mainModule.startSizeX                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize_x"));
            mainModule.startSizeY                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize_y"));
            mainModule.startSizeZ                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize_z"));
            mainModule.startRotation                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation"));
            mainModule.startRotationX               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation_x"));
            mainModule.startRotationY               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation_y"));
            mainModule.startRotationZ               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation_z"));
            mainModule.gravityModifier              = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_gravityModifier"));
            mainModule.startColor                   = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "main_startColor"));
            pSystem.randomSeed = 0;
        }

        private void MapEmissionParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.EmissionModule emissionModule = pSystem.emission;

            try {
                emissionModule.enabled              = GetBoolParam(i, "emission_enabled");
            }
            catch (NullReferenceException) {
                emissionModule.enabled = false;
                return;
            }

            emissionModule.rateOverTime             = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "emission_rateOverTime"));
            emissionModule.rateOverDistance         = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "emission_rateOverDistance"));
            emissionModule.SetBursts(NodeFXUtilities.InterpretStringToBurst(GetStringParam(i, "emission_bursts")));
            return;
        }

        private void MapShapeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ShapeModule shapeModule = pSystem.shape;

            try {
                shapeModule.enabled                     = GetBoolParam(i, "shape_enabled");
            }
            catch (NullReferenceException) {
                shapeModule.enabled = false;
                return;
            }

            shapeModule.meshShapeType               = (ParticleSystemMeshShapeType)         GetIntParam(i, "shape_meshShapeType");
            shapeModule.shapeType                   = (ParticleSystemShapeType)             GetIntParam(i, "shape_shape");
            shapeModule.arcMode                     = (ParticleSystemShapeMultiModeValue)   GetIntParam(i, "shape_arcMode");
            shapeModule.meshMaterialIndex           = GetIntParam(i, "shape_meshMaterialIndex");
            shapeModule.useMeshMaterialIndex        = GetBoolParam(i, "shape_useMeshMaterialIndex");
            shapeModule.useMeshColors               = GetBoolParam(i, "shape_useMeshColors");
            shapeModule.alignToDirection            = GetBoolParam(i, "shape_alignToDirection");
            shapeModule.radius                      = GetFloatParam(i, "shape_radius");
            shapeModule.radiusThickness             = GetFloatParam(i, "shape_radiusThickness");
            shapeModule.arc                         = GetFloatParam(i, "shape_arc");
            shapeModule.arcSpread                   = GetFloatParam(i, "shape_arcSpread");
            shapeModule.randomDirectionAmount       = GetFloatParam(i, "shape_randomizeDirectionAmount");
            shapeModule.randomPositionAmount        = GetFloatParam(i, "shape_randomizePositionAmount");
            shapeModule.sphericalDirectionAmount    = GetFloatParam(i, "shape_spherizeDirectionAmount");
            shapeModule.angle                       = GetFloatParam(i, "shape_angle");
            shapeModule.donutRadius                 = GetFloatParam(i, "shape_donutRadius");
            shapeModule.length                      = GetFloatParam(i, "shape_length");
            shapeModule.normalOffset                = GetFloatParam(i, "shape_normalOffset");
            shapeModule.boxThickness                = GetVectorParam(i, "shape_boxThickness");
            shapeModule.position                    = GetVectorParam(i, "shape_position");
            shapeModule.rotation                    = GetVectorParam(i, "shape_rotation");
            shapeModule.scale                       = GetVectorParam(i, "shape_scale");
            shapeModule.mesh                        = AssetDatabase.LoadAssetAtPath<Mesh>(GetStringParam(i, "shape_mesh"));
            shapeModule.meshRenderer                = AssetDatabase.LoadAssetAtPath<MeshRenderer>( GetStringParam(i, "shape_meshRenderer"));
            shapeModule.skinnedMeshRenderer         = AssetDatabase.LoadAssetAtPath<SkinnedMeshRenderer>(GetStringParam(i, "shape_skinnedMeshRenderer"));
            shapeModule.arcSpeed                    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "shape_arcSpeed"));
            shapeModule.radiusMode                  = 0;
            shapeModule.radiusSpeed                 = 0;
            shapeModule.radiusSpread                = 0;
            return;
        }

        private void MapVelocityOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = pSystem.velocityOverLifetime;

            try {
                velocityOverLifetimeModule.enabled      = GetBoolParam(i, "velocityOverLifetime_enabled");
            }
            catch (NullReferenceException) {
                velocityOverLifetimeModule.enabled = false;
                return;
            }

            velocityOverLifetimeModule.space        = (ParticleSystemSimulationSpace) GetIntParam(i, "velocityOverLifetime_space");
            velocityOverLifetimeModule.x            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "velocityOverLifetime_velocity_x"));
            velocityOverLifetimeModule.y            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "velocityOverLifetime_velocity_y"));
            velocityOverLifetimeModule.z            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "velocityOverLifetime_velocity_z"));
            return;
        }

        private void MapLimitVelocityOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = pSystem.limitVelocityOverLifetime;

            try {
                limitVelocityOverLifetimeModule.enabled = GetBoolParam(i, "limitVelocityOverLifetime_enabled");
            }
            catch (NullReferenceException) {
                limitVelocityOverLifetimeModule.enabled = false;
                return;
            }

            limitVelocityOverLifetimeModule.space   = (ParticleSystemSimulationSpace) GetIntParam(i, "limitVelocityOverLifetime_space");
            limitVelocityOverLifetimeModule.separateAxes = GetBoolParam(i, "limitVelocityOverLifetime_separateAxes");
            limitVelocityOverLifetimeModule.multiplyDragByParticleSize = GetBoolParam(i, "limitVelocityOverLifetime_multiplyDragBySize");
            limitVelocityOverLifetimeModule.multiplyDragByParticleVelocity = GetBoolParam(i, "limitVelocityOverLifetime_multiplyDragByVelocity");
            limitVelocityOverLifetimeModule.dampen  = GetFloatParam(i, "limitVelocityOverLifetime_dampen");
            limitVelocityOverLifetimeModule.limit   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "limitVelocityOverLifetime_speed"));
            limitVelocityOverLifetimeModule.drag    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "limitVelocityOverLifetime_drag"));
            return;
        }

        private void MapInheritVelocityParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.InheritVelocityModule inheritVelocityModule = pSystem.inheritVelocity;

            try {
                inheritVelocityModule.enabled           = GetBoolParam(i, "inheritVelocity_enabled");
            }
            catch (NullReferenceException) {
                inheritVelocityModule.enabled = false;
                return;
            }

            inheritVelocityModule.mode              = (ParticleSystemInheritVelocityMode) GetIntParam(i, "inheritVelocity_mode");
            inheritVelocityModule.curve             = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "inheritVelocity_multiplier"));
        }

        private void MapForceOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = pSystem.forceOverLifetime;

            try {
                forceOverLifetimeModule.enabled         = GetBoolParam(i, "forceOverLifetime_enabled");
            }
            catch (NullReferenceException) {
                forceOverLifetimeModule.enabled = false;
                return;
            }

            forceOverLifetimeModule.space           = (ParticleSystemSimulationSpace) GetIntParam(i, "forceOverLifetime_space");
            
            forceOverLifetimeModule.randomized      = GetBoolParam(i, "forceOverLifetime_randomized");
            forceOverLifetimeModule.x               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "forceOverLifetime_force_x"));
            forceOverLifetimeModule.y               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "forceOverLifetime_force_y"));
            forceOverLifetimeModule.z               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "forceOverLifetime_force_z"));
        }

        private void MapColorOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = pSystem.colorOverLifetime;

            try {
                colorOverLifetimeModule.enabled         = GetBoolParam(i, "colorOverLifetime_enabled");
            }
            catch (NullReferenceException) {
                colorOverLifetimeModule.enabled = false;
                return;
            }

            colorOverLifetimeModule.color           = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "colorOverLifetime_color"));
        }

        private void MapColorBySpeedParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ColorBySpeedModule colorBySpeedModule = pSystem.colorBySpeed;

            try {
                colorBySpeedModule.enabled              = GetBoolParam(i, "colorBySpeed_enabled");
            }
            catch (NullReferenceException) {
                colorBySpeedModule.enabled = false;
                return;
            }

            colorBySpeedModule.range                = GetVectorParam(i, "colorBySpeed_range");
            colorBySpeedModule.color                = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "colorBySpeed_color"));
        }

        private void MapSizeOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = pSystem.sizeOverLifetime
            ;

            try {
                sizeOverLifetimeModule.enabled          = GetBoolParam(i, "sizeOverLifetime_enabled");
            }
            catch (NullReferenceException) {
                sizeOverLifetimeModule.enabled = false;
                return;
            }

            sizeOverLifetimeModule.separateAxes     = GetBoolParam(i, "sizeOverLifetime_separateAxes");
            sizeOverLifetimeModule.size             = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size"));
            sizeOverLifetimeModule.x                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size_x"));
            sizeOverLifetimeModule.y                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size_y"));
            sizeOverLifetimeModule.z                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size_z"));
        }

        private void MapSizeBySpeedParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.SizeBySpeedModule sizeBySpeedModule = pSystem.sizeBySpeed;

            try {
                sizeBySpeedModule.enabled               = GetBoolParam(i, "sizeBySpeed_enabled");
            }
            catch (NullReferenceException) {
                sizeBySpeedModule.enabled = false;
                return;
            }

            sizeBySpeedModule.separateAxes          = GetBoolParam(i, "sizeBySpeed_separateAxes");
            sizeBySpeedModule.range                 = GetVectorParam(i, "sizeBySpeed_range");
            sizeBySpeedModule.size                  = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size"));
            sizeBySpeedModule.x                     = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size_x"));
            sizeBySpeedModule.y                     = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size_y"));
            sizeBySpeedModule.z                     = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size_z"));
        }

        private void MapRotationOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = pSystem.rotationOverLifetime;

            try {
                rotationOverLifetimeModule.enabled      = GetBoolParam(i, "rotationOverLifetime_enabled");
            }
            catch (NullReferenceException) {
                rotationOverLifetimeModule.enabled = false;
                return;
            }

            rotationOverLifetimeModule.separateAxes = GetBoolParam(i, "rotationOverLifetime_separateAxes");
            rotationOverLifetimeModule.x            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationOverLifetime_angularVelocity_x"));
            rotationOverLifetimeModule.y            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationOverLifetime_angularVelocity_y"));
            rotationOverLifetimeModule.z            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationOverLifetime_angularVelocity_z"));
        }

        private void MapRotationBySpeedParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.RotationBySpeedModule rotationBySpeedModule = pSystem.rotationBySpeed;

            try {
                rotationBySpeedModule.enabled           = GetBoolParam(i, "rotationBySpeed_enabled");
            }
            catch (NullReferenceException) {
                rotationBySpeedModule.enabled = false;
                return;
            }

            rotationBySpeedModule.separateAxes      = GetBoolParam(i, "rotationBySpeed_separateAxes");
            rotationBySpeedModule.range             = GetVectorParam(i, "rotationBySpeed_range");
            rotationBySpeedModule.x                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationBySpeed_angularVelocity_x"));
            rotationBySpeedModule.y                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationBySpeed_angularVelocity_x"));
            rotationBySpeedModule.z                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationBySpeed_angularVelocity_x"));
        }

        private void MapExternalForcesParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ExternalForcesModule externalForcesModule = pSystem.externalForces;

            try {
                externalForcesModule.enabled            = GetBoolParam(i, "externalForces_enabled");
            }
            catch (NullReferenceException) {
                externalForcesModule.enabled = false;
                return;
            }

            externalForcesModule.multiplier         = GetFloatParam(i, "externalForces_multiplier");
        }

        private void MapNoiseParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.NoiseModule noiseModule  = pSystem.noise;

            try {
                noiseModule.enabled                     = GetBoolParam(i, "noise_enabled");
            }
            catch (NullReferenceException) {
                noiseModule.enabled = false;
                return;
            }

            noiseModule.quality                     = (ParticleSystemNoiseQuality) GetIntParam(i, "noise_quality");
            noiseModule.octaveCount                 = GetIntParam(i, "noise_octaves");
            
            noiseModule.separateAxes                = GetBoolParam(i, "noise_separateAxes");
            noiseModule.damping                     = GetBoolParam(i, "noise_damping");
            noiseModule.remapEnabled                = GetBoolParam(i, "noise_remap");
            noiseModule.octaveMultiplier            = GetFloatParam(i, "noise_octaveMultiplier");
            noiseModule.octaveScale                 = GetFloatParam(i, "noise_octaveScale");
            noiseModule.frequency                   = GetFloatParam(i, "noise_frequency");
            noiseModule.remap                       = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_remapCurve"));
            noiseModule.scrollSpeed                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_scrollSpeed"));
            noiseModule.positionAmount              = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_positionAmount"));
            noiseModule.rotationAmount              = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_rotationAmount"));
            noiseModule.sizeAmount                  = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_scaleAmount"));
            noiseModule.strength                    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength"));
            noiseModule.strengthX                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength_x"));
            noiseModule.strengthY                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength_y"));
            noiseModule.strengthZ                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength_z"));
            
        }

        private void MapCollisionParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.CollisionModule collisionModule = pSystem.collision;

            try {
                collisionModule.enabled                 = GetBoolParam(i, "collision_enabled");
            }
            catch (NullReferenceException) {
                collisionModule.enabled = false;
                return;
            }

            collisionModule.type                    = (ParticleSystemCollisionType)     GetIntParam(i, "collision_type");
            collisionModule.mode                    = (ParticleSystemCollisionMode)     GetIntParam(i, "collision_mode");
            collisionModule.quality                 = (ParticleSystemCollisionQuality)  GetIntParam(i, "collision_quality");
            collisionModule.enableDynamicColliders  = GetBoolParam(i, "collision_enableDynamicColliders");
            collisionModule.multiplyColliderForceByCollisionAngle   = GetBoolParam(i, "collision_multiplyByCollisionAngle");
            collisionModule.multiplyColliderForceByParticleSize     = GetBoolParam(i, "collision_multiplyByParticleSize");
            collisionModule.multiplyColliderForceByParticleSpeed    = GetBoolParam(i, "collision_multiplyByParticleSpeed");
            collisionModule.sendCollisionMessages   = GetBoolParam(i, "collision_sendCollisionMessages");
            collisionModule.minKillSpeed            = GetFloatParam(i, "collision_minKillSpeed");
            collisionModule.maxKillSpeed            = GetFloatParam(i, "collision_maxKillSpeed");
            collisionModule.radiusScale             = GetFloatParam(i, "collision_radiusScale");
            collisionModule.colliderForce           = GetFloatParam(i, "collision_colliderForce");
            collisionModule.lifetimeLoss            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "collision_lifetimeLoss"));
            collisionModule.bounce                  = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "collision_bounce"));
            collisionModule.dampen                  = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "collision_dampen"));
        }

        private void MapTriggerParameters(ParticleSystem pSystem, int i) {
            
        }

        private void MapSubEmitterParameters(ParticleSystem pSystem, int i) {
            
        }

        private void MapTextureSheetAnimationParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = pSystem.textureSheetAnimation;

            try {
                textureSheetAnimationModule.enabled                     = GetBoolParam(i, "textureSheetAnimation_enabled");
            }
            catch (NullReferenceException) {
                textureSheetAnimationModule.enabled = false;
                return;
            }

            textureSheetAnimationModule.mode            = (ParticleSystemAnimationMode) GetIntParam(i, "textureSheetAnimation_mode");
            textureSheetAnimationModule.animation       = (ParticleSystemAnimationType) GetIntParam(i, "textureSheetAnimation_animation");
            textureSheetAnimationModule.enabled         = GetBoolParam(i, "textureSheetAnimation_enabled");
            textureSheetAnimationModule.cycleCount      = GetIntParam(i, "textureSheetAnimation_cycles");
            textureSheetAnimationModule.flipU           = GetFloatParam(i, "textureSheetAnimation_flipU");
            textureSheetAnimationModule.flipV           = GetFloatParam(i, "textureSheetAnimation_flipV");
            textureSheetAnimationModule.frameOverTime   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "textureSheetAnimation_frame"));
            textureSheetAnimationModule.startFrame      = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "textureSheetAnimation_startFrame"));
        }

        private void MapLightParameters(ParticleSystem pSystem, int i) {
            
        }

        private void MapTrailParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.TrailModule trailModule      = pSystem.trails;

            try {
                trailModule.enabled                     = GetBoolParam(i, "trails_enabled");
            }
            catch (NullReferenceException) {
                trailModule.enabled = false;
                return;
            }

            trailModule.textureMode                     = (ParticleSystemTrailTextureMode) GetIntParam(i, "trails_textureMode");
            trailModule.worldSpace                      = GetBoolParam(i, "trails_worldSpace");
            trailModule.dieWithParticles                = GetBoolParam(i, "trails_dieWithParticles");
            trailModule.sizeAffectsWidth                = GetBoolParam(i, "trails_sizeAffectsWidth");
            trailModule.sizeAffectsLifetime             = GetBoolParam(i, "trails_sizeAffectsLifetime");
            trailModule.inheritParticleColor            = GetBoolParam(i, "trails_inheritParticleColor");
            trailModule.generateLightingData            = GetBoolParam(i, "trails_generateLightingData");
            trailModule.minVertexDistance               = GetFloatParam(i, "trails_minimumVertexDistance");
            trailModule.ratio                           = GetFloatParam(i, "trails_ratio");
            trailModule.widthOverTrail                  = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "trails_widthOverTrail"));
            trailModule.lifetime                        = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "trails_lifetime"));
            trailModule.colorOverLifetime               = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "trails_colorOverLifetime"));
            trailModule.colorOverTrail                  = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "trails_colorOverTrail"));
        }

        private void MapCustomDataParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.CustomDataModule customDataModule = pSystem.customData;
            
            try {
                customDataModule.enabled                    = GetBoolParam(i, "customData_enabled");
            }
            catch (NullReferenceException) {
                customDataModule.enabled = false;
                return;
            }

            customDataModule.SetMode                    (ParticleSystemCustomData.Custom1,ParticleSystemCustomDataMode.Vector);
		    customDataModule.SetMode                    (ParticleSystemCustomData.Custom2,ParticleSystemCustomDataMode.Vector);
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 0));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 1));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 2));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 3));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 0));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 1));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 2));
            customDataModule.SetVector                  (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 3));
        }

        private void MapRendererParameters(ParticleSystem pSystem, int i) {
            ParticleSystemRenderer renderer             = pSystem.GetComponent<ParticleSystemRenderer>();

            try {
                renderer.enabled                        = GetBoolParam(i, "renderer_enabled");
            }
            catch (NullReferenceException) {
                renderer.enabled = false;
                return;
            }

            renderer.alignment                          = (ParticleSystemRenderSpace)                   GetIntParam(i, "renderer_alignment");
            renderer.shadowCastingMode                  = (UnityEngine.Rendering.ShadowCastingMode)     GetIntParam(i, "renderer_castShadows");
            renderer.renderMode                         = (ParticleSystemRenderMode)                    GetIntParam(i, "renderer_mode");
            renderer.lightProbeUsage                    = (UnityEngine.Rendering.LightProbeUsage)       GetIntParam(i, "renderer_lightProbes");
            renderer.maskInteraction                    = (SpriteMaskInteraction)                       GetIntParam(i, "renderer_masking");
            renderer.motionVectorGenerationMode         = (MotionVectorGenerationMode)                  GetIntParam(i, "renderer_motionVectors");
            renderer.reflectionProbeUsage               = (UnityEngine.Rendering.ReflectionProbeUsage)  GetIntParam(i, "renderer_reflectionProbes");
            renderer.sortMode                           = (ParticleSystemSortMode)                      GetIntParam(i, "renderer_sortMode");
            renderer.sortingOrder                       = GetIntParam(i, "renderer_orderInLayer");
            renderer.receiveShadows                     = GetBoolParam(i, "renderer_receiveShadows");
            renderer.sortingFudge                       = GetFloatParam(i, "renderer_sortingFudge");
            renderer.normalDirection                    = GetFloatParam(i, "renderer_normalDirection");
            renderer.maxParticleSize                    = GetFloatParam(i, "renderer_maxParticleSize");
            renderer.minParticleSize                    = GetFloatParam(i, "renderer_minParticleSize");
            renderer.pivot                              = GetVectorParam(i, "renderer_pivot");
            renderer.material                           = AssetDatabase.LoadAssetAtPath<Material>(      GetStringParam(i, "renderer_material") );
            renderer.trailMaterial                      = AssetDatabase.LoadAssetAtPath<Material>(      GetStringParam(i, "renderer_trailMaterial") );
        }

        public int GetIntParam(int emitterIndex, string parameter, int parameterIndex = 0) {
            string customizedXpath = String.Format("root/emitter[{0}]/attribute[text() = \'{1}\']/value[{2}]", emitterIndex + 1, parameter, parameterIndex + 1);
			XmlNode node = doc.SelectSingleNode(customizedXpath);
			
			return Convert.ToInt32(node.InnerText);
		}

        public bool GetBoolParam(int emitterIndex, string parameter, int parameterIndex = 0) {
			return Convert.ToBoolean(GetIntParam(emitterIndex, parameter, parameterIndex));
		}

		public float GetFloatParam(int emitterIndex, string parameter, int parameterIndex = 0) {
            string customizedXpath = String.Format("root/emitter[{0}]/attribute[text() = \'{1}\']/value[{2}]", emitterIndex + 1, parameter, parameterIndex + 1);
			XmlNode node = doc.SelectSingleNode(customizedXpath);
			
			return Convert.ToSingle(node.InnerText);
		}

		public string GetStringParam(int emitterIndex, string parameter, int parameterIndex = 0) {
            string customizedXpath = String.Format("root/emitter[{0}]/attribute[text() = \'{1}\']/value[{2}]", emitterIndex + 1, parameter, parameterIndex + 1);
			XmlNode node = doc.SelectSingleNode(customizedXpath);

			return node.InnerText;
		}

		public Vector4 GetVectorParam(int emitterIndex, string parameter) {
            string customizedXpath = String.Format("root/emitter[{0}]/attribute[text() = \'{1}\']/value", emitterIndex + 1, parameter);
			XmlNodeList nodes = doc.SelectNodes(customizedXpath);

            Vector4 vector = new Vector4();

            int i = 0;
            foreach (XmlNode node in nodes) {
                vector[i] = Convert.ToSingle(node.InnerText);
                i++;
            }

            return vector;
		}

        //  Refreshing

        private void OnApplicationFocus(bool hasFocus) {		
            if (refreshOnFocus == true) {
            _isDirty = !hasFocus;
            }
        }

        private IEnumerator checkForUpdates() {
            yield return new WaitForSeconds(updateInterval);
            _isDirty = true;
	    }
    }
}