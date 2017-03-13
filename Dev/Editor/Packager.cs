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
  string extraPath;
  bool clear = true;

  readonly string repoName = "/HolojamSDK-Unity/";

  void OnGUI() {
    output = EditorGUILayout.TextField("Title", output);
    version = EditorGUILayout.TextField("Version", version);
    ignorePath = EditorGUILayout.TextField("Ignore Directory", ignorePath);
    extraPath = EditorGUILayout.TextField("Extra Ignore Directory", extraPath);
    clear = EditorGUILayout.Toggle("Clear on build", clear);

    if (GUILayout.Button("Build")) Build();
  }

  void Build() {
    List<string> subdirs = new List<string>();
    string[] paths = Directory.GetDirectories(
      Application.dataPath //+ repoName
    );

    foreach (string p in paths) {
      string path = CleanPath(p);
      if (path != "Assets" + /*repoName +*/ "/" + ignorePath
        && path != "Assets" + /*repoName +*/ "/" + extraPath
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
      output + "-v" + version + ".unitypackage", 
      ExportPackageOptions.Recurse | ExportPackageOptions.Interactive
    );
  }

  string CleanPath(string input) {
    return input.Substring(input.IndexOf("Assets"));
  }
}
