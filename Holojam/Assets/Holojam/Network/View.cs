//View.cs
//Created by Aaron C Gaudette on 11.11.16
//Unity representation of (deprecated) protobuf object

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

      /* TMP */
      [HideInInspector] public Vector3[] triples;
      [HideInInspector] public Quaternion[] quads;
      [HideInInspector] public float[] floats;
      [HideInInspector] public int[] ints;
      [HideInInspector] public byte[] chars;
      [HideInInspector] public string text;
      /* TMP */

      [HideInInspector] public string label;
      bool isTracked = false;
      public bool tracked{
         get{return Application.isPlaying && instances.Contains(this) && (isTracked || sending);}
         set{isTracked = value;}
      }

      [HideInInspector] public Vector3 rawPosition;
      [HideInInspector] public Quaternion rawRotation;
      [HideInInspector] public int bits;
      [HideInInspector] public string blob;
   }
}
