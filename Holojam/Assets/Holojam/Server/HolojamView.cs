using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Holojam.Network {
	public class HolojamView : MonoBehaviour {

		public static List<HolojamView> instances = new List<HolojamView>();

		public bool isMine;

		public string label;

		[HideInInspector]
		public Vector3 rawPosition;
		[HideInInspector]
		public Quaternion rawRotation;
		[HideInInspector]
		public int bits = 0;
		[HideInInspector]
		public string blob = "";

		private bool isTracked = false;

		public bool IsTracked {
			get {
				return isMine || isTracked;
			}
			set {
				isTracked = value;
			}
		}

		private void OnEnable() {
			instances.Add(this);
		}

		private void OnDisable() {
			instances.Remove(this);
		}
	}
}

