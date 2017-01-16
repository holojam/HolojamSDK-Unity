//Utility.cs
//Created by Aaron C Gaudette

using UnityEngine;
using FlatBuffers;
using System.Collections.Generic;

namespace Holojam.Network{
   class Update{
      public readonly bool valid;
      readonly Dictionary<string,int> lookup;
      readonly Protocol.Packet packet;

      public Update(ref byte[] buffer){
         packet = Protocol.Packet.GetRootAsPacket(new ByteBuffer(buffer));
         lookup = new Dictionary<string,int>();
         //Verify this is an update, not an event
         if(packet.Type==Protocol.PacketType.Update){
            valid = true;
            //Initialize lookup
            for(int i=0;i<packet.FlakesLength;++i)
               lookup[packet.Flakes(i).Value.Label] = i;
         }else valid = false;
      }

      //Load flake data into view
      public bool Load(View view){
         if(view.scope!=packet.Scope && view.scope!="")return false;

         if(lookup.ContainsKey(view.label)){
            CopyToView(lookup[view.label],view);
            return true;
         }
         return false;
      }
      //Copy from flake struct to view component
      void CopyToView(int i, View view){
         view.triples = new Vector3[packet.Flakes(i).Value.TriplesLength];
         for(int j=0;j<packet.Flakes(i).Value.TriplesLength;++j)
            view.triples[j] = new Vector3(
               packet.Flakes(i).Value.Triples(j).Value.X,
               packet.Flakes(i).Value.Triples(j).Value.Y,
               packet.Flakes(i).Value.Triples(j).Value.Z
            );
         view.quads = new Quaternion[packet.Flakes(i).Value.QuadsLength];
         for(int j=0;j<packet.Flakes(i).Value.QuadsLength;++j)
            view.quads[j] = new Quaternion(
               packet.Flakes(i).Value.Quads(j).Value.X,
               packet.Flakes(i).Value.Quads(j).Value.Y,
               packet.Flakes(i).Value.Quads(j).Value.Z,
               packet.Flakes(i).Value.Quads(j).Value.W
            );

         view.floats = new float[packet.Flakes(i).Value.FloatsLength];
         for(int j=0;j<packet.Flakes(i).Value.FloatsLength;++j)
            view.floats[j] = packet.Flakes(i).Value.Floats(j);

         view.ints = new int[packet.Flakes(i).Value.IntsLength];
         for(int j=0;j<packet.Flakes(i).Value.IntsLength;++j)
            view.ints[j] = packet.Flakes(i).Value.Ints(j);

         view.chars = new byte[packet.Flakes(i).Value.CharsLength];
         for(int j=0;j<packet.Flakes(i).Value.CharsLength;++j)
            view.chars[j] = (byte)packet.Flakes(i).Value.Chars(j); //FLAG

         view.text = packet.Flakes(i).Value.Text;
      }

      #if UNITY_EDITOR
      public void UpdateDebug(Dictionary<string,string> debugData){
         string scope = packet.Scope;
         for(int i=0;i<packet.FlakesLength;++i)
            debugData[scope + "." + packet.Flakes(i).Value.Label] = packet.Origin;
      }
      #endif
   }

   class Translator{
      public static string Origin(){
         return System.Environment.UserName + "@" + System.Environment.MachineName;
      }

      public static byte[] BuildUpdate(List<View> views, string sendScope){
         FlatBufferBuilder builder = new FlatBufferBuilder(1024);

         StringOffset scope = builder.CreateString(sendScope);
         StringOffset origin = builder.CreateString(Origin());

         Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[views.Count];

         VectorOffset triples, quads;
         VectorOffset floats, ints, chars;
         StringOffset text = default(StringOffset);
         triples = quads = floats = ints = chars = default(VectorOffset);

         for(int i=0;i<views.Count;++i){
            StringOffset label = builder.CreateString(views[i].label);

            //Create the vectors
            if(views[i].triples!=null){
               Protocol.Flake.StartTriplesVector(builder,views[i].triples.Length);
               for(int j=views[i].triples.Length-1;j>0;--j){
                  Protocol.Vector3.CreateVector3(builder,
                     views[i].triples[j].x,
                     views[i].triples[j].y,
                     views[i].triples[j].z
                  );
               }
               triples = builder.EndVector();
            }
            if(views[i].quads!=null){
               Protocol.Flake.StartQuadsVector(builder,views[i].quads.Length);
               for(int j=0;j<views[i].quads.Length;++j){
                  Protocol.Vector4.CreateVector4(builder,
                     views[i].quads[j].x,
                     views[i].quads[j].y,
                     views[i].quads[j].z,
                     views[i].quads[j].w
                  );
               }
               quads = builder.EndVector();
            }
            if(views[i].floats!=null)
               floats = Protocol.Flake.CreateFloatsVector(builder,views[i].floats);
            if(views[i].ints!=null)
               ints = Protocol.Flake.CreateIntsVector(builder,views[i].ints);
            if(views[i].chars!=null){
               Protocol.Flake.StartCharsVector(builder,views[i].chars.Length);
               for(int j=views[i].chars.Length-1;j>0;--j)
                  builder.AddByte(views[i].chars[j]); //FLAG
               chars = builder.EndVector();
            }
            if(views[i].text!=null)
               text = builder.CreateString(views[i].text);

            //Put it all together
            Protocol.Flake.StartFlake(builder);
            Protocol.Flake.AddLabel(builder,label);

            if(views[i].triples!=null)
               Protocol.Flake.AddTriples(builder,triples);
            if(views[i].quads!=null)
               Protocol.Flake.AddQuads(builder,quads);
            if(views[i].floats!=null)
               Protocol.Flake.AddFloats(builder,floats);
            if(views[i].ints!=null)
               Protocol.Flake.AddInts(builder,ints);
            if(views[i].chars!=null)
               Protocol.Flake.AddChars(builder,chars);
            if(views[i].text!=null)
               Protocol.Flake.AddText(builder,text);

            offsets[i] = Protocol.Flake.EndFlake(builder);
         }
         var flakes = Protocol.Packet.CreateFlakesVector(builder,offsets);

         //Build packet
         Offset<Protocol.Packet> packet = Protocol.Packet.CreatePacket(
            builder,scope,origin,Protocol.PacketType.Update,flakes
         );
         builder.Finish(packet.Value);
         //Return buffer
         return builder.SizedByteArray();
      }
   }

   public class Canon{
      public static string IndexToLabel(int index, bool raw = false){
         return "M"+(Mathf.Max(0,index)+1)+(raw?"-Raw":"");
      }
   }
}
