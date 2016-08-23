//HoloIK.cs
//Created by Aaron C Gaudette on 19.08.16

using UnityEngine;
using System.Collections;

namespace Holojam.Tools{
   [RequireComponent(typeof(PairTarget))]
   public class HoloIK : MonoBehaviour{
      public Transform head; //Center of head, must be pointing forward
      public float headLength = 0.21f; //Basic scaling unit
      public Transform groundPlane;
      public Transform torso, partialTorso; //Torso renderers, with pivot below neck
      public Transform eyes; //Optional, for blinking

      [Space(8)]
      public Material skin;
      public Renderer face;
      public Material clothes;
      public Renderer limbPrefab;
      public Renderer seamPrefab;

      [Header("Read-only:")]
      public Pairable leftHand;
      public Pairable rightHand;
      public Pairable leftFoot;
      public Pairable rightFoot;

      [HideInInspector] public Transform neck;
      [HideInInspector] public Transform leftShoulder, rightShoulder;
      [HideInInspector] public Transform leftHip, rightHip;

      const float SHOULDER_WIDTH_FACTOR = 0.86f;
      const float TORSO_LENGTH_FACTOR = 2; //Hip height
      const float HIP_WIDTH_FACTOR = 0.65f;

      const float BLINK_TIME = 0.085f;
      readonly Vector2 BLINK_DELAY = new Vector2(1,11);

      const string HAND = "Hand", FOOT = "Foot";

      FullTorso fullTorso = new FullTorso();
      HalfTorso halfTorso = new HalfTorso();
      Arm leftArm = new Arm(true), rightArm = new Arm(false);
      Leg leftLeg = new Leg(), rightLeg = new Leg();

      [HideInInspector] public Renderer leftLowerArm, leftUpperArm;
      [HideInInspector] public Renderer rightLowerArm, rightUpperArm;
      [HideInInspector] public Renderer leftLowerLeg, leftUpperLeg;
      [HideInInspector] public Renderer rightLowerLeg, rightUpperLeg;
      [HideInInspector] public Renderer leftElbow, rightElbow;
      [HideInInspector] public Renderer leftKnee, rightKnee;

      float blinkDelay = 0, lastBlink = 0;

      //Manage hands and feet
      void OnPair(Pairable p){
         switch(p.type){
            case HAND:
               if(rightHand==null)rightHand = p;
               else if(leftHand==null)leftHand = p;
               break;
            case FOOT:
               if(rightFoot==null)rightFoot = p;
               else if(leftFoot==null)leftFoot = p;
               break;
         }
      }
      void OnUnpair(Pairable p){
         if(p==leftHand)leftHand = null;
         else if(p==rightHand)rightHand = null;
         else if(p==leftFoot)leftFoot = null;
         else if(p==rightFoot)rightFoot = null;
      }

      void LateUpdate(){
         //Blink
         if(eyes!=null && Time.time>lastBlink+blinkDelay)Blink();

         if(head==null || torso==null || partialTorso==null
            || groundPlane==null || limbPrefab==null || seamPrefab==null){
            Debug.LogWarning("HoloIK: Reference(s) null, stopping execution",this);
            return;
         }

         BuildJoint("Neck",ref neck); //Oriented by Torso

         //If both feet are found, use the full torso
         torso.gameObject.SetActive(leftFoot!=null && rightFoot!=null);
         //Otherwise, if at least one hand is found, use the half torso
         partialTorso.gameObject.SetActive(leftHand!=null || rightHand!=null);

         if(partialTorso.gameObject.activeSelf)
            halfTorso.Process(headLength,partialTorso,head.position,neck,groundPlane,
               leftHand,rightHand,head.forward);
         if(torso.gameObject.activeSelf)
            fullTorso.Process(headLength,torso,head.position,neck,groundPlane,
               partialTorso,leftFoot,rightFoot);

         //Place joints (shoulders and hips)
         if(torso.gameObject.activeSelf || partialTorso.gameObject.activeSelf){
            BuildJoints();
            OrientJoints();
         }

         //Place limbs
         BuildLimbs();
         DisplayLimbs();

         //Arms and legs
         if(leftHand!=null){
            leftArm.Process(headLength,leftShoulder,leftHand.center,
               leftUpperArm.transform,leftLowerArm.transform,leftElbow.transform
            );
         }
         if(rightHand!=null){
            rightArm.Process(headLength,rightShoulder,rightHand.center,
               rightUpperArm.transform,rightLowerArm.transform,rightElbow.transform
            );
         }
         if(leftFoot!=null){
            leftLeg.Process(headLength,leftHip.position,leftFoot,
               leftUpperLeg.transform,leftLowerLeg.transform,leftKnee.transform
            );
         }
         if(rightFoot!=null){
            rightLeg.Process(headLength,rightHip.position,rightFoot,
               rightUpperLeg.transform,rightLowerLeg.transform,rightKnee.transform
            );
         }
      }

