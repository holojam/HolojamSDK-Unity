using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holojam.Avatar.IK;

namespace Holojam.Avatar {

    public class StuttIKSystem : MonoBehaviour {



        /////Public/////
        //References
        //Primitives
        [Header("Connect to Motive")]
        public bool connect = true;
        [Header("Tracked Object Tags")]
        public LiveObjectTag headTag;
        public LiveObjectTag leftHandTag;
        public LiveObjectTag rightHandTag;
        public LiveObjectTag leftFootTag;
        public LiveObjectTag rightFootTag;
       
        /////Protected/////
        //References
        protected List<SplineLimb> splines = new List<SplineLimb>();
        protected List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        [SerializeField]
        protected Transform head;
        [SerializeField]
        protected Transform leftHand;
        [SerializeField]
        protected Transform rightHand;
        [SerializeField]
        protected Transform leftFoot;
        [SerializeField]
        protected Transform rightFoot;
        //Primitives
        [SerializeField]
        protected Vector3 handOffset;
        [SerializeField]
        protected Vector3 footOffset;
        [SerializeField]
        protected Color color = Color.white;
        [SerializeField]
        [Range(0f, 1f)]
        protected float pastel = 0.2f;
        [SerializeField]
        protected int detail = 10;


        void Awake() {
            splines.AddRange(this.GetComponentsInChildren<SplineLimb>());
            renderers.AddRange(this.GetComponentsInChildren<SkinnedMeshRenderer>());
            if (connect)
                this.InitObjectControllers();
        }

        // Update is called once per frame
        void Update() {
            foreach (SplineLimb spline in splines) {
                spline.color = this.color;
                spline.detail = this.detail;
            }
            foreach (Renderer renderer in renderers) {
                foreach (Material material in renderer.materials) {
                    material.SetColor("_Color", Color.Lerp(this.color, Color.white, pastel));
                }
            }
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
    }
}

