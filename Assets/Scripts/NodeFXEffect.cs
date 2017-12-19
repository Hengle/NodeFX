using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace NodeFX {
	public class NodeFXEffect : MonoBehaviour {

		public TextAsset effectDefinition;
		public string path;

		public bool enableRefresh;
		public bool refreshOnFocus;
		public bool refreshOnFileChange;

		private XmlDocument doc;
		private ParticleSystem _particleSystem;

        public void Refresh()
        {
			Debug.Log("Effect: Refreshing");
            path = AssetDatabase.GetAssetOrScenePath(effectDefinition);
			if(!string.IsNullOrEmpty(path)) {
				LoadXML(path);
			}
        }

		private void LoadXML(string path) {
			Debug.Log("Effect: Loading XML at path " + path);
			doc = new XmlDocument();
			doc.Load(path);
            InstantiateParticleSystem();
		}

        private void InstantiateParticleSystem() {

			Transform parent = transform;
            gameObject.name = effectDefinition.name;
            
            DeleteOldParticleSystems();

			for (int i = 0; i < GetEmitterCount(); i++) {
                
                ParticleSystem pSystem;

                //  For the first emitter we want to add the particlesystem component to our root gameobject. For all subsequent emitters we want to create child gameobjects.
                if (i == 0) {
                    if (gameObject.GetComponent<ParticleSystem>() == null) {
                        pSystem = gameObject.AddComponent<ParticleSystem>();
                    } else {
                        pSystem = GetComponent<ParticleSystem>();
                    }
                } else {
                    GameObject Emitter = new GameObject("Emitter" + i);
                    Emitter.transform.parent = parent;
				    pSystem = Emitter.AddComponent<ParticleSystem>();
                }

                pSystem.gameObject.SetActive(false);

				MapMainParameters(pSystem, i);
				MapEmissionParameters(pSystem, i);
				MapShapeParameters(pSystem, i);
				MapVelocityOverLifetimeParameters(pSystem, i);
				MapLimitVelocityOverLifetimeParameters(pSystem, i);
				MapInheritVelocityParameters(pSystem, i);
				MapForceOverLifetimeParameters(pSystem, i);
				MapColorOverLifetimeParameters(pSystem, i);
				MapColorBySpeedParameters(pSystem, i);
				MapSizeOverLifetimeParameters(pSystem, i);
				MapSizeBySpeedParameters(pSystem, i);
				MapRotationOverLifetimeParameters(pSystem, i);
				MapRotationBySpeedParameters(pSystem, i);
				MapExternalForcesParameters(pSystem, i);
				MapNoiseParameters(pSystem, i);
				MapCollisionParameters(pSystem, i);
				MapTriggerParameters(pSystem, i);
				MapSubEmitterParameters(pSystem, i);
				MapTextureSheetAnimationParameters(pSystem, i);
				MapLightParamters(pSystem, i);
				MapTrailParameters(pSystem, i);
				MapCustomDataParameters(pSystem, i);
				MapRendererParameters(pSystem, i);

                pSystem.gameObject.SetActive(true);
				pSystem.Play();
			}
		}

        private void DeleteOldParticleSystems() {
            foreach (ParticleSystem ps in gameObject.GetComponentsInChildren<ParticleSystem>()) {
                if (ps.gameObject != gameObject) {
                DestroyImmediate(ps.gameObject);
                }
            }
        }

        private void MapMainParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.MainModule mainModule = pSystem.main;
			mainModule.duration                     = GetFloatParam(i, "main_duration");
            mainModule.loop                         = GetBoolParam(i, "main_looping");
            mainModule.prewarm                      = GetBoolParam(i, "main_prewarm");
            mainModule.startDelay                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startDelay"));
            mainModule.startLifetime                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startLifetime"));
            mainModule.startSpeed                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSpeed"));

            mainModule.startSize3D                  = GetBoolParam(i, "main_3DStartSize");
            mainModule.startSize                    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize"));
            mainModule.startSizeX                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize_x"));
            mainModule.startSizeY                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize_y"));
            mainModule.startSizeZ                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startSize_z"));

            mainModule.startRotation3D              = GetBoolParam(i, "main_3DStartRotation");
            mainModule.startRotation                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation"));
            mainModule.startRotationX               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation_x"));
            mainModule.startRotationY               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation_y"));
            mainModule.startRotationZ               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_startRotation_z"));
            mainModule.randomizeRotationDirection   = GetFloatParam(i, "main_rotationVariance");

            mainModule.startColor                   = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "main_startColor"));
            mainModule.gravityModifier              = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "main_gravityModifier"));

            mainModule.simulationSpace              = (ParticleSystemSimulationSpace) GetIntParam(i, "main_simulationSpace");
            mainModule.simulationSpeed              = GetFloatParam(i, "main_simulationSpeed");

            mainModule.useUnscaledTime              = GetBoolParam(i, "main_deltaTime");
            mainModule.scalingMode                  = (ParticleSystemScalingMode) GetIntParam(i, "main_scalingMode");
            mainModule.playOnAwake                  = GetBoolParam(i, "main_playOnAwake");
            mainModule.emitterVelocityMode          = (ParticleSystemEmitterVelocityMode) GetIntParam(i, "main_emitterVelocity");
            mainModule.maxParticles                 = GetIntParam(i, "main_maxParticles");
            pSystem.useAutoRandomSeed               = GetBoolParam(i, "main_autoRandomSeed");
            mainModule.stopAction                   = (ParticleSystemStopAction) GetIntParam(i, "main_stopAction");
        }

        private void MapEmissionParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.EmissionModule emissionModule = pSystem.emission;
            emissionModule.enabled                  = GetBoolParam(i, "emission_enabled");
            emissionModule.rateOverTime             = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "emission_rateOverTime"));
            emissionModule.rateOverDistance         = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "emission_rateOverDistance"));
            emissionModule.SetBursts(NodeFXUtilities.InterpretStringToBurst("emission_bursts"));
        }

        private void MapShapeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ShapeModule shapeModule = pSystem.shape;
            shapeModule.enabled                     = GetBoolParam(i, "shape_enabled");
            shapeModule.shapeType                   = (ParticleSystemShapeType) GetIntParam(i, "shape_shape");
            shapeModule.radius                      = GetFloatParam(i, "shape_radius");
            shapeModule.radiusThickness             = GetFloatParam(i, "shape_radiusThickness");
            shapeModule.radiusMode                  = 0;
            shapeModule.radiusSpeed                 = 0;
            shapeModule.radiusSpread                = 0;
            shapeModule.arc                         = GetFloatParam(i, "shape_arc");
            shapeModule.arcMode                     = (ParticleSystemShapeMultiModeValue) GetIntParam(i, "shape_arcMode");
            shapeModule.arcSpeed                    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "shape_arcSpeed"));
            shapeModule.arcSpread                   = GetFloatParam(i, "shape_arcSpread");
            shapeModule.alignToDirection            = GetBoolParam(i, "shape_alignToDirection");
            shapeModule.randomDirectionAmount       = GetFloatParam(i, "shape_randomizeDirectionAmount");
            shapeModule.randomPositionAmount        = GetFloatParam(i, "shape_randomizePositionAmount");
            shapeModule.sphericalDirectionAmount    = GetFloatParam(i, "shape_spherizeDirectionAmount");
            shapeModule.angle                       = GetFloatParam(i, "shape_angle");
            shapeModule.boxThickness                = GetVectorParam(i, "shape_boxThickness");
            shapeModule.donutRadius                 = GetFloatParam(i, "shape_donutRadius");
            shapeModule.length                      = GetFloatParam(i, "shape_length");
            shapeModule.mesh                        = AssetDatabase.LoadAssetAtPath<Mesh>(GetStringParam(i, "shape_mesh"));
            shapeModule.meshRenderer                = AssetDatabase.LoadAssetAtPath<MeshRenderer>( GetStringParam(i, "shape_meshRenderer"));
            shapeModule.skinnedMeshRenderer         = AssetDatabase.LoadAssetAtPath<SkinnedMeshRenderer>(GetStringParam(i, "shape_skinnedMeshRenderer"));
            shapeModule.meshShapeType               = (ParticleSystemMeshShapeType) GetIntParam(i, "shape_meshShapeType");
            shapeModule.meshMaterialIndex           = GetIntParam(i, "shape_meshMaterialIndex");
            shapeModule.normalOffset                = GetFloatParam(i, "shape_normalOffset");
            shapeModule.useMeshMaterialIndex        = GetBoolParam(i, "shape_useMeshMaterialIndex");
            shapeModule.useMeshColors               = GetBoolParam(i, "shape_useMeshColors");
            shapeModule.position                    = GetVectorParam(i, "shape_position");
            shapeModule.rotation                    = GetVectorParam(i, "shape_rotation");
            shapeModule.scale                       = GetVectorParam(i, "shape_scale");
        }

        private void MapVelocityOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = pSystem.velocityOverLifetime;
            velocityOverLifetimeModule.enabled      = GetBoolParam(i, "velocityOverLifetime_enabled");
            velocityOverLifetimeModule.space        = (ParticleSystemSimulationSpace) GetIntParam(i, "velocityOverLifetime_space");
            velocityOverLifetimeModule.x            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "velocityOverLifetime_velocity_x"));
            velocityOverLifetimeModule.y            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "velocityOverLifetime_velocity_y"));
            velocityOverLifetimeModule.z            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "velocityOverLifetime_velocity_z"));
        }

        private void MapLimitVelocityOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetimeModule = pSystem.limitVelocityOverLifetime;
            limitVelocityOverLifetimeModule.enabled = GetBoolParam(i, "limitVelocityOverLifetime_enabled");
            limitVelocityOverLifetimeModule.space   = (ParticleSystemSimulationSpace) GetIntParam(i, "limitVelocityOverLifetime_space");
            limitVelocityOverLifetimeModule.separateAxes = GetBoolParam(i, "limitVelocityOverLifetime_separateAxes");
            limitVelocityOverLifetimeModule.limit   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "limitVelocityOverLifetime_speed"));
            limitVelocityOverLifetimeModule.dampen  = GetFloatParam(i, "limitVelocityOverLifetime_dampen");
            limitVelocityOverLifetimeModule.drag    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "limitVelocityOverLifetime_drag"));
            limitVelocityOverLifetimeModule.multiplyDragByParticleSize = GetBoolParam(i, "limitVelocityOverLifetime_multiplyDragBySize");
            limitVelocityOverLifetimeModule.multiplyDragByParticleVelocity = GetBoolParam(i, "limitVelocityOverLifetime_multiplyDragByVelocity");
        }

        private void MapInheritVelocityParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.InheritVelocityModule inheritVelocityModule = pSystem.inheritVelocity;
            inheritVelocityModule.enabled           = GetBoolParam(i, "inheritVelocity_enabled");
            inheritVelocityModule.mode              = (ParticleSystemInheritVelocityMode) GetIntParam(i, "inheritVelocity_mode");
            inheritVelocityModule.curve             = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "inheritVelocity_multiplier"));
        }

        private void MapForceOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ForceOverLifetimeModule forceOverLifetimeModule = pSystem.forceOverLifetime;
            forceOverLifetimeModule.enabled         = GetBoolParam(i, "forceOverLifetime_enabled");
            forceOverLifetimeModule.space           = (ParticleSystemSimulationSpace) GetIntParam(i, "forceOverLifetime_space");
            forceOverLifetimeModule.randomized      = GetBoolParam(i, "forceOverLifetime_randomized");
            forceOverLifetimeModule.x               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "forceOverLifetime_force_x"));
            forceOverLifetimeModule.y               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "forceOverLifetime_force_y"));
            forceOverLifetimeModule.z               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "forceOverLifetime_force_z"));
        }

        private void MapColorOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = pSystem.colorOverLifetime;
            colorOverLifetimeModule.enabled         = GetBoolParam(i, "colorOverLifetime_enabled");
            colorOverLifetimeModule.color           = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "colorOverLifetime_color"));
        }

        private void MapColorBySpeedParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ColorBySpeedModule colorBySpeedModule = pSystem.colorBySpeed;
            colorBySpeedModule.enabled              = GetBoolParam(i, "colorBySpeed_enabled");
            colorBySpeedModule.range                = GetVectorParam(i, "colorBySpeed_range");
            colorBySpeedModule.color                = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "colorBySpeed_color"));
        }

        private void MapSizeOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = pSystem.sizeOverLifetime
            ;
            sizeOverLifetimeModule.enabled          = GetBoolParam(i, "sizeOverLifetime_enabled");
            sizeOverLifetimeModule.separateAxes     = GetBoolParam(i, "sizeOverLifetime_separateAxes");
            sizeOverLifetimeModule.size             = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size"));
            sizeOverLifetimeModule.x                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size_x"));
            sizeOverLifetimeModule.y                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size_y"));
            sizeOverLifetimeModule.z                = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeOverLifetime_size_z"));
        }

        private void MapSizeBySpeedParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.SizeBySpeedModule sizeBySpeedModule = pSystem.sizeBySpeed;
            sizeBySpeedModule.enabled               = GetBoolParam(i, "sizeBySpeed_enabled");
            sizeBySpeedModule.separateAxes          = GetBoolParam(i, "sizeBySpeed_separateAxes");
            sizeBySpeedModule.range                 = GetVectorParam(i, "sizeBySpeed_range");
            sizeBySpeedModule.size                  = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size"));
            sizeBySpeedModule.x                     = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size_x"));
            sizeBySpeedModule.y                     = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size_y"));
            sizeBySpeedModule.z                     = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "sizeBySpeed_size_z"));
        }

        private void MapRotationOverLifetimeParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.RotationOverLifetimeModule rotationOverLifetimeModule = pSystem.rotationOverLifetime;
            rotationOverLifetimeModule.enabled      = GetBoolParam(i, "rotationOverLifetime_enabled");
            rotationOverLifetimeModule.separateAxes = GetBoolParam(i, "rotationOverLifetime_separateAxes");
            rotationOverLifetimeModule.x            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationOverLifetime_angularVelocity_x"));
            rotationOverLifetimeModule.y            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationOverLifetime_angularVelocity_y"));
            rotationOverLifetimeModule.z            = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationOverLifetime_angularVelocity_z"));
        }

        private void MapRotationBySpeedParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.RotationBySpeedModule rotationBySpeedModule = pSystem.rotationBySpeed;
            rotationBySpeedModule.enabled           = GetBoolParam(i, "rotationBySpeed_enabled");
            rotationBySpeedModule.separateAxes      = GetBoolParam(i, "rotationBySpeed_separateAxes");
            rotationBySpeedModule.range             = GetVectorParam(i, "rotationBySpeed_range");
            rotationBySpeedModule.x                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationBySpeed_angularVelocity_x"));
            rotationBySpeedModule.y                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationBySpeed_angularVelocity_x"));
            rotationBySpeedModule.z                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "rotationBySpeed_angularVelocity_x"));
        }

        private void MapExternalForcesParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.ExternalForcesModule externalForcesModule = pSystem.externalForces;
            externalForcesModule.enabled            = GetBoolParam(i, "externalForces_enabled");
            externalForcesModule.multiplier         = GetFloatParam(i, "externalForces_multiplier");

        }

        private void MapNoiseParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.NoiseModule noiseModule  = pSystem.noise;
            noiseModule.enabled                     = GetBoolParam(i, "noise_enabled");
            noiseModule.separateAxes                = GetBoolParam(i, "noise_separateAxes");
            noiseModule.strength                    = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength"));
            noiseModule.strengthX                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength_x"));
            noiseModule.strengthY                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength_y"));
            noiseModule.strengthZ                   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_strength_z"));
            noiseModule.octaveCount                 = GetIntParam(i, "noise_octaves");
            noiseModule.octaveMultiplier            = GetFloatParam(i, "noise_octaveMultiplier");
            noiseModule.octaveScale                 = GetFloatParam(i, "noise_octaveScale");
            noiseModule.frequency                   = GetFloatParam(i, "noise_frequency");
            noiseModule.quality                     = (ParticleSystemNoiseQuality) GetIntParam(i, "noise_quality");
            noiseModule.damping                     = GetBoolParam(i, "noise_damping");
            noiseModule.remapEnabled                = GetBoolParam(i, "noise_remap");
            noiseModule.remap                       = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_remapCurve"));
            noiseModule.scrollSpeed                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_scrollSpeed"));
            noiseModule.positionAmount                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_positionAmount"));
            noiseModule.rotationAmount                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_rotationAmount"));
            noiseModule.sizeAmount                 = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "noise_scaleAmount"));
        }

        private void MapCollisionParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.CollisionModule collisionModule = pSystem.collision;
            collisionModule.enabled                 = GetBoolParam(i, "collision_enabled");
            collisionModule.enableDynamicColliders  = GetBoolParam(i, "collision_enableDynamicColliders");
            collisionModule.multiplyColliderForceByCollisionAngle = GetBoolParam(i, "collision_multiplyByCollisionAngle");
            collisionModule.multiplyColliderForceByParticleSize = GetBoolParam(i, "collision_multiplyByParticleSize");
            collisionModule.multiplyColliderForceByParticleSpeed = GetBoolParam(i, "collision_multiplyByParticleSpeed");
            collisionModule.sendCollisionMessages   = GetBoolParam(i, "collision_sendCollisionMessages");
            collisionModule.type                    = (ParticleSystemCollisionType) GetIntParam(i, "collision_type");
            collisionModule.mode                    = (ParticleSystemCollisionMode) GetIntParam(i, "collision_mode");
            collisionModule.quality                 = (ParticleSystemCollisionQuality) GetIntParam(i, "collision_quality");
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
            textureSheetAnimationModule.enabled         = GetBoolParam(i, "textureSheetAnimation_enabled");
            textureSheetAnimationModule.mode            = (ParticleSystemAnimationMode) GetIntParam(i, "textureSheetAnimation_mode");
            textureSheetAnimationModule.animation       = (ParticleSystemAnimationType) GetIntParam(i, "textureSheetAnimation_animation");
            textureSheetAnimationModule.frameOverTime   = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "textureSheetAnimation_frame"));
            textureSheetAnimationModule.startFrame      = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "textureSheetAnimation_startFrame"));
            textureSheetAnimationModule.cycleCount      = GetIntParam(i, "textureSheetAnimation_cycles");
            textureSheetAnimationModule.flipU           = GetFloatParam(i, "textureSheetAnimation_flipU");
            textureSheetAnimationModule.flipV           = GetFloatParam(i, "textureSheetAnimation_flipV");
        }

        private void MapLightParamters(ParticleSystem pSystem, int i) {
            
        }

        private void MapTrailParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.TrailModule trailModule      = pSystem.trails;
            trailModule.enabled                         = GetBoolParam(i, "trails_enabled");
            trailModule.worldSpace                      = GetBoolParam(i, "trails_worldSpace");
            trailModule.dieWithParticles                = GetBoolParam(i, "trails_dieWithParticles");
            trailModule.sizeAffectsWidth                = GetBoolParam(i, "trails_sizeAffectsWidth");
            trailModule.sizeAffectsLifetime             = GetBoolParam(i, "trails_sizeAffectsLifetime");
            trailModule.inheritParticleColor            = GetBoolParam(i, "trails_inheritParticleColor");
            trailModule.generateLightingData            = GetBoolParam(i, "trails_generateLightingData");
            trailModule.minVertexDistance               = GetFloatParam(i, "trails_minimumVertexDistance");
            trailModule.ratio                           = GetFloatParam(i, "trails_ratio");
            trailModule.textureMode                     = (ParticleSystemTrailTextureMode) GetIntParam(i, "trails_textureMode");
            trailModule.colorOverLifetime               = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "trails_colorOverLifetime"));
            trailModule.colorOverTrail               = NodeFXUtilities.InterpretStringToGradient(GetStringParam(i, "trails_colorOverTrail"));
            trailModule.widthOverTrail               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "trails_widthOverTrail"));
            trailModule.lifetime               = NodeFXUtilities.InterpretStringToCurve(GetStringParam(i, "trails_lifetime"));
        }

        private void MapCustomDataParameters(ParticleSystem pSystem, int i) {
            ParticleSystem.CustomDataModule customDataModule = pSystem.customData;
            customDataModule.enabled                    = GetBoolParam(i, "customData_enabled");
            customDataModule.SetMode(ParticleSystemCustomData.Custom1,ParticleSystemCustomDataMode.Vector);
		    customDataModule.SetMode(ParticleSystemCustomData.Custom2,ParticleSystemCustomDataMode.Vector);
            customDataModule.SetVector (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 0));
            customDataModule.SetVector (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 1));
            customDataModule.SetVector (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 2));
            customDataModule.SetVector (ParticleSystemCustomData.Custom1, 0, GetFloatParam(i, "customData_1", 3));
            customDataModule.SetVector (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 0));
            customDataModule.SetVector (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 1));
            customDataModule.SetVector (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 2));
            customDataModule.SetVector (ParticleSystemCustomData.Custom2, 0, GetFloatParam(i, "customData_1", 3));
        }

        private void MapRendererParameters(ParticleSystem pSystem, int i) {
            
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

		public Vector4 GetVectorParam(int emitterIndex, string parameter, int parameterIndex = 0) {
            string customizedXpath = String.Format("root/emitter[{0}]/attribute[text() = \'{1}\']/value[{2}]", emitterIndex + 1, parameter, parameterIndex + 1);
			XmlNodeList nodes = doc.SelectNodes(customizedXpath);

            Vector4 vector = new Vector4();

            int i = 0;
            foreach (XmlNode node in nodes) {
                if (! String.IsNullOrEmpty(node.InnerText)) {
                    vector[i] = Convert.ToSingle(node.InnerText);
                } else {
                    vector[i] = 0;
                }
                i++;
            }
            return vector;
		}

		public int GetEmitterCount() {
			string numEmitters = doc.SelectSingleNode("root").Attributes[0].Value;
			return Convert.ToInt32(numEmitters);
		}
    }
}