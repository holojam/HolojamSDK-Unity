//ViewDebugger.cs
//Created by Aaron C Gaudette on 13.07.16

using UnityEngine;

namespace Holojam.Tools{
   public class ViewDebugger : Controller{
      public string label, scope, source;
      public bool tracked, ignoreTracking;

      protected override ProcessDelegate Process{get{return UpdateData;}}

      protected override string labelField{get{return label;}}
      protected override string scopeField{get{return scope;}}
      protected override bool isSending{get{return false;}}

      public Vector3[] triples;
      public Quaternion[] quads;
      public float[] floats;
      public int[] ints;
      public byte[] chars;
      public string text;

      void UpdateData(){
         source = view.source;
         tracked = view.tracked;
         ignoreTracking = view.ignoreTracking;

         triples = view.triples;
         quads = view.quads;
         floats = view.floats;
         ints = view.ints;
         chars = view.chars;
         text = view.text;
      }
   }
}