      //IK processing classes

      abstract class Torso{
         float headLength;
         protected Transform torso;
         protected Vector3 head;
         protected Transform neck;
         protected Transform groundPlane;

         public virtual void Process(float headLength, Transform torso,
            Vector3 head, Transform neck, Transform groundPlane){

            this.headLength = headLength;
            this.torso = torso;
            this.head = head;
            this.neck = neck;
            this.groundPlane = groundPlane;

            Calculate();
            Apply();
         }

         const float NECK_LENGTH_FACTOR = 1;
         protected float neckLength{get{return headLength*NECK_LENGTH_FACTOR;}}

         protected abstract void Calculate();
         protected abstract void Apply();
      }

      class FullTorso : Torso{
         Transform partialTorso;
         Pairable leftFoot, rightFoot;

         public void Process(float headLength, Transform torso, Vector3 head, Transform neck,
            Transform groundPlane, Transform partialTorso, Pairable leftFoot, Pairable rightFoot){

            this.partialTorso = partialTorso;
            this.leftFoot = leftFoot;
            this.rightFoot = rightFoot;
            base.Process(headLength,torso,head,neck,groundPlane);
         }

         protected Vector3 ProjectToGround(Vector3 v){
            return v + groundPlane.up * -Vector3.Dot(groundPlane.up,v);
         }

         const float LEAN = 0.3f;

         Pairable lowestFoot;
         Vector3 rawForward, leanedForward, up;

         protected override void Calculate(){
            lowestFoot = Vector3.Distance(leftFoot.center,head)<
               Vector3.Distance(rightFoot.center,head)?rightFoot:leftFoot;
            //Median point between feet, at lowest foot height
            Vector3 foundation = (leftFoot.center+rightFoot.center)*0.5f;
            foundation = Vector3.Project(lowestFoot.center-head,foundation-head) + head;

            Vector3 planedLeftFoot = ProjectToGround(leftFoot.transform.forward).normalized;
            Vector3 planedRightFoot = ProjectToGround(rightFoot.transform.forward).normalized;
            //Calculate facing direction
            rawForward = ((planedRightFoot+planedLeftFoot)*0.5f).normalized;

            up = (head-foundation).normalized;
            //Calculate leaning direction
            leanedForward = Vector3.Cross(
               Vector3.Cross(groundPlane.up,rawForward),up
            ).normalized;
         }
         protected override void Apply(){
            neck.position = head - up*neckLength;
            torso.position = partialTorso.position = neck.position;
            //Slerp between no lean and full lean
            torso.rotation = partialTorso.rotation = Quaternion.Slerp(
               Quaternion.LookRotation(rawForward,groundPlane.up),
               Quaternion.LookRotation(leanedForward,up),LEAN);
         }
      }
      class HalfTorso : Torso{
         Pairable leftHand, rightHand;
         Vector3 headForward;

         public void Process(float headLength, Transform torso, Vector3 head, Transform neck,
            Transform groundPlane, Pairable leftHand, Pairable rightHand, Vector3 headForward){

            this.leftHand = leftHand;
            this.rightHand = rightHand;
            this.headForward = headForward;
            base.Process(headLength,torso,head,neck,groundPlane);
         }

         Vector3 leftForward, rightForward, armsForward, forward;

