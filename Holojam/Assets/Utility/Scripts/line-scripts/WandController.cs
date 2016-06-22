using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Holojam
{
	public class WandController : MonoBehaviour {
		[HideInInspector]
		public MasterStream mstream;
		public string label;

		private int button_bits;

		void Start () {
			mstream = MasterStream.Instance;
		}
		
		void Update () {
			button_bits = mstream.getLiveObjectButtonBits (label);
			Debug.Log (button_bits + " " + this.name);
		}

		public int getButtonBits() {
			return button_bits;
		}
	}
}