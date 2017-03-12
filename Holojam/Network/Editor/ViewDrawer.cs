// ViewDrawer.cs
// Created by Holojam Inc. on 16.02.17

using UnityEngine;
using UnityEditor;

namespace Holojam.Network {

  [CustomPropertyDrawer(typeof(View))]
  public class ViewDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      EditorGUI.BeginProperty(position, label, property);
      Controller controller = property.serializedObject.targetObject as Controller;
      EventListener listener = property.serializedObject.targetObject as EventListener;
      EventPusher pusher = property.serializedObject.targetObject as EventPusher;

      if (!controller && !listener && !pusher)
        return;

      string brand = controller ? controller.Brand :
        listener ? listener.Brand : pusher.Brand;

      Rect rect = new Rect(position.x, position.y, position.width, 16);
      GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
      if (Application.isPlaying)
        style.normal.textColor = controller ?
          controller.Tracked ?
            new Color(.5f, 1, .5f) : new Color(1, .5f, .5f) :
          listener && listener.Fired ?
            new Color(.5f, 1, .5f) :
          pusher && pusher.Fired ?
            new Color(.5f, 1, .5f) :
          new Color(.5f, 1, 1);

      EditorGUI.LabelField(rect, brand,
        Application.isPlaying ?
          controller ?
            controller.Sending ? "Sending" : controller.Deaf ? "Not Sending" :
            controller.Tracked ? "Tracked" : "Untracked" :
          listener ?
            listener.Fired ? "Fired" : "Event" :
          pusher ?
            pusher.Fired ? "Fired" : "Event" :
          "ERROR" : "Paused", style
      );

      SerializedProperty next = property.Copy();
      if (!next.NextVisible(true)) return; // Don't draw the line if there's nothing after this

      Rect split = new Rect(position.x, position.y + 22, position.width, 1);
      EditorGUI.DrawRect(split, Color.gray);

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      // Don't offset if there's nothing after this
      float offset = 0;
      SerializedProperty next = property.Copy();
      if (next.NextVisible(true)) offset = 12;

      return base.GetPropertyHeight(property, label) + offset;
    }

    void Update() { }
  }
}
