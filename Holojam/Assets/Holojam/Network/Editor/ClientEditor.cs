//ClientEditor.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Network{
   [CustomEditor(typeof(Client))]
   public class ClientEditor : Editor{
      static bool advanced = false;

      SerializedProperty serverAddress, serverPort, multicastAddress, multicastPort;
      SerializedProperty sendScope;
      void OnEnable(){
         serverAddress = serializedObject.FindProperty("serverAddress");
         serverPort = serializedObject.FindProperty("serverPort");
         multicastAddress = serializedObject.FindProperty("multicastAddress");
         multicastPort = serializedObject.FindProperty("multicastPort");
         sendScope = serializedObject.FindProperty("sendScope");
      }

      public override void OnInspectorGUI(){
         serializedObject.Update();

         EditorGUILayout.PropertyField(serverAddress);
         EditorGUILayout.PropertyField(sendScope);

         EditorGUIUtility.labelWidth = 64;
         GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);
         GUIStyle style = new GUIStyle(EditorStyles.boldLabel);

         EditorGUILayout.Space();
         EditorGUILayout.LabelField("Packets per Second",bold);

         Client client = (Client)serializedObject.targetObject;
         if(Application.isPlaying)
            style.normal.textColor = client.sentPPS>0?
               new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
         EditorGUILayout.LabelField("Sent:",client.sentPPS.ToString(),style);

         if(Application.isPlaying)
            style.normal.textColor = client.receivedPPS>0?
               new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
         EditorGUILayout.LabelField("Received",client.receivedPPS.ToString(),style);

         EditorGUILayout.Space();
         EditorGUILayout.LabelField("Views",bold);
         EditorStyles.label.wordWrap = true;
         foreach(string s in client.threadData)
            EditorGUILayout.LabelField(s);
         if(!Application.isPlaying)
            EditorGUILayout.LabelField("   (Paused)");

         /*
         EditorGUILayout.Space();
         EditorGUILayout.LabelField("Views",bold);
         style = new GUIStyle();
         foreach(HolojamView v in HolojamView.instances){
            if(Application.isPlaying)
               style.normal.textColor = v.IsTracked?
                  new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);
            EditorGUILayout.LabelField("  "+v.Label+(v.IsMine?" (S)":""),style);
         }
         */

         EditorGUILayout.Space();
         EditorGUIUtility.labelWidth = 0;
         advanced = EditorGUILayout.Foldout(advanced,"Advanced");
         if(advanced){
            EditorGUILayout.PropertyField(serverPort);
            EditorGUILayout.PropertyField(multicastAddress);
            EditorGUILayout.PropertyField(multicastPort);
         }

         serializedObject.ApplyModifiedProperties();
      }
   }
}
