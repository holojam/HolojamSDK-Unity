// ClientEditor.cs
// Created by Holojam Inc. on 11.11.16

using UnityEngine;
using UnityEditor;

namespace Holojam.Network {

  [CustomEditor(typeof(Client))]
  public class ClientEditor : Editor {

    const int ORDER = -16000;

    SerializedProperty relayAddress, upstreamPort, multicastAddress, downstreamPort, verboseLogs;
    SerializedProperty sendScope, rate;

    SerializedProperty ediff, sdiff;

    string newRelayAddress = "?";
    int newUpstreamPort = -1;
    string newMulticastAddress = "?";
    int newDownstreamPort = -1;

    void OnEnable() {
      if (!Application.isPlaying) {
        MonoScript client = MonoScript.FromMonoBehaviour((Client)serializedObject.targetObject);
        if (MonoImporter.GetExecutionOrder(client) != ORDER) {
          MonoImporter.SetExecutionOrder(client, ORDER);
          Debug.Log(
            "Holojam.Network.Client: Initialized execution order (" + ORDER + ")",
            serializedObject.targetObject
          );
        }
      }

      relayAddress = serializedObject.FindProperty("relayAddress");
      upstreamPort = serializedObject.FindProperty("upstreamPort");
      multicastAddress = serializedObject.FindProperty("multicastAddress");
      downstreamPort = serializedObject.FindProperty("downstreamPort");
      sendScope = serializedObject.FindProperty("sendScope");
      rate = serializedObject.FindProperty("rate");
      verboseLogs = serializedObject.FindProperty("verboseLogs");

      ediff = serializedObject.FindProperty("ediff");
      sdiff = serializedObject.FindProperty("sdiff");

      newRelayAddress = relayAddress.stringValue;
      newUpstreamPort = upstreamPort.intValue;
      newMulticastAddress = multicastAddress.stringValue;
      newDownstreamPort = downstreamPort.intValue;
    }

    public override void OnInspectorGUI() {
      serializedObject.Update();

      Client client = (Client)serializedObject.targetObject;

      if (Application.isPlaying) {
        // At runtime, we need to go through the API to change the server address so that
        // we restart the sending and receiving threads and so on.
        newRelayAddress = EditorGUILayout.TextField("Relay Address", newRelayAddress);
      } else {
        // Otherwise, we can just change the serialized property directly.
        EditorGUILayout.PropertyField(relayAddress);
      }
      EditorGUILayout.PropertyField(sendScope);

      // "Apply changes" button should only be shown if we changed the IP of the server
      // while at runtime
      if (newRelayAddress != relayAddress.stringValue && Application.isPlaying) {
        if (GUILayout.Button("Apply changes")) {
          client.ChangeRelayAddress(newRelayAddress);
        }
      }

      EditorGUIUtility.labelWidth = 64;
      GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);
      GUIStyle style = new GUIStyle(EditorStyles.boldLabel);

      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Packets per Second", bold);

      if(Application.isPlaying)
        style.normal.textColor = client.sentPPS > 0 ?
          new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);
      EditorGUILayout.LabelField("Sent:", client.sentPPS.ToString(), style);

      if(Application.isPlaying)
        style.normal.textColor = client.receivedPPS > 0 ?
          new Color(0.5f, 1, 0.5f) : new Color(1, 0.5f, 0.5f);
      EditorGUILayout.LabelField("Received", client.receivedPPS.ToString(), style);

      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Flakes", bold);
      EditorStyles.label.wordWrap = true;
      foreach(string s in client.threadData)
        EditorGUILayout.LabelField(s);
      if(!Application.isPlaying)
        EditorGUILayout.LabelField("   (Paused)");

      EditorGUILayout.Space();
      EditorGUIUtility.labelWidth = 0;
      client.advanced = EditorGUILayout.Foldout(client.advanced, "Advanced");
      if(client.advanced) {
        if (Application.isPlaying) {
          // At runtime, we need to go through the API to change these properties so that we
          // restart the sending and receiving threads and so on.
          newUpstreamPort = EditorGUILayout.IntField("Upstream Port", newUpstreamPort);
          newMulticastAddress = EditorGUILayout.TextField("Multicast Address", newMulticastAddress);
          newDownstreamPort = EditorGUILayout.IntField("Downstream Port", newDownstreamPort);
        } else {
          // Otherwise, we can just change the serialized property directly.
          EditorGUILayout.PropertyField(upstreamPort);
          EditorGUILayout.PropertyField(multicastAddress);
          EditorGUILayout.PropertyField(downstreamPort);
        }
        EditorGUILayout.PropertyField(rate);

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUILayout.PropertyField(verboseLogs);
        EditorGUI.EndDisabledGroup();

        // "Apply changes" button should only be shown if we changed these settings at runtime
        if (Application.isPlaying && (
          newUpstreamPort != upstreamPort.intValue
          || newMulticastAddress != multicastAddress.stringValue
          || newDownstreamPort != downstreamPort.intValue)
        ) {
          if (GUILayout.Button("Apply changes")) {
            client.ChangeClientSettings(
              client.RelayAddress, newUpstreamPort, newMulticastAddress, newDownstreamPort
            );
          }
        }

        if (ediff != null) {
          EditorGUILayout.LabelField(
            "ediff / sdiff",
            ediff.floatValue.ToString() + " / " + sdiff.floatValue.ToString()
          );
        }
      }

      serializedObject.ApplyModifiedProperties();
    }
  }
}
