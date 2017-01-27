//SynchronizedTransform.cs
//Created by Aaron C Gaudette on 26.01.17
//Example Synchronizable

using UnityEngine;

public class SynchronizedTransform : Holojam.Tools.Synchronizable{
   //Position, rotation, scale
   protected override int triplesCount{get{return 2;}}
   protected override int quadsCount{get{return 1;}}

   public Vector3 position{
      get{return GetTriple(0);}
      set{UpdateTriple(0,value);}
   }
   public Quaternion rotation{
      get{return GetQuad(0);}
      set{UpdateQuad(0,value);}
   }
   public Vector3 scale{
      get{return GetTriple(1);}
      set{UpdateTriple(1,value);}
   }

   protected override void Sync(){
      if(sending){
         position = transform.position;
         rotation = transform.rotation;
         scale = transform.localScale;
      }else{
         transform.position = position;
         transform.rotation = rotation;
         transform.localScale = scale;
      }
   }
}
