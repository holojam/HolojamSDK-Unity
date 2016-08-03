//Holobounds.cs
//Created by Aaron C Gaudette on 22.06.16
//Playspace manager and access point

using UnityEngine;
using Holojam.Network;

namespace Holojam{
   [ExecuteInEditMode]
   public class Holobounds : MonoBehaviour{
      public Vector2[] bounds = new Vector2[4]; //Corners (FL, FR, BR, BL)
      public float floor = 0; //Floor Y
      public float ceiling = 3; //Ceiling Y -- not used for tracking

      public HolojamView calibrator; //Tracked tool for setting values

      //Reference values & functions

      public Vector3 center{get{
         return Floor(0.25f*(bounds[0]+bounds[1]+bounds[2]+bounds[3]));
      }}
      public float area{get{
         float a = 0;
         for(int i=0;i<4;++i){
            Vector2 p0 = bounds[i], p1 = bounds[(i+1)%4];
            //Add area under each segment (subtracting if underneath)
            a += (p1.x-p0.x) * (p1.y+p0.y) * 0.5f;
         }
         return Mathf.Abs(a); //Result may be negative depending on orientation
      }}
      public float xRatio{get{ //Ratio between side medians
         return Vector2.Distance(Side(3),Side(1)) / Vector2.Distance(Side(0),Side(2));
      }}

      //Bounding corner points
      public Vector3 Corner(int i){return Floor(bounds[i]);}
      public Vector3 Upper(int i){return Corner(i)+Vector3.up*ceiling;}

      //Distance from input point to edge of bounds
      public float Distance(Vector3 target){ 
         Vector2 t = new Vector2(target.x,target.z); //Do calculations in 2D

         float minDistance = -1;
         for(int i=0;i<4;++i){ //Iterate through corners
            //Compare distances from target to each side and return minimum
            float d = DistanceToSegment(bounds[i],bounds[(i+1)%4],t);
            if(d<minDistance || minDistance==-1)minDistance=d;
         }
         return minDistance;
      }

      //Calibrate a specific corner (or floor value)
      public void Calibrate(int i){
         if(calibrator==null){
            Debug.LogWarning("Holobounds: Calibrator not set");
            return;
         }
         Vector3 position = calibrator.RawPosition;
         if(i<4)bounds[i]=new Vector2(position.x,position.z);
         else if(i==4)floor=position.y;
         else ceiling=position.y;
      }
      //Draw for editor & debug
      void OnDrawGizmos(){
         Gizmos.color=new Color(1,0.5f,0); //Orange
         for(int i=0;i<4;++i){
            //Edges and corners
            Gizmos.DrawLine(Corner(i),Corner((i+1)%4));
            Gizmos.DrawLine(Corner(i),Upper(i));
         }
      }

      //Save and load calibration data
      static bool hasPlayed = false;
      void Start(){
         if(!Application.isPlaying && hasPlayed){
            for(int i=0;i<4;++i){
               bounds[i].x=PlayerPrefs.GetFloat("Holobounds_Corner"+i+"_x");
               bounds[i].y=PlayerPrefs.GetFloat("Holobounds_Corner"+i+"_y");
            }
         } else if(Application.isPlaying)hasPlayed=true;
      }
      void OnApplicationQuit(){Save();}
      void Save(){
         for(int i=0;i<4;++i){
            PlayerPrefs.SetFloat("Holobounds_Corner"+i+"_x",bounds[i].x);
            PlayerPrefs.SetFloat("Holobounds_Corner"+i+"_y",bounds[i].y);
         }
      }

      //Convert Vector2 to floored Vector3 (for ease of use)
      Vector3 Floor(Vector2 v){return new Vector3(v.x,floor,v.y);}
      //Median point of each side (F, R, B, L)
      Vector2 Side(int i){return 0.5f*(bounds[i++]+bounds[i%4]);}

      float DistanceToSegment(Vector2 p0, Vector2 p1, Vector2 target){
         //Length of segment squared
         float l2 = (p1.x-p0.x)*(p1.x-p0.x) + (p1.y-p0.y)*(p1.y-p0.y);
         if(l2==0)return Vector2.Distance(target,p0); //No segment, return distance to first point

         //Percentage of segment the projection of the target reaches
         float percent = Vector2.Dot(p1-p0,target-p0) / l2;
         //Return distance between target and projection
         return Vector2.Distance(target, p0 + (p1-p0)*percent);
      }
   }
}
