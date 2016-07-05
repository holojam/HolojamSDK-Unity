using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Holojam{

	public class VRConsoleDebug : MonoBehaviour{

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

		public static void print(string s) {
			instance.print ("DEBUG: " + s);
		}

		public static void println(string s) {
			instance.println ("DEBUG: " + s);

		}
	}
}