// Packager.cs
// Created by Aaron C Gaudette on 21.06.16

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class Packager : EditorWindow{
  [MenuItem("Window/Packager")]
  public static void ShowWindow() {
    EditorWindow.GetWindow(typeof(Packager));
  }

  string output = "Holojam";
  string version = "0.0.0";
  string ignorePath = "Dev";
  bool clear = true;

  void OnGUI(){
    output = EditorGUILayout.TextField("Title", output);
    version = EditorGUILayout.TextField("Version", version);
    ignorePath = EditorGUILayout.TextField("Ignore Directory", ignorePath);
    clear = EditorGUILayout.Toggle("Clear on build", clear);

    if (GUILayout.Button("Build")) Build();
  }
  void Build(){
    List<string> subdirs = new List<string>();
    string[] paths = Directory.GetDirectories(
      Application.dataPath + "/HolojamSDK-Unity"
    );

    foreach (string p in paths) {
      Debug.Log(CleanPath(p) + " vs " + ignorePath);
      if (CleanPath(p) != "Assets/HolojamSDK-Unity/" + ignorePath)
        subdirs.Add(CleanPath(p));
    }

    if (subdirs.Count == 0) {
      Debug.LogWarning("Packager: No valid build directories");
      return;
    }

    if (clear)
      foreach (string f in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.unitypackage"))
        File.Delete(f);

    AssetDatabase.ExportPackage(
      subdirs.ToArray(), 
      output + "-v" + version + ".unitypackage", 
      ExportPackageOptions.Recurse | ExportPackageOptions.Interactive
    );
  }
  string CleanPath(string input) {
    return input.Substring(input.IndexOf("Assets"));
  }
}
