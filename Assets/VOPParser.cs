using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class VOPParser : MonoBehaviour {

	public ParticleSystem pSystem;
	public HoudiniParms houdiniParms;
	public HoudiniAssetOTL assetOTL;
	
	private HoudiniApiAssetAccessor assetAccessor;
	private int isDirty = 0;

	// Use this for initialization
	void Start () {
		assetAccessor = HoudiniApiAssetAccessor.getAssetAccessor(gameObject);
		// InstantiateParticleSystem();
	}
	
	void Update() {
		assetOTL.reset();
		// isDirty = assetAccessor.getParmIntValue("isDirty", 0);

		// if (isDirty == 1) {
		// 	assetOTL.reset();
		// 	Debug.Log("Hello");
		// }
	}

	void InstantiateParticleSystem() {
		pSystem = GetComponent<ParticleSystem>();

		ParticleSystem.MainModule mainModule = pSystem.main;

		ParticleSystem.EmissionModule emissionModule = pSystem.emission;

		emissionModule.enabled = false;

		mainModule.duration = assetAccessor.getParmFloatValue("main_duration", 0);

		mainModule.loop = Convert.ToBoolean(assetAccessor.getParmIntValue("main_loop", 0));

		// emissionModule.enabled = true;
	}
}
