using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Holojam{

	public class VRConsole : MonoBehaviour {

		string privateCache;
		public int numLinesDisplayed = 5;
		public bool linewrapOn = true;
		public int numCharsPerLine = 20;

		char[] newLineArray = new char[]{'\n'};

		// Use this for initialization
		void Start () {
			privateCache = "";
		}
		
		// Update is called once per frame
		public virtual void Update () {
			toggleDisplay ();
			//println (Time.time.ToString());
			//var centerEye = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.CenterEye);
			//var head = UnityEngine.VR.InputTracking.GetLocalPosition (UnityEngine.VR.VRNode.Head);
			//var leftEye = UnityEngine.VR.InputTracking.GetLocalPosition (UnityEngine.VR.VRNode.LeftEye);
			//var rightEye = UnityEngine.VR.InputTracking.GetLocalPosition (UnityEngine.VR.VRNode.RightEye);
			//VRConsoleDebug.println("CenterEye: (" + centerEye.x.ToString("F8") + "," + centerEye.y.ToString("F8") + "," + centerEye.z.ToString("F8") + ")");
			//print ("Head: " + head);
			//print ("LeftEye: " + leftEye);
			//print ("RightEye: " + rightEye);
			//VRConsoleDebug.println("CenterEye: " + Vector3.Distance(centerEye, Vector3.zero).ToString("F8"));
			//print ("Head: " + Vector3.Distance(head, Vector3.zero));
			//print ("LeftEye: " + Vector3.Distance(leftEye, Vector3.zero));
			//print ("HeadToCenter: " + Vector3.Distance(centerEye, head));
			//print ("RightEye: " + Vector3.Distance(rightEye, Vector3.zero));
			reformat ();
		}

		private void toggleDisplay() {
			if (Input.GetButtonDown ("Tap")) {
				getConsole ().GetComponent<Renderer> ().enabled = !getConsole ().GetComponent<Renderer> ().enabled;
			}
		}

		private TextMesh getConsole() {
			return gameObject.GetComponent<TextMesh> ();
		}

		private string getText() {
			return getConsole ().text;
		}

		private void setText(string s) {
			getConsole ().text = s;
		}

		void clearConsole () {
			setText ("");
		}

		public void print(string s, bool printToDebugger = false) {
			setText(getText() + s);
			if (printToDebugger) {
				Debug.Log (s);
			}
		}

		public void println() {
			print ("\n");
		}

		public void println(string s, bool printToDebugger = false) {
			print(s + "\n");
			if (printToDebugger) {
				Debug.Log (s);
			}
		}

		void replaceAllInstancesOfChar(char c) {
			setText(getConsole().text.Replace("\n",""));
		}

		int countInstancesOfChar(char c, string s) {
			int count = 0;
			foreach (char cc in s) {
				if (cc.Equals(c)) {
					count++;
				}
			}
			return count;
		}

		void reformat() {
			wrapLines ();
			cull ();
		}

		void wrapLines() {
			if (!linewrapOn)
				return;

			string copy = getText (),
			       build = "";
			string[] strings;

			while (copy.Length > 0) {
				strings = copy.Split (newLineArray, 2);
				build += Regex.Replace (strings[0], ".{"+numCharsPerLine+"}(?!$)", "$0\n");
				build += "\n";
				copy = strings [1];
			}
			setText (build);
		}

		void cull() {
			var temp = getText ();

			string[] strings;

			var numNewLines = countInstancesOfChar ('\n', temp);

			while (numNewLines > numLinesDisplayed) {
				strings = temp.Split(newLineArray, 2);
				privateCache += (strings [0] + '\n');
				temp = strings [1];
				numNewLines--;
			}
			setText (temp);
		}
	}
}