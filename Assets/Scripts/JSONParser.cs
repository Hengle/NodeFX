using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[ExecuteInEditMode]
public class JSONParser : MonoBehaviour {

	public ParticleSystem[] emitters;

	// Use this for initialization
	void Start () {
		JsonUtility.FromJsonOverwrite();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

[SerializeField]
public class SerializedParticleSystem : MonoBehaviour {
	public int numEmitters;
	public int id;

	public float main_duration;

	public static SerializedParticleSystem CreateFromJSON(string jsonString) {
		return JsonUtility.FromJson<SerializedParticleSystem>(jsonString);
	}

	public string ToJSON() {
		return JsonUtility.ToJson(this);
	}

	public void Load(string savedData) {
		JsonUtility.FromJsonOverwrite(savedData, this);
	}
}