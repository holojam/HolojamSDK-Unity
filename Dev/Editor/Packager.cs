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

  string version = "0.0.0";
  string ignore0 = "Dev";
  string ignore1;
  bool clear = true;

  void OnGUI() {
    version = EditorGUILayout.TextField("Version", version);
    ignore0 = EditorGUILayout.TextField("Ignore", ignore0);
    ignore1 = EditorGUILayout.TextField("Ignore", ignore1);
    clear = EditorGUILayout.Toggle("Clear on build", clear);

    if (GUILayout.Button("Build")) Build();
  }

  void Build() {
    List<string> subdirs = new List<string>();
    string[] paths = Directory.GetDirectories(
      Application.dataPath
    );

    foreach (string p in paths) {
      string path = CleanPath(p);
      if (path != "Assets" + "/" + ignore0
        && path != "Assets" + "/" + ignore1
      ) {
        subdirs.Add(path);
      }
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
      "Holojam_v" + version + ".unitypackage", 
      ExportPackageOptions.Recurse | ExportPackageOptions.Interactive
    );
  }

  string CleanPath(string input) {
    return input.Substring(input.IndexOf("Assets"));
  }
}
