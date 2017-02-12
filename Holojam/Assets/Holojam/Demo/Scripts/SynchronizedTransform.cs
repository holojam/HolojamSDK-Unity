// SynchronizedTransform.cs
// Created by Holojam Inc. on 26.01.17
// Example Synchronizable

using UnityEngine;

public class SynchronizedTransform : Holojam.Tools.Synchronizable{

   [SerializeField] private string label = "Synchronizable";
   [SerializeField] private string scope = "";

   // Position, rotation, scale
   public override int TripleCount{get{return 2;}}
   public override int QuadCount{get{return 1;}}

   public override string Label { get { return label; } }
   public override string Scope { get { return scope; } }

   // Proxies
   public Vector3 Position{
      get{return GetTriple(0);}
      set{SetTriple(0,value);}
   }
   public Quaternion Rotation{
      get{return GetQuad(0);}
      set{SetQuad(0,value);}
   }
   public Vector3 Scale{
      get{return GetTriple(1);}
      set{SetTriple(1,value);}
   }

   protected override void Sync(){
      if(Sending){
         Position = transform.position;
         Rotation = transform.rotation;
         Scale = transform.localScale;
      }else{
         transform.position = Position;
         transform.rotation = Rotation;
         transform.localScale = Scale;
      }
   }
}
