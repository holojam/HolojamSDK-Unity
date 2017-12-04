using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holojam.Network;

namespace Holojam.Components{
	public class Synchronizer  {
		Dictionary<string, int>dataLocMap;
		int vec3Count,vec4Count,intCount,floatCount,byteCount;
    Flake data;

		public Synchronizer(Flake data){
      this.data = data;
			this.dataLocMap = new Dictionary<string, int> ();
			this.vec3Count = 1;
			this.vec4Count = 1;
			this.intCount = 0;
			this.floatCount = 0;
			this.byteCount = 0;
		}

    public void Stage(int vec3Count=0, int vec4Count=0, int floatCount=0, int intCount=0, int byteCount=0,bool hasText=false){
      data = new Flake (vec3Count,vec4Count,floatCount,intCount,byteCount,hasText);
		}

    ///<summary>
    /// This is the a method group for syncing data. 
    /// Syncing allows user to use a string to indicate the data being synced, and don't have to worry about the the indexing of the Flake.
    ///</summary>
   
		public void SyncData(string name, Vector3 newData){
      if (!dataLocMap.ContainsKey (name)) {
        if (this.vec3Count < data.vector3s.Length) {
          dataLocMap.Add (name, this.vec3Count);
          this.vec3Count++;
        } else {
          Debug.LogError ("Totaly number of sync data is over the size of the staged data buffered size. Please resize your Flake package."); 
        }
      
      }
      this.data.vector3s [this.dataLocMap [name]] = newData;
		}

    public void SyncData(string name, Quaternion newData){
      if (!dataLocMap.ContainsKey (name)) {
        if (this.vec4Count < data.vector4s.Length) {
          dataLocMap.Add (name, this.vec4Count);
          this.vec4Count++;
        } else {
          Debug.LogError ("Totaly number of sync data is over the size of the staged data buffered size. Please resize your Flake package.");
        }
      }
      this.data.vector4s [this.dataLocMap [name]] = newData;
		}

		public void SyncData(string name, int newData){
      if (!dataLocMap.ContainsKey (name)) {
        if (this.intCount < data.ints.Length) {
          dataLocMap.Add (name, this.intCount);
          this.intCount++;
        } else {
          Debug.LogError ("Totaly number of sync data is over the size of the staged data buffered size. Please resize your Flake package.");
        }
      }
      this.data.ints [this.dataLocMap [name]] = newData;
		}

		public void SyncData(string name, float newData){
      if (!dataLocMap.ContainsKey (name)) {
          if (this.floatCount < data.floats.Length) {
            dataLocMap.Add (name, this.floatCount);
            this.floatCount++;
          } else {
            Debug.LogError ("Totaly number of sync data is over the size of the staged data buffered size. Please resize your Flake package.");
          }
      }
      this.data.floats [this.dataLocMap [name]] = newData;
		}

		public void SyncData(string name, byte newData){
      if (!dataLocMap.ContainsKey (name)) {
        if (this.byteCount < data.bytes.Length) {
          dataLocMap.Add (name, this.byteCount);
          this.byteCount++;
        } else {
          Debug.LogError ("Totaly number of sync data is over the size of the staged data buffered size. Please resize your Flake package.");
        }
       
      }
      this.data.bytes [this.dataLocMap [name]] = newData;
		}

    /// <summary>
    /// Functions used in sharedData and virtualObject Components to grab data over Holojam network. 
    /// Only the string that represented the synced data is needed.
    /// </summary>


		public Vector3 GetVec3(string name){
      int result;
      if (this.dataLocMap.TryGetValue (name, out result)) {
        return data.vector3s [result];
      } else {
        return Vector3.zero;
      }
      
		}
		public Quaternion GetVec4(string name){
      int result;
      if (this.dataLocMap.TryGetValue (name, out result)) {
        return data.vector4s [result];
      } else {
        return Quaternion.identity;

      }

		}
		public float GetFloat(string name){
      int result;
      if (this.dataLocMap.TryGetValue (name, out result)) {
        return data.floats [result];
      } else {
        return 1f;
      }
		}
		public int GetInt(string name){
      int result;
      if (this.dataLocMap.TryGetValue (name, out result)) {
        return data.ints [result];
      } else {
        return 1;
      }
		}
		public byte GetByte(string name){
      int result;
      if (this.dataLocMap.TryGetValue (name, out result)) {
        return data.bytes [result];
      } else {
        return (byte)1;
      }
		}
	}
}
