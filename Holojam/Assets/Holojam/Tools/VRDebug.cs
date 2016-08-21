using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Holojam.Tools{

	public class VRDebug : MonoBehaviour{

		static VRConsole debug = null;

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

		public static void print(string s, bool printToDebug = false) {
			instance.queueForPrinting(s);
			if (printToDebug) {
				Debug.Log(s);
			}
		}

		public static void println(string s, bool printToDebug = false) {
			instance.queueForPrinting (s + "\n");
			if (printToDebug) {
				Debug.Log (s);
			}
		}
	}
}