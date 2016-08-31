using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Holojam.Tools{

	public class VRDebug : MonoBehaviour{

		static VRConsole debug = null;

		static bool debugOn = false;

		private float lastPressed = -1f;

		static VRConsole instance {
			get {
				if (debug == null) {
					debug = (VRConsole)FindObjectOfType (typeof(VRConsole));
					if (FindObjectsOfType (typeof(VRConsole)).Length > 1) {
						Debug.Log ("More than one VRConsole in scene; Debug statements may be written to different console than expected.");
					}
				}
				return debug;
			}
		}

		public void Update() {
			if (Input.GetKey (KeyCode.Mouse0)) {
				Debug.Log ("Currently pressing");
				if (lastPressed == -1f) {
					lastPressed = Time.time;
				} else {
					if (Time.time - lastPressed > 3) {
						lastPressed = -1f;
						debugOn = !debugOn;
					}
				}
			} else {
				lastPressed = -1f;
			}
		}

		public static void Print(string s, bool printToDebug = false) {
			if (debugOn) {
				instance.queueForPrinting (s);
				if (printToDebug) {
					Debug.Log (s);
				}
			}
		}

		public static void Println(string s, bool printToDebug = false) {
			if (debugOn) {
				instance.queueForPrinting (s + "\n");
				if (printToDebug) {
					Debug.Log (s);
				}
			}
		}
	}
}