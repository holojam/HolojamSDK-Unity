using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holojam.Avatar.IK;

namespace Holojam.Avatar {
	public class HoloIKSystem : MonoBehaviour {

          public LiveObjectTag headTag;
          public LiveObjectTag leftHandTag;
          public LiveObjectTag rightHandTag;
          public LiveObjectTag leftFootTag;
          public LiveObjectTag rightFootTag;

		public Transform head;
		public Transform leftHand;
		public Transform rightHand;
		public Transform leftFoot;
		public Transform rightFoot;

          //public Vector3 handOffset;
          //public Vector3 footOffset;
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
			GameObject[] objs = new GameObject[5] {head.gameObject,leftHand.gameObject,rightHand.gameObject,
										    leftFoot.gameObject, rightFoot.gameObject };
               LiveObjectTag[] tags = new LiveObjectTag[5] { headTag, leftHandTag, rightHandTag, leftFootTag, rightFootTag };

               for (int i = 0; i < 5; i++) {
                    GameObject go = objs[i];
                    TrackedObject trackedObject = go.GetComponent<TrackedObject>();
                    if (trackedObject == null)
                         trackedObject = go.AddComponent<TrackedObject>();
                    trackedObject.liveObjectTag = tags[i];
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

