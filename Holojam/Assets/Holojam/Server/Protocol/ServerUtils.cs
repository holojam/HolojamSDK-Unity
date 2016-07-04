using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using ProtoBuf;
using update_protocol_v3;
using System.Threading;

namespace Holojam.Server {
     class PacketBuffer {

          public const int PACKET_SIZE = 65507; // ~65KB buffer sizes


          public byte[] bytes;
          public MemoryStream stream;
          public long frame;

          public PacketBuffer(int packetSize) {
               bytes = new byte[packetSize];
               stream = new MemoryStream(bytes);
               frame = 0;
          }

          public void copyFrom(PacketBuffer other) {
               this.bytes = other.bytes;
               this.stream = other.stream;
               this.frame = other.frame;
          }
     }

     class LiveObjectStorage {
          public static readonly Vector3 DEFAULT_VECTOR_POSITION = Vector3.zero;
          public static readonly Quaternion DEFAULT_QUATERNION_ROTATION = Quaternion.identity;

          public string label;
          public Vector3 position = DEFAULT_VECTOR_POSITION;
          public Quaternion rotation = DEFAULT_QUATERNION_ROTATION;
          public int bits = 0;
          public string blob = "";


          public LiveObjectStorage(string label) {
               this.label = label;
          }

          public LiveObject ToLiveObject() {
               LiveObject o = new LiveObject();
               o.label = this.label;

               o.x = position.x;
               o.y = position.y;
               o.z = position.z;

               o.qx = rotation.x;
               o.qy = rotation.y;
               o.qz = rotation.z;
               o.qw = rotation.w;

               o.button_bits = bits;
               
               if (!string.IsNullOrEmpty(blob)) {
                    ExtraData data = new ExtraData();
                    data.label = "json";
                    data.string_val = blob;

                    o.extra_data.Add(data);
               }

               return o;
          }
     }

     class Motive {

          private static readonly Dictionary<LiveObjectTag, string> tagNames = new Dictionary<LiveObjectTag, string>() {
               { LiveObjectTag.HEADSET1, "VR1" },
               { LiveObjectTag.HEADSET2, "VR2" },
               { LiveObjectTag.HEADSET3, "VR3" },
               { LiveObjectTag.HEADSET4, "VR4" },
               { LiveObjectTag.WAND1, "VR1_wand" },
               { LiveObjectTag.WAND2, "VR2_wand" },
               { LiveObjectTag.WAND3, "VR3_wand" },
               { LiveObjectTag.WAND4, "VR4_wand" },
               { LiveObjectTag.BOX1, "VR1_box" },
               { LiveObjectTag.LEFTHAND1, "VR1_lefthand"},
               { LiveObjectTag.RIGHTHAND1, "VR1_righthand"},
               { LiveObjectTag.LEFTFOOT1, "VR1_leftankle"},
               { LiveObjectTag.RIGHTFOOT1, "VR1_rightankle"},
               { LiveObjectTag.LEFTHAND2, "VR2_lefthand"},
               { LiveObjectTag.RIGHTHAND2, "VR2_righthand"},
               { LiveObjectTag.LEFTFOOT2, "VR2_leftankle"},
               { LiveObjectTag.RIGHTFOOT2, "VR2_rightankle"},
               { LiveObjectTag.LEFTHAND3, "VR3_lefthand"},
               { LiveObjectTag.RIGHTHAND3, "VR3_righthand"},
               { LiveObjectTag.LEFTFOOT3, "VR3_leftankle"},
               { LiveObjectTag.RIGHTFOOT3, "VR3_rightankle"},
               { LiveObjectTag.LAPTOP, "VR1_laptop"},
               { LiveObjectTag.TABLE, "VR1_table"}
          };

          public static string GetName(LiveObjectTag tag) {
               if (tagNames.ContainsKey(tag)) {
                    return tagNames[tag];
               } else {
                    throw new System.ArgumentException("Illegal tag.");
               }
          }
     }

}

