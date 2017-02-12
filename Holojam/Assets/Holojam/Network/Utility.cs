// Utility.cs
// Created by Holojam Inc. on 13.11.16

using UnityEngine;
using FlatBuffers;
using System;
using System.Collections.Generic;

namespace Holojam.Network {

  /// <summary>
  /// Wrapper around a Holojam packet struct in order to prevent unwanted copying--provides
  /// low-level functionality.
  /// </summary>
  internal class PacketWrapper {
    public readonly Protocol.Packet data;

    /// <summary>
    /// Generate a packet from a raw buffer.
    /// </summary>
    /// <param name="buffer"></param>
    public PacketWrapper(ref byte[] buffer) {
      data = Protocol.Packet.GetRootAsPacket(new ByteBuffer(buffer));
    }


    /// <summary>
    /// Copy a flake from this wrapper's packet to a specified target View, given an index.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="view"></param>
    public void CopyToView(int i, View view) {
      view.scope = data.Scope;

      view.label = data.Flakes(i).Value.Label;
      view.triples = new Vector3[data.Flakes(i).Value.TriplesLength];
      for (int j = 0; j < data.Flakes(i).Value.TriplesLength; ++j)
        view.triples[j] = new Vector3(
           data.Flakes(i).Value.Triples(j).Value.X,
           data.Flakes(i).Value.Triples(j).Value.Y,
           data.Flakes(i).Value.Triples(j).Value.Z
        );
      view.quads = new Quaternion[data.Flakes(i).Value.QuadsLength];
      for (int j = 0; j < data.Flakes(i).Value.QuadsLength; ++j)
        view.quads[j] = new Quaternion(
           data.Flakes(i).Value.Quads(j).Value.X,
           data.Flakes(i).Value.Quads(j).Value.Y,
           data.Flakes(i).Value.Quads(j).Value.Z,
           data.Flakes(i).Value.Quads(j).Value.W
        );

      view.floats = new float[data.Flakes(i).Value.FloatsLength];
      for (int j = 0; j < data.Flakes(i).Value.FloatsLength; ++j)
        view.floats[j] = data.Flakes(i).Value.Floats(j);

      view.ints = new int[data.Flakes(i).Value.IntsLength];
      for (int j = 0; j < data.Flakes(i).Value.IntsLength; ++j)
        view.ints[j] = data.Flakes(i).Value.Ints(j);

      view.chars = new byte[data.Flakes(i).Value.CharsLength];
      for (int j = 0; j < data.Flakes(i).Value.CharsLength; ++j)
        view.chars[j] = (byte)data.Flakes(i).Value.Chars(j); //TODO

      view.text = data.Flakes(i).Value.Text;
    }
  }

  /// <summary>
  /// Abstract base class for translation functionality around Holojam updates and
  /// events/notifications (incoming). Contains a PacketWrapper.
  /// </summary>
  abstract internal class Nugget {
    protected PacketWrapper packet;

    /// <summary>
    /// Protected constructor for factory method.
    /// </summary>
    /// <param name="packet"></param>
    protected Nugget(PacketWrapper packet) {
      this.packet = packet;
    }

    /// <summary>
    /// Load a raw flake from this Nugget into a View in Unity.
    /// </summary>
    /// <param name="view"></param>
    /// <returns>True if successful.</returns>
    public abstract bool Load(View view);

    /// <summary>
    /// Factory method for creating Nuggets of a specific derived type given a raw buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns>The derived Nugget.</returns>
    public static Nugget Create(ref byte[] buffer) {
      PacketWrapper packet = new PacketWrapper(ref buffer);
      switch (packet.data.Type) {
        case Protocol.PacketType.Update:
          return new Update(packet);
        case Protocol.PacketType.Event:
          return new Event(packet);
        default: return null;
      }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Debug function for custom Client inspector.
    /// </summary>
    /// <param name="debugData"></param>
    public void UpdateDebug(Dictionary<string, string> debugData) {
      string scope = packet.data.Scope;
      for (int i = 0; i < packet.data.FlakesLength; ++i)
        debugData[scope + "."
           + packet.data.Flakes(i).Value.Label] = packet.data.Origin;
    }
    #endif
  }

