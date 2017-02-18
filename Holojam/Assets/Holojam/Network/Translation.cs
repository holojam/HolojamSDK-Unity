// Translation.cs
// Created by Holojam Inc. on 12.02.17
// Collection of classes dealing with raw protocol translation

using System;
using System.Collections.Generic;
using UnityEngine;
using FlatBuffers;

namespace Holojam.Network {

  /// <summary>
  /// Wrapper around a Holojam nugget struct in order to prevent unwanted copying--provides
  /// low-level functionality.
  /// </summary>
  internal class NuggetWrapper {
    public readonly Protocol.Nugget data;

    /// <summary>
    /// Generate a nugget from a raw buffer.
    /// </summary>
    /// <param name="buffer"></param>
    public NuggetWrapper(ref byte[] buffer) {
      data = Protocol.Nugget.GetRootAsNugget(new ByteBuffer(buffer));
    }

    /// <summary>
    /// Copy a raw flake from this wrapper's nugget to a specified target Controller,
    /// given an index.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="controller"></param>
    public void CopyToController(int i, Controller controller) {
      controller.Source = data.Origin;
      CopyToFlake(i, controller.data);
    }

    /// <summary>
    /// Copy a raw flake from this wrapper's nugget to a specified Flake in Unity.
    /// </summary>
    /// <param name="i">Index of the Protocol.Flake in this nugget to copy.</param>
    /// <param name="flake">Target Flake.</param>
    public void CopyToFlake(int i, Flake flake) {
      flake.vector3s = new Vector3[data.Flakes(i).Value.Vector3sLength];
      for (int j = 0; j < data.Flakes(i).Value.Vector3sLength; ++j)
        flake.vector3s[j] = new Vector3(
           data.Flakes(i).Value.Vector3s(j).Value.X,
           data.Flakes(i).Value.Vector3s(j).Value.Y,
           data.Flakes(i).Value.Vector3s(j).Value.Z
        );
      flake.vector4s = new Quaternion[data.Flakes(i).Value.Vector4sLength];
      for (int j = 0; j < data.Flakes(i).Value.Vector4sLength; ++j)
        flake.vector4s[j] = new Quaternion(
           data.Flakes(i).Value.Vector4s(j).Value.X,
           data.Flakes(i).Value.Vector4s(j).Value.Y,
           data.Flakes(i).Value.Vector4s(j).Value.Z,
           data.Flakes(i).Value.Vector4s(j).Value.W
        );

      flake.floats = new float[data.Flakes(i).Value.FloatsLength];
      for (int j = 0; j < data.Flakes(i).Value.FloatsLength; ++j)
        flake.floats[j] = data.Flakes(i).Value.Floats(j);

      flake.ints = new int[data.Flakes(i).Value.IntsLength];
      for (int j = 0; j < data.Flakes(i).Value.IntsLength; ++j)
        flake.ints[j] = data.Flakes(i).Value.Ints(j);

      flake.bytes = new byte[data.Flakes(i).Value.BytesLength];
      for (int j = 0; j < data.Flakes(i).Value.BytesLength; ++j)
        flake.bytes[j] = (byte)data.Flakes(i).Value.Bytes(j);

      flake.text = data.Flakes(i).Value.Text;
    }
  }

  /// <summary>
  /// Abstract base class for translation functionality around Holojam updates and
  /// events/notifications (incoming). Contains a NuggetWrapper.
  /// </summary>
  abstract internal class Nugget {
    protected NuggetWrapper nugget;

    /// <summary>
    /// Protected constructor for factory method.
    /// </summary>
    /// <param name="nugget"></param>
    protected Nugget(NuggetWrapper nugget) {
      this.nugget = nugget;
    }

