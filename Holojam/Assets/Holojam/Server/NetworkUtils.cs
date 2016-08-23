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

namespace Holojam.Network {
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

	[Obsolete("MasterStream/MasterServer is deprecated. Please use HolojamNetwork.")]
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
				o.extra_data=blob;
			}

			return o;
		}
	}

	public class Motive{
		
		public enum Tag {
			HEADSET1, HEADSET2, HEADSET3, HEADSET4,
			WAND1, WAND2, WAND3, WAND4,
			BOX1, BOX2, SPHERE1,
			LEFTHAND1, RIGHTHAND1, LEFTFOOT1, RIGHTFOOT1,
			LEFTHAND2, RIGHTHAND2, LEFTFOOT2, RIGHTFOOT2,
			LEFTHAND3, RIGHTHAND3, LEFTFOOT3, RIGHTFOOT3,
			LEFTHAND4, RIGHTHAND4, LEFTFOOT4, RIGHTFOOT4,
			LAPTOP, TABLE,
			VIVE,VIVECONTROLLERLEFT,VIVECONTROLLERRIGHT
		}
		
		private static readonly Dictionary<Tag, string> tagNames = new Dictionary<Tag, string>() {
			{ Tag.HEADSET1, "VR1" },
			{ Tag.HEADSET2, "VR2" },
			{ Tag.HEADSET3, "VR3" },
			{ Tag.HEADSET4, "VR4" },
			{ Tag.WAND1, "VR1_wand" },
			{ Tag.WAND2, "VR2_wand" },
			{ Tag.WAND3, "VR3_wand" },
			{ Tag.WAND4, "VR4_wand" },
			{ Tag.BOX1, "VR1_box" },
			{ Tag.LEFTHAND1, "VR1_lefthand"},
			{ Tag.RIGHTHAND1, "VR1_righthand"},
			{ Tag.LEFTFOOT1, "VR1_leftankle"},
			{ Tag.RIGHTFOOT1, "VR1_rightankle"},
			{ Tag.LEFTHAND2, "VR2_lefthand"},
			{ Tag.RIGHTHAND2, "VR2_righthand"},
			{ Tag.LEFTFOOT2, "VR2_leftankle"},
			{ Tag.RIGHTFOOT2, "VR2_rightankle"},
			{ Tag.LEFTHAND3, "VR3_lefthand"},
			{ Tag.RIGHTHAND3, "VR3_righthand"},
			{ Tag.LEFTFOOT3, "VR3_leftankle"},
			{ Tag.RIGHTFOOT3, "VR3_rightankle"},
			{ Tag.LEFTHAND4, "VR4_lefthand"},
			{ Tag.RIGHTHAND4, "VR4_righthand"},
			{ Tag.LEFTFOOT4, "VR4_leftankle"},
			{ Tag.RIGHTFOOT4, "VR4_rightankle"},
			{ Tag.LAPTOP, "VR1_laptop"},
			{ Tag.TABLE, "VR1_table"},
			{ Tag.VIVE, "vive"},
			{ Tag.VIVECONTROLLERLEFT, "vive_controller_left"},
			{ Tag.VIVECONTROLLERRIGHT, "vive_controller_right"}
		};
		
		public static string GetName(Tag tag) {
			if (tagNames.ContainsKey(tag)) {
				return tagNames[tag];
			} else {
				throw new System.ArgumentException("Illegal tag.");
			}
		}
		public static int tagCount{get{return Enum.GetNames(typeof(Tag)).Length;}}
	}
}