         protected override void Calculate(){
            if(leftHand!=null)leftForward = (leftHand.center-head).normalized;
            if(rightHand!=null)rightForward = (rightHand.center-head).normalized;

            if(leftHand!=null && rightHand!=null){
               armsForward = 0.5f*(leftForward+rightForward);
               armsForward.Normalize();
            }
            else armsForward = (leftHand!=null?leftForward:rightForward).normalized;

            //Weight arm and head vectors based on their relative "Y" rotation
            float armsWeight = 1-Mathf.Abs(Vector3.Dot(armsForward,groundPlane.up));
            armsWeight*=armsWeight;
            float headWeight = 1-Mathf.Abs(Vector3.Dot(headForward,groundPlane.up));
            headWeight*=headWeight;

            forward = 0.5f*(Vector3.Lerp(forward,armsForward,armsWeight)+
               Vector3.Lerp(forward,headForward,headWeight)
            );
         }
         protected override void Apply(){
            //Not enough information to determine true up vector
            neck.position = head - groundPlane.up*neckLength;

            torso.position = neck.position;
            torso.rotation = Quaternion.LookRotation(forward,groundPlane.up);
         }
      }

      //Adapted from ArmIK.cs
      class Arm{
         bool left;
         public Arm(bool left){this.left=left;}

         float headLength;
         Transform shoulder;
         Vector3 hand;

         Transform upperRenderArm, lowerRenderArm, renderElbow;

         public void Process(float headLength, Transform shoulder, Vector3 hand,
            Transform upperRenderArm, Transform lowerRenderArm, Transform renderElbow){

            this.headLength = headLength;
            this.shoulder = shoulder;
            this.hand = hand;
            this.upperRenderArm = upperRenderArm;
            this.lowerRenderArm = lowerRenderArm;
            this.renderElbow = renderElbow;

            Calculate();
            Render();
         }

         const float R = 0.7071067812f; //Square root of 0.5
         readonly float[,,] HINT_MAP_LEFT = new float[,,]{
            {{-1, 0, 0}, { 0,-R,-R}},
            {{ 1, 0, 0}, { 0, 0, 1}},
            {{ 0, 1, 0}, {-1, 0, 0}},
            {{ 0,-1, 0}, {-R, 0,-R}},
            {{ 0, 0, 1}, {-R,-R, 0}},
            {{-R, 0, R}, {-R, 0,-R}},
            {{ R, 0, R}, {-R, 0, R}},
         };
         readonly float[,,] HINT_MAP_RIGHT = new float[,,]{
            {{ 1, 0, 0}, { 0,-R,-R}},
            {{-1, 0, 0}, { 0, 0, 1}},
            {{ 0, 1, 0}, { 1, 0, 0}},
            {{ 0,-1, 0}, { R, 0,-R}},
            {{ 0, 0, 1}, { R,-R, 0}},
            {{ R, 0, R}, { R, 0,-R}},
            {{-R, 0, R}, { R, 0, R}},
         };
         const int HINT_MAP_LENGTH = 7;
         Vector3 HintMap(int x, int y){
            return left?
               new Vector3(HINT_MAP_LEFT[x,y,0],HINT_MAP_LEFT[x,y,1],HINT_MAP_LEFT[x,y,2]):
               new Vector3(HINT_MAP_RIGHT[x,y,0],HINT_MAP_RIGHT[x,y,1],HINT_MAP_RIGHT[x,y,2]);
         }

         //Human-scaled values
         const float ELBOW_LENGTH_FACTOR = 1.5f;
         const float ARM_LENGTH_FACTOR = 1.3f;
         const float HAND_OFFSET = 0.14f;

         const float SMOOTH_A = 0.2f, SMOOTH_B = 0.9f;

         float elbowLength{get{return headLength*ELBOW_LENGTH_FACTOR;}}
         float armLength{get{return headLength*ARM_LENGTH_FACTOR;}}

         Vector3 shoulderExtension; //Direction and distance to extend elbow from shoulder
         Vector3 elbow{get{return shoulder.position+shoulder.rotation*shoulderExtension;}}

