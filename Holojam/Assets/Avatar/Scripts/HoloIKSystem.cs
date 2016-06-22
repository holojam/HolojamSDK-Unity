using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holojam.Avatar.IK;

namespace Holojam.Avatar {
	public class HoloIKSystem : MonoBehaviour {

		public string label;

		public string[] mocapLabels = new string[4] { "_lefthand", "_righthand", "_leftankle", "_rightankle" };

		public Transform head;
		public Transform leftHand;
		public Transform rightHand;
		public Transform leftFoot;
		public Transform rightFoot;

		public Vector3 handOffset;
		public Vector3 footOffset;
		public Color color = Color.white;
		[Range(0f, 1f)]
		public float pastel = 0.2f;
		public int detail = 10;


		protected MasterStream stream;
		protected List<SplineLimb> splines = new List<SplineLimb>();
		protected List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();

		void Awake() {
			stream = MasterStream.Instance;
			splines.AddRange(this.GetComponentsInChildren<SplineLimb>());
			renderers.AddRange(this.GetComponentsInChildren<SkinnedMeshRenderer>());
			this.InitObjectControllers();
		}

		void InitObjectControllers() {
			ObjectController c;
			GameObject[] objs = new GameObject[5] {head.gameObject,leftHand.gameObject,rightHand.gameObject,
										    leftFoot.gameObject, rightFoot.gameObject };
			string[] labels = new string[5] { label, label + mocapLabels[0], label + mocapLabels[1], label + mocapLabels[2], label + mocapLabels[3] };
			Vector3[] offsets = new Vector3[5] {Vector3.zero, new Vector3(handOffset.x*-1,handOffset.y,handOffset.z), handOffset,
													new Vector3(footOffset.x*-1,footOffset.y,footOffset.z), footOffset };
			for (int i = 0; i < 5; i++) {
				c = objs[i].GetComponent<ObjectController>();
				if (c == null)
					c = objs[i].AddComponent<ObjectController>();
				c.label = labels[i];
				c.offset = offsets[i];
			}
		}



		void Update() {
			this.PositionBody();
			foreach(SplineLimb spline in splines) {
				spline.color = this.color;
				spline.detail = this.detail;
			}
			foreach(Renderer renderer in renderers) {
				foreach (Material material in renderer.materials) {
					material.SetColor("_Color", Color.Lerp(this.color,Color.white, pastel));
				}
			}

		}

		Vector3 _nb;
		void PositionBody() {
			float la = leftFoot.rotation.eulerAngles.y;
			float ra = rightFoot.rotation.eulerAngles.y;
			float ny = ((Mathf.Abs(la - ra) > 180) ? (la + ra + 360) : (la + ra)) / 2f;
			ny = ny % 360;

			transform.rotation = Quaternion.Euler(0, ny, 0);
			//Debug.Log(ny);
			_nb = new Vector3((leftFoot.position.x + rightFoot.position.x) / 2f, 0f, (leftFoot.position.z + rightFoot.position.z) / 2f);
			Vector3 delta = this.transform.position - _nb;
			this.transform.position = _nb;

			leftFoot.position += delta;
			rightFoot.position += delta;
		}
	}
}