    /// <summary>
    /// Factory method for creating Nuggets of a specific derived type given a raw buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns>The derived Nugget.</returns>
    public static Nugget Create(ref byte[] buffer) {
      NuggetWrapper nugget = new NuggetWrapper(ref buffer);
      switch (nugget.data.Type) {
        case Protocol.NuggetType.UPDATE:
          return new Update(nugget);
        case Protocol.NuggetType.EVENT:
          return new Event(nugget);
        default: return null;
      }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Debug function for custom Client inspector.
    /// </summary>
    /// <param name="debugData"></param>
    public void UpdateDebug(Dictionary<string, string> debugData) {
      string scope = nugget.data.Scope;
      for (int i = 0; i < nugget.data.FlakesLength; ++i)
        debugData[scope + "."
           + nugget.data.Flakes(i).Value.Label] = nugget.data.Origin;
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
    /// <param name="nugget"></param>
    internal Update(NuggetWrapper nugget) : base(nugget) {
      // Initialize lookup table
      lookup = new Dictionary<string, int>();
      for (int i = 0; i < nugget.data.FlakesLength; ++i)
        lookup[nugget.data.Flakes(i).Value.Label] = i;
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
      if (controller.Scope != nugget.data.Scope && controller.Scope != ""
        || controller.data == null)
        return false;

      if (lookup.ContainsKey(controller.Label)) {
        nugget.CopyToController(lookup[controller.Label], controller);
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
    /// <param name="nugget"></param>
    internal Event(NuggetWrapper nugget) : base(nugget) {
      scope = nugget.data.Scope;
      label = nugget.data.Flakes(0).Value.Label;
      source = nugget.data.Origin;

      notification = nugget.data.Flakes(0).Value.Vector3sLength == 0
         && nugget.data.Flakes(0).Value.Vector4sLength == 0
         && nugget.data.Flakes(0).Value.FloatsLength == 0
         && nugget.data.Flakes(0).Value.IntsLength == 0
         && nugget.data.Flakes(0).Value.BytesLength == 0
         && String.IsNullOrEmpty(nugget.data.Flakes(0).Value.Text);
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
      nugget.CopyToFlake(0, flake);
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
    /// <param name="vector3s"></param>
    /// <param name="vector4s"></param>
    /// <param name="floats"></param>
    /// <param name="ints"></param>
    /// <param name="bytes"></param>
    /// <param name="text"></param>

    static void BuildFlake(
      string label, Flake flake, ref FlatBufferBuilder builder, ref Offset<Protocol.Flake> offset,
      ref VectorOffset vector3s, ref VectorOffset vector4s,
      ref VectorOffset floats, ref VectorOffset ints, ref VectorOffset bytes,
      ref StringOffset text
    ) {
      StringOffset labelOffset = builder.CreateString(label);

      // Create the vectors
      if (flake.vector3s != null) {
        Protocol.Flake.StartVector3sVector(builder, flake.vector3s.Length);
        for (int j = flake.vector3s.Length - 1; j >= 0; --j) {
          Protocol.Vector3.CreateVector3(builder,
             flake.vector3s[j].x,
             flake.vector3s[j].y,
             flake.vector3s[j].z
          );
        }
        vector3s = builder.EndVector();
      }
      if (flake.vector4s != null) {
        Protocol.Flake.StartVector4sVector(builder, flake.vector4s.Length);
        for (int j = flake.vector4s.Length - 1; j >= 0; --j) {
          Protocol.Vector4.CreateVector4(builder,
             flake.vector4s[j].x,
             flake.vector4s[j].y,
             flake.vector4s[j].z,
             flake.vector4s[j].w
          );
        }
        vector4s = builder.EndVector();
      }
      if (flake.floats != null)
        floats = Protocol.Flake.CreateFloatsVector(builder, flake.floats);
      if (flake.ints != null)
        ints = Protocol.Flake.CreateIntsVector(builder, flake.ints);
      if (flake.bytes != null) {
        Protocol.Flake.StartBytesVector(builder, flake.bytes.Length);
        for (int j = flake.bytes.Length - 1; j > 0; --j)
          builder.AddByte(flake.bytes[j]);
        bytes = builder.EndVector();
      }
      if (flake.text != null)
        text = builder.CreateString(flake.text);

      // Put it all together
      Protocol.Flake.StartFlake(builder);
      Protocol.Flake.AddLabel(builder, labelOffset);

      if (flake.vector3s != null)
        Protocol.Flake.AddVector3s(builder, vector3s);
      if (flake.vector4s != null)
        Protocol.Flake.AddVector4s(builder, vector4s);
      if (flake.floats != null)
        Protocol.Flake.AddFloats(builder, floats);
      if (flake.ints != null)
        Protocol.Flake.AddInts(builder, ints);
      if (flake.bytes != null)
        Protocol.Flake.AddBytes(builder, bytes);
      if (flake.text != null)
        Protocol.Flake.AddText(builder, text);

      offset = Protocol.Flake.EndFlake(builder);
    }

    /// <summary>
    /// Build a Holojam update nugget buffer with FlatBuffers.
    /// </summary>
    /// <param name="controllers"></param>
    /// <returns>A raw buffer built using the Holojam FlatBuffers protocol schema.</returns>
    public static byte[] BuildUpdate(List<Controller> controllers) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);

      StringOffset scope = builder.CreateString(Client.SEND_SCOPE);
      StringOffset origin = builder.CreateString(Canon.Origin());

      Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[controllers.Count];

      VectorOffset vector3s, vector4s;
      VectorOffset floats, ints, bytes;
      StringOffset text = default(StringOffset);
      vector3s = vector4s = floats = ints = bytes = default(VectorOffset);

      for (int i = 0; i < controllers.Count; ++i)
        BuildFlake(controllers[i].Label, controllers[i].data, ref builder, ref offsets[i],
          ref vector3s, ref vector4s, ref floats, ref ints, ref bytes, ref text
        );
      var flakes = Protocol.Nugget.CreateFlakesVector(builder, offsets);

      // Build nugget
      Offset<Protocol.Nugget> nugget = Protocol.Nugget.CreateNugget(
         builder, scope, origin, Protocol.NuggetType.UPDATE, flakes
      );
      builder.Finish(nugget.Value);
      // Return buffer
      return builder.SizedByteArray();
    }

    /// <summary>
    /// Build a Holojam event nugget buffer with FlatBuffers.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="flake"></param>
    /// <returns>A raw buffer built using the Holojam FlatBuffers protocol schema.</returns>
    public static byte[] BuildEvent(string label, Flake flake) {
      FlatBufferBuilder builder = new FlatBufferBuilder(1024);
      StringOffset scope = builder.CreateString(Client.SEND_SCOPE);
      StringOffset origin = builder.CreateString(Canon.Origin());

      Offset<Protocol.Flake>[] offsets = new Offset<Protocol.Flake>[1];

      VectorOffset vector3s, vector4s;
      VectorOffset floats, ints, bytes;
      StringOffset text = default(StringOffset);
      vector3s = vector4s = floats = ints = bytes = default(VectorOffset);

      BuildFlake(label, flake, ref builder, ref offsets[0],
        ref vector3s, ref vector4s, ref floats, ref ints, ref bytes, ref text
      );

      var flakes = Protocol.Nugget.CreateFlakesVector(builder, offsets);

      // Build nugget
      Offset<Protocol.Nugget> nugget = Protocol.Nugget.CreateNugget(
         builder, scope, origin, Protocol.NuggetType.EVENT, flakes
      );
      builder.Finish(nugget.Value);
      // Return buffer
      return builder.SizedByteArray();
    }

    /// <summary>
    /// Build a Holojam notification nugget buffer with FlatBuffers.
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

      var flakes = Protocol.Nugget.CreateFlakesVector(builder, offsets);

      // Build nugget
      Offset<Protocol.Nugget> nugget = Protocol.Nugget.CreateNugget(
         builder, scope, origin, Protocol.NuggetType.EVENT, flakes
      );
      builder.Finish(nugget.Value);
      // Return buffer
      return builder.SizedByteArray();
    }
  }
}
