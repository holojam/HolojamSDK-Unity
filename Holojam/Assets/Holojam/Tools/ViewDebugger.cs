// ViewDebugger.cs
// Created by Holojam Inc. on 13.07.16

using UnityEngine;

namespace Holojam.Tools{
   /// <summary>
   /// Read-only data dump of a View.
   /// Deprecated.
   /// </summary>
   public sealed class ViewDebugger : Controller{
      public string label, scope, source;
      public bool tracked, ignoreTracking;

      protected override ProcessDelegate Process{get{return UpdateData;}}

      public override string Label{get{return label;}}
      public override string Scope{get{return scope;}}
      public override bool Sending{get{return false;}}

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
