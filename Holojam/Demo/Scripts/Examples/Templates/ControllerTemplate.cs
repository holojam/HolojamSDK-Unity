// ControllerTemplate.cs
// Created by Holojam Inc. on 01.04.17

using UnityEngine;

// This is an example extension of the Controller class, with all the possible overrides
// and fields shown. In the majority of cases, you should override Synchronizable,
// Trackable, or SynchronizableTrackable instead of doing something like this.

public class ControllerTemplate : Holojam.Network.Controller {

  // Try adding two of these components, one sending and one not

  // Expose sending to the inspector
  [SerializeField] bool sending = true;

  // Controller property overrides

  public override string Label {
    get { return "label"; } // The unique identifier for this piece of data
  }

  public override string Scope {
    get { return ""; }
  }

  public override bool Sending {
    get { return sending; }
  }

  public override bool Deaf {
    get { return false; }
  }

  protected override ProcessDelegate Process {
    get { return PrintData; }
  }

  // This is a proxy--you need to make one if you want to access this Controller's data
  // from outside the class
  public Vector3 MyVector3 {
    get {
      return data.vector3s[0];
    }
    set {
      data.vector3s[0] = value;
    }
  }

  // Here's another proxy, but this time demonstrates a mapping from a core Flake
  // data type (byte) to something more abstract (an array of two bools)
  public bool[] MyBools {
    get {
      return new bool[] {
        data.bytes[0] == 1 ? true : false,
        data.bytes[1] == 1 ? true : false
      };
    }
    set {
      data.bytes[0] = value[0] ? (byte)1 : (byte)0;
      data.bytes[1] = value[1] ? (byte)1 : (byte)0;
    }
  }

  // Called in Update()
  void PrintData() {
    // Controller

    /*
    Debug.Log(
      "There are " + Holojam.Network.Controller.All<ControllerTemplate>().Count
      + " ControllersTemplate(s) in the scene"
    );
    */

    Debug.Log(
      "This ControllerTemplate is " + (Sending ? "sending" : "not sending")
      + " and " + (Tracked ? "tracked" : "not tracked") + "\n"
      + "It has the brand " + Brand
      + (Sending ? "" : " and is coming from " + Source),
      this
    );

    // FlakeComponent

    /*
    if (!Sending) {
      Debug.Log(
        "This ControllerTemplate was last updated at " + Timestamp
        + " (delta = " + DeltaTime() + "ms)",
        this
      );
    }
    */

    bool[] myBools = MyBools;

    Debug.Log(
      "ControllerTemplate: MyVector3 = " + MyVector3
      + ", MyBools = (" + myBools[0] + ", " + myBools[1] + ")",
      this
    );
  }

  // Controller method overrides

  // If you need to do something on enable
  protected override void OnEnable() {
    base.OnEnable();
    Debug.Log("ControllerTemplate enabled", this);
  }

  // If you need to do something on disable
  protected override void OnDisable() {
    base.OnDisable();
    Debug.Log("ControllerTemplate disabled", this);
  }

  // Don't do this unless you really need to!
  protected override void Update() {
    if (!Application.isPlaying) return;

    //Debug.Log("ControllerTemplate updating", this);

    Process(); // Mandatory call
  }

  // FlakeComponent method overrides

  // If you need to do something on Awake()
  protected override void Awake() {
    Debug.Log(
      "ControllerTemplate data before allocation: "
      + data, this
    );

    ResetData();

    Debug.Log(
      "ControllerTemplate data after allocation: "
      + data, this
    );
  }

  // You need to reset (allocate) this Controller's data before you can use it
  // Awake() calls ResetData() by default
  public override void ResetData() {
    data = new Holojam.Network.Flake(
      1, 2, 3, 4, 5, true
    );
  }
}
