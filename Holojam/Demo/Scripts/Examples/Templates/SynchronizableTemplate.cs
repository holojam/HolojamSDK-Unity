// SynchronizableTemplate.cs
// Created by Holojam Inc. on 01.04.17

using UnityEngine;

// This is an example extension of the Synchronizable class, with the most common
// overrides and fields shown.

// Synchronizables are asymmetric--they could be hosting data (e.g. on a Master Client)
// or listening to data.

public class SynchronizableTemplate : Holojam.Tools.Synchronizable {

  // Try switching between Master Client and regular client on the BuildManager to
  // experiment with auto hosting.

  public Color color = Color.white; // Synchronize a color as an example

  // Expose auto hosting to the inspector
  [SerializeField] bool autoHost;

  public override string Label {
    get { return "label"; } // The unique identifier for this piece of data
  }

  public override bool Host {
    get { return false; }
  }

  public override bool AutoHost {
    get { return autoHost; }
  }

  // This is a proxy--you need to make one if you want to access this Controller's data
  // from outside the class
  public int MyInt {
    get { return data.ints[0]; }
    set { data.ints[0] = value; }
  }

  // Here's another proxy, but this time demonstrates a mapping from a core Flake
  // data type (float) to something more abstract (a Unity color)
  public Color MyColor {
    get {
      return new Color(
        data.floats[0],
        data.floats[1],
        data.floats[2],
        1
      );
    }
    set {
      data.floats[0] = value.r;
      data.floats[1] = value.g;
      data.floats[2] = value.b;
    }
  }

  // You need to reset (allocate) this Controller's data before you can use it
  // Awake() calls ResetData() by default
  public override void ResetData() {
    // Allocate one int and three floats
    data = new Holojam.Network.Flake(
      0, 0, 3, 1, 0, false
    );
  }

  // Core method in Synchronizable
  protected override void Sync() {
    // If this synchronizable is hosting data on the Label
    if (Sending) {
      // Set the outgoing data
      MyInt = 8;
      MyColor = color;

      Debug.Log("SynchronizableTemplate: sending data on " + Brand);
    }

    // If this synchronizable is listening for data on the Label
    else {
      if (Tracked) { // Do something with the incoming data if it's tracked
        Debug.Log(
          "SynchronizableTemplate: data is coming in on " + Brand
          + " from " + Source
          + " (MyInt = " + MyInt + ")",
          this
        );

        color = MyColor; // Set the color in the inspector
      }

      // Not tracked--either nobody is hosting on the Label, or this client
      // is not connected to the network
      else {
        Debug.Log(
          "SynchronizableTemplate: no data coming in on " + Brand,
          this
        );
      }
    }
  }
}
