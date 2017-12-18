using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class XMLImporter : MonoBehaviour {

	public string path;
	public TextAsset emitterDefinition;
	XmlDocument doc;

	ParticleSystem _particleSystem;

	void Start() {
		LoadXML();
	}

	void OnApplicationFocus() {
		LoadXML();
		InstantiateParticleSystem();
	}

	void LoadXML() {
		doc = new XmlDocument();
		path = AssetDatabase.GetAssetOrScenePath(emitterDefinition);
		doc.Load(path);
	}

	private int GetIntParam(int emitterIndex, string parameter, int parameterIndex = 0) {
		string xpath = "root/emitter[" + (emitterIndex + 1) + "]/attribute[text() = \'" + parameter + "\']/value[" + (parameterIndex + 1) +"]";
		Debug.Log(xpath);

		XmlNode node = doc.SelectSingleNode(xpath);
		
		return Convert.ToInt32(node.InnerText);
	}

	private float GetFloatParam(int emitterIndex, string parameter, int parameterIndex = 0) {
		string xpath = "root/emitter[" + (emitterIndex + 1) + "]/attribute[text() = \'" + parameter + "\']/value[" + (parameterIndex + 1) +"]";
		Debug.Log(xpath);
		XmlNodeList nodeList = doc.SelectNodes(xpath);
		
		foreach (XmlNode node in nodeList) {
			Debug.Log(node.Name);
		}
		
		return Convert.ToSingle(nodeList.Item(parameterIndex).InnerText);
	}

	private string GetStringParam(int emitterIndex, string parameter, int parameterIndex = 0) {
		return "";
	}

	private Vector4 GetVectorParam(int emitterIndex, string parameter, int parameterIndex = 0) {
		return new Vector4(0,0,0,0);
	}

	private int GetEmitterCount() {
		return 1;
	}

	void InstantiateParticleSystem() {
		Debug.Log(GetFloatParam(0, "main_duration"));
		Debug.Log(GetIntParam(0, "main_duration"));

	// for (int i = 0; i < GetEmitterCount(); i++) {
	// 	ParticleSystem.MainModule mainModule = _particleSystem.main;
	// 	mainModule.duration = GetFloatParam(i, "main_duration");
	// }
	}
}
