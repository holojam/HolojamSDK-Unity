//Converter.cs
//Created by Holojam Inc. on 11.11.16
//

#define SMOOTH

using UnityEngine;

namespace Holojam.Tools {

  /// <summary>
  /// 
  /// </summary>
  public class Converter : MonoBehaviour {
    [System.Serializable]
    public class Smoothing {
      public float cap, pow;
      public Smoothing(float cap, float pow) {
        this.cap = cap;
        this.pow = pow;
      }
    }
    readonly Smoothing XY_SMOOTHING = new Smoothing(.05f, 1.1f);
    readonly Smoothing Z_SMOOTHING = new Smoothing(.15f, 2);
    const float R_SMOOTHING = .12f;

    public BuildManager buildManager;
    public Scope extraData;

    public enum DebugMode { NONE, POSITION, REMOTE }
    public DebugMode debugMode = DebugMode.NONE;

#if SMOOTH
    Vector3 lastLeft = Vector3.zero, lastRight = Vector3.zero;
#endif

    Network.View input, output;
    Transform imu;
    Network.View test;
    Quaternion raw, correction, correctionTarget = Quaternion.identity;

    //Proxies
    public Vector3 outputPosition {
      get { return output.triples[0]; }
      set { output.triples[0] = value; }
    }
    public Quaternion outputRotation {
      get { return output.quads[0]; }
      set { output.quads[0] = value; }
    }
    public bool hasInput { get { return input.tracked; } }

    void Awake() {
      if (BuildManager.DEVICE == BuildManager.Device.VIVE)
        return;

      //Ignore debug flags on builds
      if (!BuildManager.IsMasterClient())
        debugMode = DebugMode.NONE;

      if (buildManager == null) {
        Debug.LogWarning("Converter: Build Manager reference is null!");
        return;
      }
      if (extraData == null) {
        Debug.LogWarning("Converter: Extra data reference is null!");
        return;
      }
      if (BuildManager.IsMasterClient() && debugMode == DebugMode.NONE)
        return;

      imu = buildManager.viewer.transform.GetChild(0);
      if (debugMode == DebugMode.REMOTE) {
        test = gameObject.AddComponent<Network.View>() as Network.View;
        test.label = "Remote";
        test.scope = "Holojam";
        test.sending = false;
      }

      input = gameObject.AddComponent<Network.View>() as Network.View;
      input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX, true);
      input.scope = "Holoscope";
      input.sending = false;

      output = gameObject.AddComponent<Network.View>() as Network.View;
      output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);
      output.scope = Network.Client.SEND_SCOPE;
      output.sending = true;

      //Allocate
      input.triples = new Vector3[2];
      output.triples = new Vector3[1];
      output.quads = new Quaternion[1];
    }

    void Update() {
      if (BuildManager.DEVICE == BuildManager.Device.VIVE)
        return;

      //Editor debugging
      if (BuildManager.IsMasterClient()) {
        if (debugMode == DebugMode.POSITION) {
#if SMOOTH
          outputPosition = extraData.Localize((
             SmoothPosition(input.triples[0], ref lastLeft) +
             SmoothPosition(input.triples[1], ref lastRight)) * .5f
          );
#else
                  outputPosition = extraData.Localize(.5f*(input.triples[0]+input.triples[1]));
#endif
          return;
        } else if (debugMode == DebugMode.NONE) return;
      }

#if SMOOTH
      Vector3 left = SmoothPosition(input.triples[0], ref lastLeft);
      Vector3 right = SmoothPosition(input.triples[1], ref lastRight);
#else
            Vector3 left = input.triples[0], right = input.triples[1];
#endif
      Vector3 inputPosition = .5f * (left + right);

      //Update views
      input.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX, true);
      output.label = Network.Canon.IndexToLabel(BuildManager.BUILD_INDEX);

      //Get IMU data
      if (debugMode == DebugMode.REMOTE)
        raw = test.quads[0];
      else switch (BuildManager.DEVICE) {
          case BuildManager.Device.CARDBOARD:
          raw = imu.localRotation;
          break;
          case BuildManager.Device.DAYDREAM:
          raw = UnityEngine.VR.InputTracking.GetLocalRotation(
             UnityEngine.VR.VRNode.CenterEye
          );
          break;
        }

      //Update target if tracked
      if (input.tracked) {
        //Read in secondary vector
        Vector3 nbar = (right - left).normalized;

        Vector3 imuUp = raw * Vector3.up;
        Vector3 imuForward = raw * Vector3.forward;
        Vector3 imuRight = raw * Vector3.right;

        //Compare orientations relative to gravity
        Quaternion difference = Quaternion.LookRotation(-nbar, Vector3.up)
           * Quaternion.Inverse(Quaternion.LookRotation(imuRight, Vector3.up));

        Vector3 newForward = difference * imuForward;
        Vector3 newUp = difference * imuUp;

        //Ideal rotation
        Quaternion target = Quaternion.LookRotation(newForward, newUp);
        correctionTarget = target * Quaternion.Inverse(raw);
      }

#if SMOOTH
      //Lazily interpolate correction (only has to be a baseline, not immediate)
      correction = Quaternion.Slerp(
         correction, correctionTarget, Time.deltaTime * R_SMOOTHING
      );
#else
            correction = correctionTarget;
#endif

      //Update output
      outputRotation = extraData.Localize(correction * raw);
      outputPosition = extraData.Localize(
         //inputPosition - outputRotation*Vector3.up*extraData.stem
         inputPosition
      );
    }

    //Smooth signal while minimizing perceived latency
    Vector3 SmoothPosition(Vector3 target, ref Vector3 last) {
      Vector2 xyLast = new Vector2(last.x, last.y);
      Vector2 xyTarget = new Vector2(target.x, target.y);

      Vector2 xy = Vector2.Lerp(xyLast, xyTarget, Mathf.Pow(
         Mathf.Min(1, (xyLast - xyTarget).magnitude / XY_SMOOTHING.cap), XY_SMOOTHING.pow
      ));
      float z = Mathf.Lerp(last.z, target.z, Mathf.Pow(
         Mathf.Min(1, Mathf.Abs(last.z - target.z) / Z_SMOOTHING.cap), Z_SMOOTHING.pow
      ));

      target = new Vector3(xy.x, xy.y, z);

      last = target;
      return target;
    }
  }
}
