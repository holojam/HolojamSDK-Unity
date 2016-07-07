using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Holojam.Network {
	public class HolojamView : MonoBehaviour {

		public static List<HolojamView> instances = new List<HolojamView>();

		public bool isMine;

		public string label;

		private Vector3 rawPosition;
		private Quaternion rawRotation;
		private int bits;
		private string blob;
		private bool isTracked = false;

		public Vector3 RawPosition {
			get {
				return rawPosition;
			}
			set {
				rawPosition = value;
			}
		}

		public Quaternion RawRotation {
			get {
				return rawRotation;
			}
			set {
				rawRotation = value;
			}
		}

		public int Bits {
			get {
				return bits;
			}
			set {
				bits = value;
			}
		}

		public string Blob {
			get {
				return blob;
			}
			set {
				blob = value;
			}
		}

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

