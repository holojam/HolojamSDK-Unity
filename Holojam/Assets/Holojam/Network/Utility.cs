using UnityEngine;
using ProtoBuf;
using update_protocol_v3;

namespace Holojam.Network{
   //Interim class for transferring data between the network and the view
   class SwapFlake{
      public static readonly Vector3 DEFAULT_POSITION = Vector3.zero;
      public static readonly Quaternion DEFAULT_ROTATION = Quaternion.identity;

      public string scope;
      public string source;
      public string label;
      public bool isTracked;
      public Vector3 position;
      public Quaternion rotation;
      public int bits;
      public string blob;

      public SwapFlake(string scope, string source, string label){
         this.scope = scope;
         this.source = source;
         this.label = label;
         isTracked = false;
         position = DEFAULT_POSITION;
         rotation = DEFAULT_ROTATION;
         bits = 0;
         blob = "";
      }

      //Conversions
      public static SwapFlake FromView(View view){
         SwapFlake f = new SwapFlake(view.scope,view.source,view.label);

         f.isTracked = view.tracked;
         f.position = view.rawPosition;
         f.rotation = view.rawRotation;
         f.bits = view.bits;
         f.blob = view.blob;

         return f;
      }
      public update_protocol_v3.LiveObject ToFlake(){
         update_protocol_v3.LiveObject f = new update_protocol_v3.LiveObject();
         f.label = this.label;
         f.is_tracked = isTracked;

         f.x = position.x;
         f.y = position.y;
         f.z = position.z;

         f.qx = rotation.x;
         f.qy = rotation.y;
         f.qz = rotation.z;
         f.qw = rotation.w;

         f.button_bits = bits;

         if(!string.IsNullOrEmpty(blob))
            f.extra_data = blob;

         return f;
      }
   }

   public class Canon{
      public static string IndexToLabel(int index, bool raw = false){
         return "M"+(Mathf.Max(0,index)+1)+(raw?"-Raw":"");
      }
   }
}