         void Calculate(){
            Vector3 endpoint = shoulder.InverseTransformPoint(hand);
            shoulderExtension = Hint(endpoint);

            //Smooth factor
            float t = endpoint.magnitude/(elbowLength+armLength) - SMOOTH_A;
            t = Mathf.Clamp01(t*t*(3-t-t));
            t = SMOOTH_A * Mathf.Sqrt(t) + SMOOTH_B;

            //Smooth lengths
            float el = elbowLength*t;
            float al = armLength*t;

            float distanceSquared = Vector3.Dot(endpoint,endpoint);
            float elbowLengthSquared = el*el;

            float x = 0.5f*(1 + (elbowLengthSquared - al*al)/distanceSquared);
            float y = Vector3.Dot(endpoint,shoulderExtension)/distanceSquared;
            shoulderExtension -= y*endpoint;
            y = Mathf.Sqrt(
               Mathf.Max(0,elbowLengthSquared-distanceSquared * x*x)
               /Vector3.Dot(shoulderExtension,shoulderExtension
            ));

            shoulderExtension = endpoint*x + shoulderExtension*y;
         }
         //Given the endpoint, make an educated guess as to what the extension vector should be
         protected Vector3 Hint(Vector3 endpoint){
            Vector3 hint = Vector3.zero;
            for(int i=0;i<HINT_MAP_LENGTH;++i){
               float d = Vector3.Dot(HintMap(i,0),endpoint);
               if(d>0)hint += HintMap(i,1)*d;
            }
            return hint.normalized;
         }

         void Render(){
            Debug.DrawLine(shoulder.position,elbow,Color.white);
            Debug.DrawLine(elbow,hand,Color.white);

            upperRenderArm.position = shoulder.position;
            upperRenderArm.forward = elbow-shoulder.position;
            upperRenderArm.localScale =
               new Vector3(upperRenderArm.localScale.x,upperRenderArm.localScale.y,
                  Vector3.Distance(elbow,shoulder.position));

            lowerRenderArm.position = elbow;
            lowerRenderArm.forward = hand-elbow;
            lowerRenderArm.localScale =
               new Vector3(lowerRenderArm.localScale.x,lowerRenderArm.localScale.y,
                  Vector3.Distance(hand-lowerRenderArm.forward*HAND_OFFSET,elbow));

            renderElbow.position = elbow;
            renderElbow.forward = elbow-shoulder.position;
         }
      }

      class Leg{
         float headLength;
         Vector3 hip;
         Pairable foot;

         Transform lowerRenderLeg, upperRenderLeg, renderKnee;

         public void Process(float headLength, Vector3 hip, Pairable foot,
            Transform upperRenderLeg, Transform lowerRenderLeg, Transform renderKnee){

            this.headLength = headLength;
            this.hip = hip;
            this.foot = foot;
            this.lowerRenderLeg = lowerRenderLeg;
            this.upperRenderLeg = upperRenderLeg;
            this.renderKnee = renderKnee;

            Calculate();
            Render();
         }

         //Human-scaled values
         const float THIGH_RATIO = 0.45f;
         //Complementary to TORSO_LENGTH_FACTOR and NECK_LENGTH_FACTOR
         const float LEG_LENGTH_FACTOR = 3;

         const float MIN_BEND = 0.03f;
         const float TIGHT_BIAS = 1.1f;
         const float BEND_POWER = 1.6f;

         float legLength{get{return headLength*LEG_LENGTH_FACTOR;}}

         Vector3 knee;

         void Calculate(){
            //Foot offset out of total leg length
            float stretch = Vector3.Distance(hip,foot.center)/legLength;
            stretch = Mathf.Min(1,Mathf.Pow(stretch,BEND_POWER)); //Exponentiate for a better fit

            float bend = Mathf.Lerp(
               0.5f * legLength * Mathf.Lerp(TIGHT_BIAS,1,stretch), //Length of leg half
               MIN_BEND,stretch
            );

            knee = Vector3.Lerp(hip,foot.center,THIGH_RATIO) + foot.transform.forward*bend;
         }
         void Render(){
            Debug.DrawLine(hip,knee,Color.white);
            Debug.DrawLine(knee,foot.center,Color.white);

            upperRenderLeg.position = hip;
            upperRenderLeg.forward = knee-hip;
            upperRenderLeg.localScale =
               new Vector3(upperRenderLeg.localScale.x,upperRenderLeg.localScale.y,
                  Vector3.Distance(knee,hip));

            lowerRenderLeg.position = knee;
            lowerRenderLeg.forward = foot.center-knee;
            lowerRenderLeg.localScale =
               new Vector3(lowerRenderLeg.localScale.x,lowerRenderLeg.localScale.y,
                  Vector3.Distance(foot.center,knee));

            renderKnee.position = knee;
            renderKnee.forward = knee-hip;
         }
      }

      //Limb functions

