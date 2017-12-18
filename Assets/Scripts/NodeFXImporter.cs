using System;
using System.Xml;
using UnityEngine;

namespace NodeFX {
	[ExecuteInEditMode]
	public class XMLImporter : MonoBehaviour {

		private XmlDocument doc;
		private ParticleSystem _particleSystem;
		private string xpath = "root/emitter[{emitterIndex}]/attribute[text() = \'{parameter}\']/value[{parameterIndex}]";

		public void LoadXML(string path) {
			doc = new XmlDocument();
			doc.Load(path);
		}

		public int GetIntParam(int emitterIndex, string parameter, int parameterIndex = 0) {
			XmlNode node = doc.SelectSingleNode(String.Format(xpath, emitterIndex + 1, parameter, parameterIndex + 1));
			
			return Convert.ToInt32(node.InnerText);
		}

		public float GetFloatParam(int emitterIndex, string parameter, int parameterIndex = 0) {
			XmlNode node = doc.SelectSingleNode(String.Format(xpath, emitterIndex + 1, parameter, parameterIndex + 1));
			
			return Convert.ToSingle(node.InnerText);
		}

		public string GetStringParam(int emitterIndex, string parameter, int parameterIndex = 0) {
			XmlNode node = doc.SelectSingleNode(String.Format(xpath, emitterIndex + 1, parameter, parameterIndex + 1));
			return node.InnerText;
		}

		public Vector4 GetVectorParam(int emitterIndex, string parameter, int parameterIndex = 0) {
			XmlNodeList nodes = doc.SelectNodes(String.Format(xpath, emitterIndex + 1, parameter, parameterIndex + 1));

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

		void InstantiateParticleSystem() {

			Transform parent = gameObject.transform;

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
				MapInheritVelocityOverLifetimeParameters(pSystem, i);
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

        private void MapMainParameters(ParticleSystem pSystem, int i)
        {
            ParticleSystem.MainModule mainModule = pSystem.main;
			mainModule.duration = GetFloatParam(i, "main_duration");
        }

        private void MapEmissionParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapShapeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapVelocityOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapLimitVelocityOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
        }

        private void MapInheritVelocityOverLifetimeParameters(ParticleSystem pSystem, int i)
        {
            throw new NotImplementedException();
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
