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
        [Header("Motive Label")]
        public string label;
        [Header("Motive Sublabels")]
        public string[] mocapLabels = new string[4] { "_lefthand", "_righthand", "_leftankle", "_rightankle" };
       
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
    }
}

