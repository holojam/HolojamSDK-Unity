//Synchronizable.cs
//Created by Aaron C Gaudette on 11.07.16

using UnityEngine;

namespace Holojam.Tools{
   public class Synchronizable : Controller{
      public bool useMasterPC = false; //update

      protected override ProcessDelegate Process{get{return Sync;}}

      protected override void UpdateViewSending(bool sending){
         sending = sending && (BuildManager.IsMasterPC() || !useMasterPC);
         view.sending = sending;
      }

      public Vector3 synchronizedVector3{
         get{return view.rawPosition;}
         set{if(sending)view.rawPosition=value;}
      }
      public Quaternion synchronizedQuaternion{
         get{return view.rawRotation;}
         set{if(sending)view.rawRotation=value;}
      }
      public int synchronizedInt{
         get{return view.bits;}
         set{if(sending)view.bits=value;}
      }
      public string synchronizedString{
         get{return view.blob;}
         set{if(sending)view.blob=value;}
      }

      //Override this in derived classes
      protected virtual void Sync(){
         //By default syncs transform data
         if(sending){
            synchronizedVector3 = transform.position;
            synchronizedQuaternion = transform.rotation;
         }else{
            transform.position = synchronizedVector3;
            transform.rotation = synchronizedQuaternion;
         }
      }
   }
}
