using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace Holojam.Tools{

	public class VRConsole : MonoBehaviour {

		//string privateCache;
		string freshStrings;
		public int numLinesDisplayed = 5;
		public bool linewrapOn = true;
		public int numCharsPerLine = 20;

		char[] newLineArray = new char[]{'\n'};

		// Use this for initialization
		void Start () {
			//privateCache = "";
			freshStrings = "";
			getConsole ().GetComponent<Renderer> ().enabled = false;
		}
		
		// Update is called once per frame
		public void FixedUpdate () {
			toggleDisplay ();
			printFreshStrings();
			reformat ();
		}

		private void printFreshStrings() {
			if (freshStrings.Equals ("")) {
				return;
			}
			if (freshStrings.LastIndexOf ("\n") != freshStrings.Length - 1) {
				freshStrings += "\n";
			}
			print (freshStrings);
			//privateCache += freshStrings;
			freshStrings = "";
		}

		private void toggleDisplay() {
			if (Input.GetKeyDown (KeyCode.Mouse0)) {
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

		public void queueForPrinting(string s) {
			freshStrings += s;
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
				if (strings.Length < 2) {
					break;
				} 
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
				//privateCache += (strings [0] + '\n');
				temp = strings [1];
				numNewLines--;
			}
			setText (temp);
		}
	}
}