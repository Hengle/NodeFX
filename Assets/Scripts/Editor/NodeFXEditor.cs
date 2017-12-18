using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeFX;
using System;
using System.IO;

[CustomEditor(typeof(NodeFXEffect))]
public class NodeFXEditor : Editor {

	NodeFXEffect targetEffect;

	private FileSystemWatcher _fileSystemWatcher;
	private GUIStyle headerStyle = new GUIStyle();

	void OnEnable() {
		targetEffect = (NodeFXEffect)target;
		GenerateStyles();
        targetEffect._isDirty = true;
	}

	public override void OnInspectorGUI()
    {
		GUIDrawHeader();

        targetEffect.effectDefinition = (TextAsset)EditorGUILayout.ObjectField("Effect Definition", targetEffect.effectDefinition, typeof(TextAsset),false);
		
        GUIDrawTopShelfButtons();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUIDrawRefreshToggles();

        CheckForUpdates();
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
        targetEffect.enableRefresh = EditorGUILayout.BeginToggleGroup("Automatic Refresh", targetEffect.enableRefresh);

        targetEffect.refreshOnFocus = EditorGUILayout.Toggle("Refresh On Window Focus", targetEffect.refreshOnFocus);
        targetEffect.refreshOnFileChange = EditorGUILayout.Toggle("Refresh On File Change", targetEffect.refreshOnFileChange);
		targetEffect.refreshAtInterval = EditorGUILayout.Toggle("Refresh At Time Interval", targetEffect.refreshAtInterval);

        if (targetEffect.refreshAtInterval)
        {
            targetEffect.refreshInterval = EditorGUILayout.FloatField("Interval (in seconds)", targetEffect.refreshInterval);
        }
		
        EditorGUILayout.EndToggleGroup();
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

    private void CheckForUpdates()
    {
        Debug.Log("Editor: Checking for updates");
        Debug.Log(targetEffect._isDirty);
        if (targetEffect.refreshAtInterval == true) {
			if (targetEffect._isDirty) {
                Debug.Log("Editor: Effect is dirty");
				targetEffect._isDirty = false;
				targetEffect.Refresh();
				UpdateOnInterval();
			}
		}

		if (_fileSystemWatcher == null) {
			CreateFileWatcher();
		}

		if (targetEffect.refreshOnFileChange) {
				_fileSystemWatcher.EnableRaisingEvents = true;
		} else {
				_fileSystemWatcher.EnableRaisingEvents = false;
		}
    }

    private void CreateFileWatcher()
    {
		_fileSystemWatcher = new FileSystemWatcher();
		targetEffect.Refresh();
		_fileSystemWatcher.Path = targetEffect.path;

		/* Watch for changes in LastAccess and LastWrite times, and 
		the renaming of files or directories. */
		_fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite 
		| NotifyFilters.FileName | NotifyFilters.DirectoryName;
		
		// Only watch xml files.
		_fileSystemWatcher.Filter = "*.xml";

		// Add event handlers.
		_fileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
		_fileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);
		_fileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
		_fileSystemWatcher.Renamed += new RenamedEventHandler(OnRenamed);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        targetEffect.Refresh();
    }

    IEnumerator UpdateOnInterval() {
        Debug.Log("Editor: Refreshing in " + targetEffect.refreshInterval + " seconds");
		yield return new WaitForSeconds(targetEffect.refreshInterval);
		targetEffect._isDirty = true;
	}

	private void OnApplicationFocus() {
		if (targetEffect.refreshOnFocus) {
			targetEffect.Refresh();
		}
	}

    private void OnButtonOpenEditor()
    {
        throw new NotImplementedException();
    }

    private void OnButtonRefresh() {
		targetEffect.Refresh();
	}

	private void GenerateStyles() {
		headerStyle.alignment = TextAnchor.UpperCenter;
		headerStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/nodefx_logo.png");
	}
} 