      void BuildLimbs(){
         BuildLimb("LeftLowerArm",ref leftLowerArm);
         BuildLimb("LeftUpperArm",ref leftUpperArm);
         BuildLimb("RightLowerArm",ref rightLowerArm);
         BuildLimb("RightUpperArm",ref rightUpperArm);

         BuildLimb("LeftLowerLeg",ref leftLowerLeg);
         BuildLimb("LeftUpperLeg",ref leftUpperLeg);
         BuildLimb("RightLowerLeg",ref rightLowerLeg);
         BuildLimb("RightUpperLeg",ref rightUpperLeg);

         BuildLimb("LeftElbow",ref leftElbow,true);
         BuildLimb("RightElbow",ref rightElbow,true);
         BuildLimb("LeftKnee",ref leftKnee,true);
         BuildLimb("RightKnee",ref rightKnee,true);
      }
      void BuildLimb(string name, ref Renderer limb, bool seam = false, bool force = false){
         if(force && limb!=null)DestroyImmediate(limb);
         if(limb==null){
            Transform t = head.Find(name);
            if(t!=null)limb = t.GetComponent<Renderer>();
            if(limb==null){
               limb = Instantiate(
                  seam?seamPrefab:limbPrefab,Vector3.zero,Quaternion.identity
               ) as Renderer;
               limb.transform.parent = head;
               limb.gameObject.name = name;
            }
         }
      }

      void DisplayLimbs(){
         DisplayLimb(leftLowerArm,leftUpperArm,leftElbow,leftHand!=null);
         DisplayLimb(rightLowerArm,rightUpperArm,rightElbow,rightHand!=null);

         //Only display legs if the torso is active
         DisplayLimb(leftLowerLeg,leftUpperLeg,leftKnee,
            leftLeg!=null && torso.gameObject.activeSelf);
         DisplayLimb(rightLowerLeg,rightUpperLeg,rightKnee,
            rightLeg!=null && torso.gameObject.activeSelf);

         if(face!=null)face.material = skin;
      }
      void DisplayLimb(Renderer lower, Renderer upper, Renderer seam, bool toggle){
         lower.enabled = toggle; upper.enabled = toggle; seam.enabled = toggle;
         lower.material = skin;
         upper.material = seam.material = clothes;
      }

      //Joint functions

      void BuildJoints(){
         BuildJoint("LeftShoulder",ref leftShoulder);
         BuildJoint("RightShoulder",ref rightShoulder);
         BuildJoint("LeftHip",ref leftHip);
         BuildJoint("RightHip",ref rightHip);
      }
      void BuildJoint(string name, ref Transform joint){
         if(joint==null){
            joint = head.Find(name);
            if(joint==null){
               joint = new GameObject(name).transform;
               joint.parent = head;
            }
         }
      }

      void OrientJoints(){
         leftShoulder.position = JointPosition(true,true);
         rightShoulder.position = JointPosition(true,false);

         leftHip.position = JointPosition(false,true);
         rightHip.position = JointPosition(false,false);

         leftShoulder.rotation = rightShoulder.rotation =
         leftHip.rotation = rightHip.rotation = torso.rotation;
      }
      Vector3 JointPosition(bool shoulder, bool left){
         Transform activeTorso = torso.gameObject.activeSelf?torso:partialTorso;
         return neck.position +
            (shoulder?Vector3.zero:-activeTorso.up*TORSO_LENGTH_FACTOR*headLength) +
            activeTorso.right*(shoulder?SHOULDER_WIDTH_FACTOR:HIP_WIDTH_FACTOR) *
            (left?-headLength:headLength);
      }
      //Blink
      void Blink(){
         StartCoroutine(ToggleEyes(eyes.localScale));
         blinkDelay = Random.Range(BLINK_DELAY.x,BLINK_DELAY.y);
         lastBlink = Time.time;
      }
      IEnumerator ToggleEyes(Vector3 initialScale, bool close = true){
         float initialTime = Time.time;
         Vector3 shut = new Vector3(initialScale.x,0,initialScale.z);
         while(close?eyes.localScale.y>0:eyes.localScale.y<initialScale.y){
            eyes.localScale = Vector3.Lerp(
               close?initialScale:shut,close?shut:initialScale,
               (Time.time-initialTime)/BLINK_TIME
            );
            yield return null;
         }
         if(close)StartCoroutine(ToggleEyes(initialScale,false));
      }
   }
}