// Configuration.cs
// Created by Holojam Inc. on 03.04.17

using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools {

  /// <summary>
  /// This component enables runtime settings to be configured via
  /// an XML config file. The location of this file is customizable,
  /// but by default is in the same directory as the executable.
  /// See Holojam/SampleConfig.xml for an example of the config file format.
  /// </summary>
  public class Configuration : MonoBehaviour {

    /// <summary>
    /// Paths are relative to the directory of the executable, or in the case
    /// of the editor, relative to the project directory.
    /// </summary>
    public string configFilePath = "./HolojamConfig.xml";

    Queue<string> loadResult;

    void Awake() {
      loadResult = LoadConfiguration();
    }

    // Log load result
    void Start() {
      while (loadResult.Count > 0) {
        RemoteLogger.Log(
          "Holojam.Tools.Configuration: " + loadResult.Dequeue()
        );
      }
    }

    // Returns a queue of messages indicating the result of the load attempt
    Queue<string> LoadConfiguration() {
      XmlDocument configFile = new XmlDocument();
      FileInfo info = new FileInfo(configFilePath);
      var result = new Queue<string>();

      Tools.InfoPanel.SetString(
        "config", "Config: Not found"
      );

      // Load file
      try {
        configFile.Load(@configFilePath);
        Debug.Log(
          "Holojam.Tools.Configuration: Load successful at " + info.FullName
        );
        result.Enqueue("Load successful at " + configFilePath);
        Tools.InfoPanel.SetString(
          "config", "Config: Loaded"
        );
      }
      catch (FileNotFoundException) {
        result.Enqueue("File at " + info.FullName + " not found!");
        return result;
      }
      catch (DirectoryNotFoundException) {
        result.Enqueue("Directory " + info.FullName + " is invalid!");
        return result;
      }
      catch (System.Exception ex) {
        result.Enqueue("Error during load attempt: " + ex.Message);
        return result;
      }

      // Read nodes
      foreach (XmlNode node in configFile.DocumentElement.ChildNodes) {
        if (node.NodeType == XmlNodeType.Comment)
            continue;

        switch (node.Name) {
          case "RelayIP":
            Client client = GetComponent<Client>();
            if (client == null) {
              result.Enqueue(
                "Client component not found"
                + " (is the Configuration component alongside the Client component?)"
              );
              Tools.InfoPanel.SetString(
                "config", "Config: Loaded with errors"
              );
              break;
            }

            string ip = GetText(node);
            IPAddress dummy;
            if (!IPAddress.TryParse(ip, out dummy)) {
              // Invalid IP address
              result.Enqueue(
                "Error during read--relay IP \"" + ip + "\" is invalid"
              );
              Tools.InfoPanel.SetString(
                "config", "Config: Loaded with errors"
              );
              break;
            }

            client.ChangeRelayAddress(ip);
            break;

          case "BuildIndex":
            string buildIndexText = GetText(node);

            int buildIndex = -1;
            // Invalid index
            if (!int.TryParse(buildIndexText, out buildIndex)) {
              result.Enqueue(
                "Error during read--build index \"" + buildIndexText
                + "\" should be a number"
              );
              Tools.InfoPanel.SetString(
                "config", "Config: Loaded with errors"
              );
              break;
            }

            BuildManager.BUILD_INDEX = buildIndex;
            break;

          default:
            result.Enqueue(
              "Unknown option \"" + node.Name + "\""
            );
            Tools.InfoPanel.SetString(
              "config", "Config: Loaded with errors"
            );
            break;
        }
      }

      return result;
    }

    string GetText(XmlNode node) {
      foreach (XmlNode child in node.ChildNodes) {
        if (child.NodeType == XmlNodeType.Text)
          return child.Value;
      }
      return null;
    }
  }
};
