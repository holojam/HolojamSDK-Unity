// Configuration.cs
// Created by Holojam Inc. on 03.04.17

using System.Net;
using System.IO;
using System.Xml;
using UnityEngine;
using Holojam.Network;

namespace Holojam.Tools {

  /// <summary>
  /// This component allows certain settings to be set via an XML config file. The location of this
  /// file is customizable, but by default is in the same directory as the executable.
  /// Add this to the main Holojam object, where the Client component is, to enable config file reading.
  /// See Holojam/SampleConfig.xml for an example of the config file format.
  /// </summary>
  public class Configuration : MonoBehaviour {

    /// <summary>
    /// This should point to the location of the config file. Relative paths are relative to the
    /// directory the executable is in, or when in the Unity editor relative to the main project
    /// directory.
    /// </summary>
    public string configFilePath = "./HolojamConfig.xml";

    void Awake() {
      LoadConfiguration();
    }

    void LoadConfiguration() {
      XmlDocument configFile = new XmlDocument();
      try {
        configFile.Load(configFilePath);
        RemoteLogger.Log(
          "Holojam.Tools.Configuration: Holojam configuration file found and loaded."
        );
      }
      catch (FileNotFoundException) {
        RemoteLogger.Log(
          "Holojam.Tools.Configuration: Holojam configuration file at "
          + configFilePath + " not found."
        );
        return;
      }
      catch (XmlException ex) {
        RemoteLogger.Log(
          "Holojam.Tools.Configuration: Error reading Holojam configuration file: "
          + ex.Message
        );
        return;
      }

      foreach (XmlNode node in configFile.DocumentElement.ChildNodes) {
        if (node.NodeType == XmlNodeType.Comment) {
            continue;
        }
        switch (node.Name) {
        case "RelayIP":
          Client client = GetComponent<Client>();
          if (client == null) {
            RemoteLogger.Log(
              "Holojam.Tools.Configuration: Component should be added to the same GameObject "
              + " as the Holojam Client."
            );
            break;
          }
          string ip = GetText(node);
          IPAddress dummy;
          if (!IPAddress.TryParse(ip, out dummy)) {
            // The IP address is invalid
            RemoteLogger.Log(
              "Holojam.Tools.Configuration: "
              + "Error in Holojam configuration file--relay IP should be a valid IP"
              + " address, instead got \"" + ip + "\"."
            );
            break;
          }
          client.ChangeRelayAddress(ip);
          break;
        case "BuildIndex":
          string buildIndexText = GetText(node);
          int buildIndex = -1;
          if (!int.TryParse(buildIndexText, out buildIndex)) {
            RemoteLogger.Log(
              "Holojam.Tools.Configuration: "
              + "Error in Holojam configuration file--build index should be a number,"
              + " instead got \"" + buildIndexText + "\"."
            );
            break;
          }
          BuildManager.BUILD_INDEX = buildIndex;
          break;
        default:
          RemoteLogger.Log(
            "Holojam.Tools.Configuration: Unknown option \"" + node.Name
            + "\" found in Holojam config file at " + configFilePath
          );
          break;
        }
      }
    }

    string GetText(XmlNode node) {
      foreach (XmlNode child in node.ChildNodes) {
        if (child.NodeType == XmlNodeType.Text) {
          return child.Value;
        }
      }
      return null;
    }
  }
};
