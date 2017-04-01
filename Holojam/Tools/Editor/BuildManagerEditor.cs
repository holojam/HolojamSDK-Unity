// BuildManagerEditor.cs
// Created by Holojam Inc. on 11.11.16

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Holojam.Tools {

  [CustomEditor(typeof(BuildManager))]
  public class BuildManagerEditor : Editor {

    SerializedProperty viewer; //, device;
    SerializedProperty preview, runtimeIndexing;

    string[] previewOptions;

    void OnEnable() {
      //device = serializedObject.FindProperty("device");
      preview = serializedObject.FindProperty("preview");
      //previewIndex = serializedObject.FindProperty("previewIndex");
      runtimeIndexing = serializedObject.FindProperty("runtimeIndexing");
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      //EditorGUILayout.PropertyField(device);
      EditorGUILayout.PropertyField(preview, new GUIContent(
        "Preview Mode"//, "Preview Unity build state for a specific actor"
      ));

      BuildManager buildManager = (BuildManager)serializedObject.targetObject;
      List<Actor> actors = new List<Actor>(Network.Controller.All<Actor>());

      actors.Sort((Actor a, Actor b) => {
        return a.index.CompareTo(b.index);
      });

      if (buildManager.preview) {
        EditorGUI.indentLevel++;

        previewOptions = new string[actors.Count + 1];
        previewOptions[0] = "NONE";
        for (int i = 0; i < actors.Count; ++i) {
          previewOptions[i + 1] = actors[i].gameObject.name + " (" + actors[i].index + ")";
        }

        buildManager.previewIndex = EditorGUILayout.Popup(
          "Target", buildManager.previewIndex, previewOptions
        );

        EditorUtility.SetDirty(buildManager);

        EditorGUI.indentLevel--;
      }
      EditorGUILayout.PropertyField(runtimeIndexing);

      GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);

      string label = !BuildManager.BUILD_ACTOR ? "None" :
        BuildManager.BUILD_ACTOR.Label;
      EditorGUILayout.LabelField(
        "Status",
        buildManager.preview ? (buildManager.previewIndex == 0 ?
          "Spectator" : ("Preview (" + label + ")")) :
        "Master Client",
        bold
      );

      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Actors", bold);
      EditorStyles.label.wordWrap = true;
      //GUIStyle style = new GUIStyle();

      foreach (Actor a in actors) {
        //style.normal.textColor = a.debugColor;
        EditorGUILayout.LabelField("   " + a.gameObject.name + " [" + a.Brand + "] " +
          (a.IsLocal ? "(Local) " : "") + (a.IsBuild ? "(Build)" : "")//, style
        );
      }

      if (actors.Count == 0)
        EditorGUILayout.LabelField("   (None)");

      serializedObject.ApplyModifiedProperties();
    }
  }
}
