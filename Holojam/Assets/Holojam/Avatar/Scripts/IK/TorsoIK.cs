using UnityEngine;
using System.Collections;

namespace Holojam.Avatar.IK {
   // [ExecuteInEditMode]
    public class TorsoIK : MonoBehaviour {


        /////Protected/////
        //References
        [Header("References Bones")]
        [SerializeField]
        protected Transform head;
        [SerializeField]
        protected Transform torso;
        [SerializeField]
        protected Transform leftHip;
        [SerializeField]
        protected Transform rightHip;
        [SerializeField]
        protected Transform leftFoot;
        [SerializeField]
        protected Transform rightFoot;
        //Primitives
        protected float distanceToHead;
        protected float distanceToFloor;
        protected float distanceToHips;

        void Awake() {
            distanceToHead = Vector3.Distance(torso.position, head.position);
            distanceToFloor = Vector3.Distance(torso.position, Vector3.zero);
            distanceToHips = Vector3.Distance((leftHip.position + rightHip.position) / 2f, torso.position);
        }

        // Update is called once per frame
        void Update() {

            //figure out the forward of the torso
            float leftAngle = leftFoot.rotation.eulerAngles.y;
            float rightAngle = rightFoot.rotation.eulerAngles.y;
            float angle = ((Mathf.Abs(leftAngle - rightAngle) > 180) ? (leftAngle + rightAngle + 360f) : (leftAngle + rightAngle)) / 2f;
            angle = angle % 360f;

            //figure out the center of mass (assumed to always be between the two feet
            Vector3 center = new Vector3((leftFoot.position.x + rightFoot.position.x) / 2f, 0f, (leftFoot.position.z + rightFoot.position.z) / 2f);


            torso.rotation = Quaternion.Euler(0f,angle,0f);

            //put the torso between the center and the head.
            Vector3 torsoPosition = head.position - ((head.position - center).normalized * distanceToHead);
            torso.position = torsoPosition;
			//torso.up = ((head.position - center).normalized);
        }
    }

}
