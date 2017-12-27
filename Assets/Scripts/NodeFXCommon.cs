using System;
using UnityEngine;
using System.IO;
using System.Xml;

namespace NodeFX {
	public static class NodeFXUtilities {

		public static ParticleSystem.MinMaxCurve InterpretStringToCurve(string parameter) {
			ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve();
			
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

		public static ParticleSystem.MinMaxGradient InterpretStringToGradient(string parameter) {
			ParticleSystem.MinMaxGradient curve = new ParticleSystem.MinMaxGradient();

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

		public static ParticleSystem.Burst[] InterpretStringToBurst(string parameter)
		{
			string[] choppedString = parameter.Split(":".ToCharArray());

			int numBursts = choppedString.Length / 4;

			ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[numBursts];

			for (int i = 0; i < numBursts; i++) {
				ParticleSystem.Burst currentBurst = new ParticleSystem.Burst();
				currentBurst.time = Convert.ToSingle(choppedString[4 * i]);
				currentBurst.cycleCount = Convert.ToInt16(choppedString[4 * i + 1]);
				currentBurst.repeatInterval = Convert.ToSingle(choppedString[4 * i + 2]);
				currentBurst.count = InterpretStringToCurve(choppedString[4 * i + 3]);
				bursts[i] = currentBurst;
			}
			return bursts;
		}

		///	<Summary>
		///	Reads a list of values and returns a float curve. The number of samples decide the resolution of the resulting curve. We need the offset to be able to handle curve pairs (such as the "random between curves" mode).
		///	</summary>
		private static AnimationCurve GenerateCurve(string[] parameter, int offset = 4) {
			AnimationCurve curve = new AnimationCurve();
			int samples = Convert.ToInt32(parameter[2]);

			for (int i = 0; i < samples; i++) {
				float position = (float) i / (samples - 1.0f);
				float value = Convert.ToSingle(parameter[i+offset]);
				curve.AddKey(position, value);
			}
			return curve;
		}

		///	<Summary>
		///	Reads a list of values and returns a gradient. The number of samples decide the resolution of the resulting gradient (although this should in most cases be kept at 8). We need the offset to be able to handle gradient pairs (such as the "random between gradients" mode).
		///	</summary>
		private static Gradient GenerateGradient(string[] parameter, int offset = 4) {
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

		public static int GetEmitterCount(XmlDocument doc) {
			string numEmitters = doc.SelectSingleNode("root").Attributes[0].Value;
			return Convert.ToInt32(numEmitters);
		}

        public static string GetSource(XmlDocument doc) {
            string source = doc.SelectSingleNode("root").Attributes[1].Value;
			return source;
        }

		public static FileSystemWatcher CreateFileWatcher(string effectPath, string effectName)
        {
            FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
            string folder = effectPath.Replace(effectName + ".xml", "");
            string folderPath = Application.dataPath + folder.Substring(6);
            _fileSystemWatcher.Path = folderPath;

            // Watch for changes in LastAccess and LastWrite times
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            
            // Only watch xml files.
            _fileSystemWatcher.Filter = "*.xml";

            // Add event handlers.
            _fileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
            _fileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);
            _fileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            _fileSystemWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            return _fileSystemWatcher;
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
			foreach (NodeFXEffect effect in GameObject.FindObjectsOfType<NodeFXEffect>()) {
				effect.Refresh();
			}
        }
	}
}
