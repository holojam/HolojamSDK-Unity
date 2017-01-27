//View.cs
//Created by Aaron C Gaudette on 11.11.16
//Unity representation of flatbuffers object

using UnityEngine;
using System.Collections.Generic;

namespace Holojam.Network{
   public class View : MonoBehaviour{
      public static List<View> instances = new List<View>();
      void OnEnable(){instances.Add(this);}
      void OnDisable(){instances.Remove(this);}

      [HideInInspector] public string scope;
      [HideInInspector] public string source;
      [HideInInspector] public bool sending;
      [HideInInspector] public bool ignoreTracking;

      [HideInInspector] public string label;

      [HideInInspector] public Vector3[] triples;
      [HideInInspector] public Quaternion[] quads;
      [HideInInspector] public float[] floats;
      [HideInInspector] public int[] ints;
      [HideInInspector] public byte[] chars;
      [HideInInspector] public string text;

      bool isTracked = false;
      public bool tracked{
         get{return Application.isPlaying && instances.Contains(this)
            && (isTracked || sending);}
         set{isTracked = value;}
      }
   }
}
