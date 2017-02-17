// Translation.cs
// Created by Holojam Inc. on 12.02.17
// Collection of classes dealing with raw protocol translation

using System;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

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
    /// Copy a raw flake from this wrapper's packet to a specified target Controller,
    /// given an index.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="controller"></param>
    public void CopyToController(int i, Controller controller) {
      controller.Source = data.Origin;
      CopyToFlake(i, controller.data);
    }

    /// <summary>
    /// Copy a raw flake from this wrapper's packet to a specified Flake in Unity.
    /// </summary>
    /// <param name="i">Index of the Protocol.Flake in this packet to copy.</param>
    /// <param name="flake">Target Flake.</param>
    public void CopyToFlake(int i, Flake flake) {
      flake.triples = new Vector3[data.Flakes(i).Value.TriplesLength];
      for (int j = 0; j < data.Flakes(i).Value.TriplesLength; ++j)
        flake.triples[j] = new Vector3(
           data.Flakes(i).Value.Triples(j).Value.X,
           data.Flakes(i).Value.Triples(j).Value.Y,
           data.Flakes(i).Value.Triples(j).Value.Z
        );
      flake.quads = new Quaternion[data.Flakes(i).Value.QuadsLength];
      for (int j = 0; j < data.Flakes(i).Value.QuadsLength; ++j)
        flake.quads[j] = new Quaternion(
           data.Flakes(i).Value.Quads(j).Value.X,
           data.Flakes(i).Value.Quads(j).Value.Y,
           data.Flakes(i).Value.Quads(j).Value.Z,
           data.Flakes(i).Value.Quads(j).Value.W
        );

      flake.floats = new float[data.Flakes(i).Value.FloatsLength];
      for (int j = 0; j < data.Flakes(i).Value.FloatsLength; ++j)
        flake.floats[j] = data.Flakes(i).Value.Floats(j);

      flake.ints = new int[data.Flakes(i).Value.IntsLength];
      for (int j = 0; j < data.Flakes(i).Value.IntsLength; ++j)
        flake.ints[j] = data.Flakes(i).Value.Ints(j);

      flake.chars = new byte[data.Flakes(i).Value.CharsLength];
      for (int j = 0; j < data.Flakes(i).Value.CharsLength; ++j)
        flake.chars[j] = (byte)data.Flakes(i).Value.Chars(j); //TODO

      flake.text = data.Flakes(i).Value.Text;
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
    /// Load a raw flake from this Update into a specified Controller in Unity using the lookup table.
    /// </summary>
    /// <param name="controller"></param>
    /// <returns>
    /// True if successful, false if the Controller is not present, there is a scope mismatch, or
    /// the flake is null.
    /// </returns>
    public bool Load(Controller controller) {
      // An empty scope is parsed as a "whitelist all"
      if (controller.Scope != packet.data.Scope && controller.Scope != ""
        || controller.data == null)
        return false;

      if (lookup.ContainsKey(controller.Label)) {
        packet.CopyToController(lookup[controller.Label], controller);
        controller.Tracked = true;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Nugget extension for Holojam events (a single flake).
  /// </summary>
  internal sealed class Event : Nugget {

    public readonly string scope, label, source;
    public readonly bool notification;

    /// <summary>
    /// Event constructor: assigns event label and determines whether or not
    /// this is a notification.
    /// </summary>
    /// <param name="packet"></param>
    internal Event(PacketWrapper packet) : base(packet) {
      scope = packet.data.Scope;
      label = packet.data.Flakes(0).Value.Label;
      source = packet.data.Origin;

      notification = packet.data.Flakes(0).Value.TriplesLength == 0
         && packet.data.Flakes(0).Value.QuadsLength == 0
         && packet.data.Flakes(0).Value.FloatsLength == 0
         && packet.data.Flakes(0).Value.IntsLength == 0
         && packet.data.Flakes(0).Value.CharsLength == 0
         && String.IsNullOrEmpty(packet.data.Flakes(0).Value.Text);
    }

    /// <summary>
    /// Loads the event data into a specified Flake in Unity.
    /// </summary>
    /// <param name="flake"></param>
    /// <returns>
    /// True if successful, false if this is a notification or the flake is null.
    /// </returns>
    public bool Load(Flake flake) {
      if (notification || flake == null) return false;
      packet.CopyToFlake(0, flake);
      return true;
    }
  }

  /// <summary>
  /// Translation class (outgoing) from data inside Unity to the Holojam protocol (FlatBuffers).
  /// </summary>
  internal class Translator {

    /// <summary>
    /// Build a Holojam flake with FlatBuffers.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="flake"></param>
    /// <param name="builder"></param>
    /// <param name="offset"></param>
    /// <param name="triples"></param>
    /// <param name="quads"></param>
    /// <param name="floats"></param>
    /// <param name="ints"></param>
    /// <param name="chars"></param>
    /// <param name="text"></param>

    static void BuildFlake(
      string label, Flake flake, ref FlatBufferBuilder builder, ref Offset<Protocol.Flake> offset,
      ref VectorOffset triples, ref VectorOffset quads,
      ref VectorOffset floats, ref VectorOffset ints, ref VectorOffset chars,
      ref StringOffset text
    ) {
      StringOffset labelOffset = builder.CreateString(label);

      // Create the vectors
      if (flake.triples != null) {
        Protocol.Flake.StartTriplesVector(builder, flake.triples.Length);
        for (int j = flake.triples.Length - 1; j >= 0; --j) {
          Protocol.Vector3.CreateVector3(builder,
             flake.triples[j].x,
             flake.triples[j].y,
             flake.triples[j].z
          );
        }
        triples = builder.EndVector();
      }
      if (flake.quads != null) {
        Protocol.Flake.StartQuadsVector(builder, flake.quads.Length);
        for (int j = flake.quads.Length - 1; j >= 0; --j) {
          Protocol.Vector4.CreateVector4(builder,
             flake.quads[j].x,
             flake.quads[j].y,
             flake.quads[j].z,
             flake.quads[j].w
          );
        }
        quads = builder.EndVector();
      }
      if (flake.floats != null)
        floats = Protocol.Flake.CreateFloatsVector(builder, flake.floats);
      if (flake.ints != null)
        ints = Protocol.Flake.CreateIntsVector(builder, flake.ints);
      if (flake.chars != null) {
        Protocol.Flake.StartCharsVector(builder, flake.chars.Length);
        for (int j = flake.chars.Length - 1; j > 0; --j)
          builder.AddByte(flake.chars[j]); //TODO
        chars = builder.EndVector();
      }
      if (flake.text != null)
        text = builder.CreateString(flake.text);

      // Put it all together
      Protocol.Flake.StartFlake(builder);
      Protocol.Flake.AddLabel(builder, labelOffset);

      if (flake.triples != null)
        Protocol.Flake.AddTriples(builder, triples);
      if (flake.quads != null)
        Protocol.Flake.AddQuads(builder, quads);
      if (flake.floats != null)
        Protocol.Flake.AddFloats(builder, floats);
      if (flake.ints != null)
        Protocol.Flake.AddInts(builder, ints);
      if (flake.chars != null)
        Protocol.Flake.AddChars(builder, chars);
      if (flake.text != null)
        Protocol.Flake.AddText(builder, text);

      offset = Protocol.Flake.EndFlake(builder);
    }

    /// <summary>
    /// Build a Holojam update packet buffer with FlatBuffers.
    /// </summary>
    /// <param name="controllers"></param>
    /// <returns>A raw buffer built using the Holojam FlatBuffers protocol schema.</returns>
    public static byte[] BuildUpdate(List<Controller> controllers) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);

      StringOffset scope = builder.CreateString(Client.SEND_SCOPE);
      StringOffset origin = builder.CreateString(Canon.Origin());

      Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[controllers.Count];

      VectorOffset triples, quads;
      VectorOffset floats, ints, chars;
      StringOffset text = default(StringOffset);
      triples = quads = floats = ints = chars = default(VectorOffset);

      for (int i = 0; i < controllers.Count; ++i)
        BuildFlake(controllers[i].Label, controllers[i].data, ref builder, ref offsets[i],
          ref triples, ref quads, ref floats, ref ints, ref chars, ref text
        );
      var flakes = Protocol.Packet.CreateFlakesVector(builder, offsets);

      // Build packet
      Offset<Protocol.Packet> packet = Protocol.Packet.CreatePacket(
         builder, scope, origin, Protocol.PacketType.Update, flakes
      );
      builder.Finish(packet.Value);
      // Return buffer
      return builder.SizedByteArray();
    }

    /// <summary>
    /// Build a Holojam event packet buffer with FlatBuffers.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="flake"></param>
    /// <returns>A raw buffer built using the Holojam FlatBuffers protocol schema.</returns>
    public static byte[] BuildEvent(string label, Flake flake) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      StringOffset scope = builder.CreateString(Client.SEND_SCOPE);
      StringOffset origin = builder.CreateString(Canon.Origin());

      Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[1];

      VectorOffset triples, quads;
      VectorOffset floats, ints, chars;
      StringOffset text = default(StringOffset);
      triples = quads = floats = ints = chars = default(VectorOffset);

      BuildFlake(label, flake, ref builder, ref offsets[0],
        ref triples, ref quads, ref floats, ref ints, ref chars, ref text
      );

      var flakes = Protocol.Packet.CreateFlakesVector(builder, offsets);

      // Build packet
      Offset<Protocol.Packet> packet = Protocol.Packet.CreatePacket(
         builder, scope, origin, Protocol.PacketType.Event, flakes
      );
      builder.Finish(packet.Value);
      // Return buffer
      return builder.SizedByteArray();
    }

    /// <summary>
    /// Build a Holojam notification packet buffer with FlatBuffers.
    /// </summary>
    /// <param name="label"></param>
    /// <returns>A raw buffer built using the Holojam FlatBuffers protocol schema.</returns>
    public static byte[] BuildNotification(string label) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      StringOffset scope = builder.CreateString(Client.SEND_SCOPE);
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
}
