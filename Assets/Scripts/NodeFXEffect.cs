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
		public bool refreshAtInterval;
		public float refreshInterval = 5.0f;
		public bool refreshOnFocus;
		public bool refreshOnFileChange;
		
		public bool _isDirty;
		private XmlDocument doc;
		private ParticleSystem _particleSystem;

		void OnEnabled() {

		}

        public void Refresh()
        {
			Debug.Log("Effect: Refreshing");
            path = AssetDatabase.GetAssetOrScenePath(effectDefinition);
			if(!string.IsNullOrEmpty(path)) {
				LoadXML(path);
			}
        }

		public void LoadXML(string path) {
			Debug.Log("Effect: Loading XML at path " + path);
			doc = new XmlDocument();
			doc.Load(path);
            InstantiateParticleSystem();
		}

		public int GetIntParam(int emitterIndex, string parameter, int parameterIndex = 0) {
            string customizedXpath = String.Format("root/emitter[{0}]/attribute[text() = \'{1}\']/value[{2}]", emitterIndex + 1, parameter, parameterIndex + 1);
			XmlNode node = doc.SelectSingleNode(customizedXpath);
			
			return Convert.ToInt32(node.InnerText);
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

        private void InstantiateParticleSystem() {

			Transform parent = gameObject.transform;

            DeleteOldParticleSystems();

			for (int i = 0; i < GetEmitterCount(); i++) {

				GameObject Emitter = new GameObject("Emitter" + i);
				Emitter.transform.parent = parent;
				ParticleSystem pSystem = Emitter.AddComponent<ParticleSystem>();
				pSystem.Stop();

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

				pSystem.Play();
			}
		}

        private void DeleteOldParticleSystems()
        {
            foreach (ParticleSystem ps in gameObject.GetComponentsInChildren<ParticleSystem>()) {
                DestroyImmediate(ps.gameObject);
            }
        }

        private void MapMainParameters(ParticleSystem pSystem, int i)
        {
            Debug.Log("Effect: Mapping parameters: Main");
            ParticleSystem.MainModule mainModule = pSystem.main;
			mainModule.duration = GetFloatParam(i, "main_duration");
            Debug.Log(GetFloatParam(i, "main_duration"));
        }

        private void MapEmissionParameters(ParticleSystem pSystem, int i)
        {
            Debug.Log("Effect: Mapping parameters: Emission");
        }

        private void MapShapeParameters(ParticleSystem pSystem, int i)
        {
            Debug.Log("Effect: Mapping parameters: Shape");
        }

        private void MapVelocityOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            Debug.Log("Effect: Mapping parameters: VelocityOverLifetime");
        }

        private void MapLimitVelocityOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            Debug.Log("Effect: Mapping parameters: LimitVelocityOverLifetime");
        }

        private void MapInheritVelocityParameters(ParticleSystem pSystem, int i)
        {
            Debug.Log("Effect: Mapping parameters: InheritVelocity");
        }

        private void MapForceOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapColorOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapColorBySpeedParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapSizeOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapSizeBySpeedParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapRotationOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapRotationBySpeedParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapExternalForcesParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapNoiseParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapCollisionParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapTriggerParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapSubEmitterParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapTextureSheetAnimationParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapLightParamters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapTrailParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapCustomDataParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapRendererParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }
    }
}