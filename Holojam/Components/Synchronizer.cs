using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;

namespace Holojam.Components{

  public class Synchronizer  {

    Flake data;
    Dictionary<string, int> dataLocMap;
    int vec3Count, vec4Count, intCount, floatCount, byteCount;

    public Synchronizer(Flake data) {
      this.data = data;
      dataLocMap = new Dictionary<string, int>();

      vec3Count = 1;
      vec4Count = 1;
      intCount = 0;
      floatCount = 0;
      byteCount = 0;
    }

    public void Stage(
      int vec3Count = 0,
      int vec4Count = 0,
      int floatCount = 0,
      int intCount = 0,
      int byteCount = 0,
      bool hasText = false
    ) {
      data = new Flake(
        vec3Count,
        vec4Count,
        floatCount,
        intCount,
        byteCount,
        hasText
      );
    }

    ///<summary>
    /// Synchronize Vector3 data over the Holojam network.
    /// This allows the user to tag synchronized data with a string without
    /// worrying about indexing within the Flake.
    ///</summary>
    public void SyncData(string name, Vector3 newData) {
      if (!dataLocMap.ContainsKey(name)) {
        if (this.vec3Count < data.vector3s.Length) {
          dataLocMap.Add(name, this.vec3Count);
          this.vec3Count++;
        } else {
          Debug.LogError(
            "Attempted to synchronize more data than the Flake can support."
            + "Restage your Flake."
          );
        }
      }

      this.data.vector3s[this.dataLocMap[name]] = newData;
    }

    ///<summary>
    /// Synchronize Quaternion data over the Holojam network.
    /// This allows the user to tag synchronized data with a string without
    /// worrying about indexing within the Flake.
    ///</summary>
    public void SyncData(string name, Quaternion newData) {
      if (!dataLocMap.ContainsKey(name)) {
        if (this.vec4Count < data.vector4s.Length) {
          dataLocMap.Add(name, this.vec4Count);
          this.vec4Count++;
        } else {
          Debug.LogError(
            "Attempted to synchronize more data than the Flake can support."
            + "Restage your Flake."
          );
        }
      }

      this.data.vector4s[this.dataLocMap[name]] = newData;
    }

    ///<summary>
    /// Synchronize int data over the Holojam network.
    /// This allows the user to tag synchronized data with a string without
    /// worrying about indexing within the Flake.
    ///</summary>
    public void SyncData(string name, int newData) {
      if (!dataLocMap.ContainsKey(name)) {
        if (this.intCount < data.ints.Length) {
          dataLocMap.Add(name, this.intCount);
          this.intCount++;
        } else {
          Debug.LogError(
            "Attempted to synchronize more data than the Flake can support."
            + "Restage your Flake."
          );
        }
      }

      this.data.ints[this.dataLocMap[name]] = newData;
    }

    ///<summary>
    /// Synchronize float data over the Holojam network.
    /// This allows the user to tag synchronized data with a string without
    /// worrying about indexing within the Flake.
    ///</summary>
    public void SyncData(string name, float newData) {
      if (!dataLocMap.ContainsKey(name)) {
        if (this.floatCount < data.floats.Length) {
          dataLocMap.Add(name, this.floatCount);
          this.floatCount++;
        } else {
          Debug.LogError(
            "Attempted to synchronize more data than the Flake can support."
            + "Restage your Flake."
          );
        }
      }

      this.data.floats[this.dataLocMap[name]] = newData;
    }

    ///<summary>
    /// Synchronize byte data over the Holojam network.
    /// This allows the user to tag synchronized data with a string without
    /// worrying about indexing within the Flake.
    ///</summary>
    public void SyncData(string name, byte newData) {
      if (!dataLocMap.ContainsKey(name)) {
        if (this.byteCount < data.bytes.Length) {
          dataLocMap.Add(name, this.byteCount);
          this.byteCount++;
        } else {
          Debug.LogError(
            "Attempted to synchronize more data than the Flake can support."
            + "Restage your Flake."
          );
        }
      }

      this.data.bytes [this.dataLocMap [name]] = newData;
    }

    /// <summary>
    /// Grab tagged Vector3 data over the Holojam network.
    /// Only the string that represents the synchronized data is needed.
    /// </summary>
    public Vector3 GetVec3(string name) {
      int result;

      if (this.dataLocMap.TryGetValue(name, out result)) {
        return data.vector3s[result];
      } else {
        return Vector3.zero;
      }
    }

    /// <summary>
    /// Grab tagged Quaternion data over the Holojam network.
    /// Only the string that represents the synchronized data is needed.
    /// </summary>
    public Quaternion GetVec4(string name) {
      int result;

      if (this.dataLocMap.TryGetValue(name, out result)) {
        return data.vector4s[result];
      } else {
        return Quaternion.identity;
      }
    }

    /// <summary>
    /// Grab tagged float data over the Holojam network.
    /// Only the string that represents the synchronized data is needed.
    /// </summary>
    public float GetFloat(string name) {
      int result;

      if (this.dataLocMap.TryGetValue(name, out result)) {
        return data.floats[result];
      } else {
        return 1f;
      }
    }

    /// <summary>
    /// Grab tagged int data over the Holojam network.
    /// Only the string that represents the synchronized data is needed.
    /// </summary>
    public int GetInt(string name) {
      int result;

      if (this.dataLocMap.TryGetValue(name, out result)) {
        return data.ints[result];
      } else {
        return 1;
      }
    }

    /// <summary>
    /// Grab tagged byte data over the Holojam network.
    /// Only the string that represents the synchronized data is needed.
    /// </summary>
    public byte GetByte(string name) {
      int result;

      if (this.dataLocMap.TryGetValue(name, out result)) {
        return data.bytes[result];
      } else {
        return (byte)1;
      }
    }
  }
}
