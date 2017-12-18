using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeFX;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class NodeFXEffect : MonoBehaviour {

	public TextAsset emitterDefinition;

	public bool updateOnInterval;
	public float updateInterval;
	public bool updateOnFileChanged;
	public bool updateOnFocus;
	
	private bool _isDirty;
	private XmlDocument doc;
	private string path;
	private ParticleSystem _particleSystem;
	private XMLImporter _importer;

	void OnEnabled() {
		_importer = new XMLImporter();
	}

	void OnApplicationFocus() {
		path = AssetDatabase.GetAssetOrScenePath(emitterDefinition);

		if(!string.IsNullOrEmpty(path) && updateOnFocus) {
			_importer.LoadXML(path);
		}
	}

	IEnumerator checkForUpdates() {
		yield return new WaitForSeconds(updateInterval);
		_isDirty = true;
	}
}