  /// <summary>
  /// Nugget extension for Holojam updates (a collection of flakes).
  /// </summary>
  internal sealed class Update : Nugget {
    readonly Dictionary<string, int> lookup;

    /// <summary>
    /// Update constructor: initializes and populates lookup table.
    /// </summary>
    /// <param name="packet"></param>
    internal Update(PacketWrapper packet) : base(packet) {
      // Initialize lookup table
      lookup = new Dictionary<string, int>();
      for (int i = 0; i < packet.data.FlakesLength; ++i)
        lookup[packet.data.Flakes(i).Value.Label] = i;
    }

    /// <summary>
    /// Load a raw flake from this Update into a specified View in Unity using the lookup table.
    /// </summary>
    /// <param name="view"></param>
    /// <returns>
    /// True if successful, false if the view was not present or there was a scope mismatch.
    /// </returns>
    public override bool Load(View view) {
      if (view.scope != packet.data.Scope && view.scope != "") return false;

      if (lookup.ContainsKey(view.label)) {
        packet.CopyToView(lookup[view.label], view);
        view.tracked = true;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Nugget extension for Holojam events (a single flake).
  /// </summary>
  internal sealed class Event : Nugget {

    public readonly string label;
    public readonly bool notification;

    /// <summary>
    /// Event constructor: assigns event label and determines whether or not
    /// this is a notification.
    /// </summary>
    /// <param name="packet"></param>
    internal Event(PacketWrapper packet) : base(packet) {
      label = packet.data.Flakes(0).Value.Label;
      notification = packet.data.Flakes(0).Value.TriplesLength == 0
         && packet.data.Flakes(0).Value.QuadsLength == 0
         && packet.data.Flakes(0).Value.FloatsLength == 0
         && packet.data.Flakes(0).Value.IntsLength == 0
         && packet.data.Flakes(0).Value.CharsLength == 0
         && String.IsNullOrEmpty(packet.data.Flakes(0).Value.Text);
    }

    /// <summary>
    /// Loads the event data into a specified View in Unity.
    /// </summary>
    /// <param name="view"></param>
    /// <returns>True if successful.</returns>
    public override bool Load(View view) {
      //if(view.scope!=packet.data.Scope && view.scope!="")return false; //TODO
      packet.CopyToView(0, view);
      view.ignoreTracking = view.tracked = true;
      return true;
    }
  }

  /// <summary>
  /// Translation class (outgoing) from data inside Unity to the Holojam protocol (FlatBuffers).
  /// </summary>
  internal class Translator {

    /// <summary>
    /// Private function for building generic Holojam packets with FlatBuffers.
    /// </summary>
    /// <param name="sendScope"></param>
    /// <param name="type"></param>
    /// <param name="views"></param>
    /// <returns>A raw buffer built using the Holojam FlatBuffers protocol schema.</returns>
    static byte[] BuildPacket(string sendScope, Protocol.PacketType type, List<View> views) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);

      StringOffset scope = builder.CreateString(sendScope);
      StringOffset origin = builder.CreateString(Canon.Origin());

      Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[views.Count];

      VectorOffset triples, quads;
      VectorOffset floats, ints, chars;
      StringOffset text = default(StringOffset);
      triples = quads = floats = ints = chars = default(VectorOffset);

      for (int i = 0; i < views.Count; ++i) {
        StringOffset label = builder.CreateString(views[i].label);

        // Create the vectors
        if (views[i].triples != null) {
          Protocol.Flake.StartTriplesVector(builder, views[i].triples.Length);
          for (int j = views[i].triples.Length - 1; j >= 0; --j) {
            Protocol.Vector3.CreateVector3(builder,
               views[i].triples[j].x,
               views[i].triples[j].y,
               views[i].triples[j].z
            );
          }
          triples = builder.EndVector();
        }
        if (views[i].quads != null) {
          Protocol.Flake.StartQuadsVector(builder, views[i].quads.Length);
          for (int j = views[i].quads.Length - 1; j >= 0; --j) {
            Protocol.Vector4.CreateVector4(builder,
               views[i].quads[j].x,
               views[i].quads[j].y,
               views[i].quads[j].z,
               views[i].quads[j].w
            );
          }
          quads = builder.EndVector();
        }
        if (views[i].floats != null)
          floats = Protocol.Flake.CreateFloatsVector(builder, views[i].floats);
        if (views[i].ints != null)
          ints = Protocol.Flake.CreateIntsVector(builder, views[i].ints);
        if (views[i].chars != null) {
          Protocol.Flake.StartCharsVector(builder, views[i].chars.Length);
          for (int j = views[i].chars.Length - 1; j > 0; --j)
            builder.AddByte(views[i].chars[j]); //TODO
          chars = builder.EndVector();
        }
        if (views[i].text != null)
          text = builder.CreateString(views[i].text);

        // Put it all together
        Protocol.Flake.StartFlake(builder);
        Protocol.Flake.AddLabel(builder, label);

        if (views[i].triples != null)
          Protocol.Flake.AddTriples(builder, triples);
        if (views[i].quads != null)
          Protocol.Flake.AddQuads(builder, quads);
        if (views[i].floats != null)
          Protocol.Flake.AddFloats(builder, floats);
        if (views[i].ints != null)
          Protocol.Flake.AddInts(builder, ints);
        if (views[i].chars != null)
          Protocol.Flake.AddChars(builder, chars);
        if (views[i].text != null)
          Protocol.Flake.AddText(builder, text);

        offsets[i] = Protocol.Flake.EndFlake(builder);
      }
      var flakes = Protocol.Packet.CreateFlakesVector(builder, offsets);

      // Build packet
      Offset<Protocol.Packet> packet = Protocol.Packet.CreatePacket(
         builder, scope, origin, type, flakes
      );
      builder.Finish(packet.Value);
      // Return buffer
      return builder.SizedByteArray();
    }

    /// <summary>
    /// Build Holojam update packet buffer with FlatBuffers.
    /// </summary>
    /// <param name="sendScope"></param>
    /// <param name="views"></param>
    /// <returns>The buffer specified.</returns>
    public static byte[] BuildUpdate(string sendScope, List<View> views) {
      return BuildPacket(sendScope, Protocol.PacketType.Update, views);
    }

    /// <summary>
    /// Build Holojam event packet buffer with FlatBuffers.
    /// </summary>
    /// <param name="sendScope"></param>
    /// <param name="view"></param>
    /// <returns>The buffer specified.</returns>
    public static byte[] BuildEvent(string sendScope, View view) {
      return BuildPacket(
         sendScope, Protocol.PacketType.Event, new List<View> { view }
      );
    }

    /// <summary>
    /// Build Holojam notification packet buffer with FlatBuffers.
    /// </summary>
    /// <param name="sendScope"></param>
    /// <param name="label"></param>
    /// <returns>The buffer specified.</returns>
    public static byte[] BuildNotification(string sendScope, string label) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      StringOffset scope = builder.CreateString(sendScope);
      StringOffset origin = builder.CreateString(Canon.Origin());

      Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[1];

      StringOffset notificationLabel = builder.CreateString(label);
      Protocol.Flake.StartFlake(builder);
      Protocol.Flake.AddLabel(builder, notificationLabel);
      offsets[0] = Protocol.Flake.EndFlake(builder);

      var flakes = Protocol.Packet.CreateFlakesVector(builder, offsets);

      // Build packet
      Offset<Protocol.Packet> packet = Protocol.Packet.CreatePacket(
         builder, scope, origin, Protocol.PacketType.Event, flakes
      );
      builder.Finish(packet.Value);
      // Return buffer
      return builder.SizedByteArray();
    }
  }

  /// <summary>
  /// Class for canonical/reserved Holojam network identifiers.
  /// </summary>
  public class Canon {

    /// <summary>
    /// Returns a string representing the Holojam machine identifier.
    /// </summary>
    /// <returns>The string specified.</returns>
    public static string Origin() {
      return System.Environment.UserName + "@" + System.Environment.MachineName;
    }

    /// <summary>
    /// Returns a string label given a build/actor index (Holojam standard).
    /// </summary>
    /// <param name="index"></param>
    /// <param name="raw"></param>
    /// <returns>The string specified.</returns>
    public static string IndexToLabel(int index, bool raw = false) {
      return "M" + Mathf.Max(1, index) + (raw ? "-Raw" : "");
    }
  }
}
