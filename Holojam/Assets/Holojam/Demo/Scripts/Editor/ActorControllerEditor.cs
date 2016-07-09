//ActorControllerEditor.cs
//Created by Aaron C Gaudette on 05.07.16

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ActorController))]
public class ActorControllerEditor : Editor{
	public override void OnInspectorGUI(){
		ActorController a = (ActorController)target;
		
		EditorGUILayout.BeginHorizontal();
			a.name=EditorGUILayout.TextField(a.name);
			a.motif=EditorGUILayout.ColorField(a.motif);
		EditorGUILayout.EndHorizontal();
		
		a.mask=EditorGUILayout.ObjectField("Mask",a.mask,typeof(GameObject),true) as GameObject;
		a.head=EditorGUILayout.ObjectField("Head",a.head,typeof(Transform),true) as Transform;
		
		EditorStyles.label.wordWrap = true;
		EditorGUILayout.LabelField(
			"Actor "+(a.index+1)+" ("+(a.managed?"Managed/":"Unmanaged/")+(a.view.IsTracked?"Tracked)":"Untracked)")
		);
	}
}