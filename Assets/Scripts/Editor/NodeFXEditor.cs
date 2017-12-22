using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeFX;
using System;


[CustomEditor(typeof(NodeFXEffect))]
public class NodeFXEditor : Editor {

	NodeFXEffect targetEffect;

	private GUIStyle headerStyle = new GUIStyle();

	void OnEnable() {
		targetEffect = (NodeFXEffect)target;
		GenerateStyles();
	}

	public override void OnInspectorGUI() {
		GUIDrawHeader();

        targetEffect.effectDefinition = (TextAsset)EditorGUILayout.ObjectField("Effect Definition", targetEffect.effectDefinition, typeof(TextAsset),false);
		
        if (targetEffect.effectDefinition != null) {
            GUIDrawTopShelfButtons();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUIDrawRefreshToggles();

            //CheckForUpdates();
        }
    }

    private void GUIDrawHeader()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("", headerStyle, GUILayout.MinHeight(75), GUILayout.MaxWidth(250));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void GUIDrawRefreshToggles() {
		
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();

        targetEffect.refreshOnFileChange = EditorGUILayout.Toggle("Refresh On Definition Update", targetEffect.refreshOnFileChange);
        targetEffect.refreshOnFocus = EditorGUILayout.Toggle("Refresh On Window Focus", targetEffect.refreshOnFocus);
        targetEffect.refreshAtInterval = EditorGUILayout.Toggle("Refresh At Interval", targetEffect.refreshAtInterval);

        if (targetEffect.refreshAtInterval) {
            targetEffect.updateInterval = EditorGUILayout.FloatField("Interval (in seconds)",targetEffect.updateInterval);
        }

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
        
    }

    private void GUIDrawTopShelfButtons()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Refresh"),
                                GUILayout.MinHeight(30),
                                GUILayout.MaxWidth(400)
                                ))
        {
            OnButtonRefresh();
        }

        if (GUILayout.Button(new GUIContent("Open Editor"),
                                GUILayout.MinHeight(30),
                                GUILayout.MaxWidth(400)
                                ))
        {
            OnButtonOpenEditor();
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void OnButtonOpenEditor()
    {
        if (targetEffect.effectDefinition != null) {
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
            start.FileName = targetEffect.source;
            System.Diagnostics.Process.Start(start);
        }
    }

    private void OnButtonRefresh() {
		targetEffect.Refresh();
	}

	private void GenerateStyles() {
		headerStyle.alignment = TextAnchor.UpperCenter;
		headerStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/nodefx_logo.png");
	}
} 

public class NodeFXMenu : MonoBehaviour {
    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    [MenuItem("GameObject/Effects/NodeFX System")]
    static void CreateNewEffect(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject effect = new GameObject("NodeFXEffect");
        effect.AddComponent<NodeFXEffect>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(effect, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(effect, "Create " + effect.name);
        Selection.activeObject = effect;
    }

    [MenuItem("CONTEXT/NodeFXEffect/Refresh")]
    static void Refresh(MenuCommand command)
    {
        NodeFXEffect target = (NodeFXEffect)command.context;
        target.Refresh();
    }
}