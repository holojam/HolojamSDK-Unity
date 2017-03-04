using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Holojam.Vive {
  public class ViveControllerRelay : Tools.Relay {

    public string label = "left";
    private SteamVR_TrackedObject controller;

    /// <summary>
    /// The ViveControllerRelay doesn't send on master clients.
    /// </summary>
    public override bool Sending { get { return !Holojam.Tools.BuildManager.IsMasterClient(); } }

    /// <summary>
    /// Automatically selects the canon Vive label depending on the build index, with an additional controller label.
    /// </summary>
    public override string Label {
      get { return Network.Canon.IndexToLabel(Tools.BuildManager.BUILD_INDEX,label); }
    }

    /// <summary>
    /// Data descriptor is initialized with one Vector3 and one Quaternion.
    /// </summary>
    public override void ResetData() {
      data = new Network.Flake(1, 1, 4, 6);
    }

    /// <summary>
    /// Send position and rotation every update.
    /// </summary>
    protected override void Load() {
      Position = transform.position;
      Rotation = transform.rotation;

      //Set press ints
      AppMenuPress = GetPress(EVRButtonId.k_EButton_ApplicationMenu);
      GripPress = GetPress(EVRButtonId.k_EButton_Grip);
      TouchpadPress = GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad);
      TriggerPress = GetPress(EVRButtonId.k_EButton_SteamVR_Trigger);

      //Set touch ints
      TouchpadTouch = GetTouch(EVRButtonId.k_EButton_SteamVR_Touchpad);
      TriggerTouch = GetTouch(EVRButtonId.k_EButton_SteamVR_Trigger);

      //Set axes
      TouchpadAxis = SteamVR_Controller.Input(index).GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
      TriggerAxis = SteamVR_Controller.Input(index).GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger);
    }



    private bool GetPress(EVRButtonId id) {
      return SteamVR_Controller.Input(index).GetPressDown(id) 
        || SteamVR_Controller.Input(index).GetPress(id);
    }

    private bool GetTouch(EVRButtonId id) {
      return SteamVR_Controller.Input(index).GetTouchDown(id)
        || SteamVR_Controller.Input(index).GetTouch(id);
    }

    private int index {
      get { return (int)controller.index; }
    }

    private bool AppMenuPress {
      get { return data.ints[0] == 1; }
      set { data.ints[0] = (value ? 1 : 0); }
    }

    private bool GripPress {
      get { return data.ints[1] == 1; }
      set { data.ints[1] = (value ? 1 : 0); }
    }

    private bool TouchpadPress {
      get { return data.ints[2] == 1; }
      set { data.ints[2] = (value ? 1 : 0); }
    }

    private bool TriggerPress {
      get { return data.ints[3] == 1; }
      set { data.ints[3] = (value ? 1 : 0); }
    }

    private bool TouchpadTouch {
      get { return data.ints[4] == 1; }
      set { data.ints[4] = (value ? 1 : 0); }
    }

    private bool TriggerTouch {
      get { return data.ints[5] == 1; }
      set { data.ints[5] = (value ? 1 : 0); }
    }

    private Vector2 TouchpadAxis {
      get { return new Vector2(data.floats[0], data.floats[1]); }
      set { data.floats[0] = value.x; data.floats[1] = value.y; }
    }

    private Vector2 TriggerAxis {
      get { return new Vector2(data.floats[2], data.floats[3]); }
      set { data.floats[2] = value.x; data.floats[3] = value.y; }
    }


    protected override void Awake() {
      base.Awake();

      controller = GetComponent<SteamVR_TrackedObject>();
      if (controller == null) {
        throw new System.Exception("ViveControllerRelay missing controller.");
      }
    }
  }
}
