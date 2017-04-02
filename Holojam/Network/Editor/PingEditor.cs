// PingEditor.cs
// Created by Holojam Inc. on 01.04.17

using UnityEngine;
using UnityEditor;

namespace Holojam.Network {

  [CustomEditor(typeof(Ping))]
  public class PingEditor : Editor {

    public override void OnInspectorGUI() {
      serializedObject.Update();

      Ping ping = (Ping)serializedObject.targetObject;
      int latency = ping.CorrectedLatency;

      GUIStyle style = new GUIStyle(EditorStyles.boldLabel);

      if(Application.isPlaying) {
        style.normal.textColor = latency > 100 || !ping.Connected ? new Color(1, .5f, .5f) :
          latency > 50 ? new Color(1, 1, .5f) : new Color(.5f, 1, .5f);
      }

      EditorGUILayout.LabelField(
        "Latency", Application.isPlaying ? ping.Connected ?
          (latency + "ms") : "Disconnected" : "Paused", style
      );

      EditorUtility.SetDirty(ping);
      serializedObject.ApplyModifiedProperties();
    }
  }
}
