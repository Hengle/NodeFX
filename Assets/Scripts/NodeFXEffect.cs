using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeFX;

[ExecuteInEditMode]
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

	void OnEnabled() {

	}

	void OnApplicationFocus() {

	}

	IEnumerator checkForUpdates() {
		yield return new WaitForSeconds(updateInterval);
		_isDirty = true;
	}
}
