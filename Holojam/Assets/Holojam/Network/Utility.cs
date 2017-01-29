//Utility.cs
//Created by Aaron C Gaudette

using UnityEngine;
using FlatBuffers;
using System;
using System.Collections.Generic;

namespace Holojam.Network{

   internal class PacketWrapper{
      public readonly Protocol.Packet data;

      public PacketWrapper(ref byte[] buffer){
         data = Protocol.Packet.GetRootAsPacket(new ByteBuffer(buffer));
      }

      //Copy from flake struct to view component
      public void CopyToView(int i, View view){
         view.scope = data.Scope;

         view.label = data.Flakes(i).Value.Label;
         view.triples = new Vector3[data.Flakes(i).Value.TriplesLength];
         for(int j=0;j<data.Flakes(i).Value.TriplesLength;++j)
            view.triples[j] = new Vector3(
               data.Flakes(i).Value.Triples(j).Value.X,
               data.Flakes(i).Value.Triples(j).Value.Y,
               data.Flakes(i).Value.Triples(j).Value.Z
            );
         view.quads = new Quaternion[data.Flakes(i).Value.QuadsLength];
         for(int j=0;j<data.Flakes(i).Value.QuadsLength;++j)
            view.quads[j] = new Quaternion(
               data.Flakes(i).Value.Quads(j).Value.X,
               data.Flakes(i).Value.Quads(j).Value.Y,
               data.Flakes(i).Value.Quads(j).Value.Z,
               data.Flakes(i).Value.Quads(j).Value.W
            );

         view.floats = new float[data.Flakes(i).Value.FloatsLength];
         for(int j=0;j<data.Flakes(i).Value.FloatsLength;++j)
            view.floats[j] = data.Flakes(i).Value.Floats(j);

         view.ints = new int[data.Flakes(i).Value.IntsLength];
         for(int j=0;j<data.Flakes(i).Value.IntsLength;++j)
            view.ints[j] = data.Flakes(i).Value.Ints(j);

         view.chars = new byte[data.Flakes(i).Value.CharsLength];
         for(int j=0;j<data.Flakes(i).Value.CharsLength;++j)
            view.chars[j] = (byte)data.Flakes(i).Value.Chars(j); //FLAG

         view.text = data.Flakes(i).Value.Text;
      }
   }

   abstract internal class Nugget{
      protected PacketWrapper packet;

      protected Nugget(PacketWrapper packet){
         this.packet = packet;
      }

      public abstract bool Load(View view);

      public static Nugget Create(ref byte[] buffer){
         PacketWrapper packet = new PacketWrapper(ref buffer);
         switch(packet.data.Type){
            case Protocol.PacketType.Update:
               return new Update(packet);
            case Protocol.PacketType.Event:
               return new Event(packet);
            default: return null;
         }
      }

      #if UNITY_EDITOR
      public void UpdateDebug(Dictionary<string,string> debugData){
         string scope = packet.data.Scope;
         for(int i=0;i<packet.data.FlakesLength;++i)
            debugData[scope + "."
               + packet.data.Flakes(i).Value.Label] = packet.data.Origin;
      }
      #endif
   }

   internal class Update : Nugget{
      readonly Dictionary<string,int> lookup;

      public Update(PacketWrapper packet):base(packet){
         //Initialize lookup table
         lookup = new Dictionary<string,int>();
         for(int i=0;i<packet.data.FlakesLength;++i)
            lookup[packet.data.Flakes(i).Value.Label] = i;
      }

      public override bool Load(View view){
         if(view.scope!=packet.data.Scope && view.scope!="")return false;

         if(lookup.ContainsKey(view.label)){
            packet.CopyToView(lookup[view.label],view);
            view.tracked = true;
            return true;
         }
         return false;
      }
   }
   internal class Event : Nugget{
      public readonly string label;
      public readonly bool notification;

      public Event(PacketWrapper packet):base(packet){
         label = packet.data.Flakes(0).Value.Label;
         notification = packet.data.Flakes(0).Value.TriplesLength==0
            && packet.data.Flakes(0).Value.QuadsLength==0
            && packet.data.Flakes(0).Value.FloatsLength==0
            && packet.data.Flakes(0).Value.IntsLength==0
            && packet.data.Flakes(0).Value.CharsLength==0
            && String.IsNullOrEmpty(packet.data.Flakes(0).Value.Text);
      }

      //
      public override bool Load(View view){
         //if(view.scope!=packet.data.Scope && view.scope!="")return false;
         packet.CopyToView(0,view);
         view.ignoreTracking = view.tracked = true;
         return true;
      }
   }

   internal class Translator{
      public static string Origin(){
         return System.Environment.UserName + "@" + System.Environment.MachineName;
      }

      static byte[] BuildPacket(
         string sendScope, Protocol.PacketType type, List<View> views
      ){
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
               for(int j=views[i].triples.Length-1;j>=0;--j){
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
               for(int j=views[i].quads.Length-1;j>=0;--j){
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
            builder,scope,origin,type,flakes
         );
         builder.Finish(packet.Value);
         //Return buffer
         return builder.SizedByteArray();
      }

      public static byte[] BuildUpdate(string sendScope, List<View> views){
         return BuildPacket(sendScope,Protocol.PacketType.Update,views);
      }
      public static byte[] BuildEvent(string sendScope, View view){
         return BuildPacket(
            sendScope,Protocol.PacketType.Event,new List<View>{view}
         );
      }

      public static byte[] BuildNotification(string sendScope, string label){
         FlatBufferBuilder builder = new FlatBufferBuilder(1024);
         StringOffset scope = builder.CreateString(sendScope);
         StringOffset origin = builder.CreateString(Origin());

         Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[1];

         StringOffset notificationLabel = builder.CreateString(label);
         Protocol.Flake.StartFlake(builder);
         Protocol.Flake.AddLabel(builder,notificationLabel);
         offsets[0] = Protocol.Flake.EndFlake(builder);

         var flakes = Protocol.Packet.CreateFlakesVector(builder,offsets);

         //Build packet
         Offset<Protocol.Packet> packet = Protocol.Packet.CreatePacket(
            builder,scope,origin,Protocol.PacketType.Event,flakes
         );
         builder.Finish(packet.Value);
         //Return buffer
         return builder.SizedByteArray();
      }
   }

   public class Canon{
      public static string IndexToLabel(int index, bool raw = false){
         return "M" + Mathf.Max(1,index) + (raw?"-Raw":"");
      }
   }
}
