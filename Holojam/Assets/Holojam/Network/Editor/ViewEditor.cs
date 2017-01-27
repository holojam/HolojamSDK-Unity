//ViewEditor.cs
//Created by Aaron C Gaudette on 11.11.16

using UnityEngine;
using UnityEditor;
using Holojam.Network;

namespace Holojam.Network{
   [CustomEditor(typeof(View))]
   public class ViewEditor : Editor{
      public override void OnInspectorGUI(){
         View view = (View)target;
         GUIStyle style = new GUIStyle(EditorStyles.boldLabel);

         if(Application.isPlaying)
            style.normal.textColor=
               view.ignoreTracking?new Color(0.5f,1,1):
               view.tracked?new Color(0.5f,1,0.5f):new Color(1,0.5f,0.5f);

         bool noLabel = string.IsNullOrEmpty(view.label);
         EditorGUILayout.LabelField(noLabel?"No Label":view.scope+"."+view.label,
            noLabel?"":
            view.ignoreTracking?"Event":
            (view.sending && view.tracked)?"Sending":
            view.sending?"Paused":
            view.tracked?"Tracked":"Untracked",
            style
         );

         EditorUtility.SetDirty(serializedObject.targetObject);
      }
   }
}
