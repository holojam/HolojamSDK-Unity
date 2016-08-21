//Fence.cs
//Created by Aaron C Gaudette on 24.06.16
//Procedural holobounds visualization for custom worlds

using UnityEngine;
using System.Collections.Generic;

namespace Holojam.Tools{
   [RequireComponent(typeof(Holobounds))]
   [RequireComponent(typeof(MeshFilter))]
   [RequireComponent(typeof(MeshRenderer))]
   public class Fence : MonoBehaviour{
      public Material material;
      public ActorManager actorManager;

      public float minRange = 1.5f; //Distance to fade
      public float maxAlpha = 1;

      //Mesh data
      List<Vector3> verts = new List<Vector3>();
      List<int> tris = new List<int>();
      int quadIndex = 0;
      List<Vector2> uvs = new List<Vector2>();

      void Update(){
         Color newColor = material.color;

         if(actorManager!=null && actorManager.buildActor!=null){
            //Modulate transparency
            newColor.a = maxAlpha * (1-holobounds.Distance(actorManager.buildActor.eyes)/minRange);
         }
         else newColor.a=maxAlpha;

         //Update material
         material.color=newColor;
      }

      Holobounds holobounds;
      Mesh mesh; Renderer r;
      void Start(){
         holobounds=GetComponent<Holobounds>();
         mesh=GetComponent<MeshFilter>().mesh;
         r=GetComponent<MeshRenderer>();

         //Build mesh
         GenerateMesh();
         ProcessMesh();
      }

      void GenerateMesh(){
         Quad(holobounds.Corner(0),holobounds.Upper(0),holobounds.Upper(1),holobounds.Corner(1));
         Quad(holobounds.Corner(1),holobounds.Upper(1),holobounds.Upper(2),holobounds.Corner(2));
         Quad(holobounds.Corner(2),holobounds.Upper(2),holobounds.Upper(3),holobounds.Corner(3));
         Quad(holobounds.Corner(3),holobounds.Upper(3),holobounds.Upper(0),holobounds.Corner(0));
      }
      void Quad(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br){
         //Set vertices (negate local offset)
         verts.Add(tl-transform.position);
         verts.Add(tr-transform.position);
         verts.Add(br-transform.position);
         verts.Add(bl-transform.position);

         //Build triangles
         int prefix = quadIndex*4;
         tris.Add(prefix+0); tris.Add(prefix+1); tris.Add(prefix+3);
         tris.Add(prefix+3); tris.Add(prefix+1); tris.Add(prefix+2);

         //Update for next quad
         quadIndex++;

         //Texture
         float xScale = Vector3.Distance(tl,tr);
         float yScale = holobounds.ceiling-holobounds.floor;

         uvs.Add(new Vector2(0,yScale));
         uvs.Add(new Vector2(xScale,yScale));
         uvs.Add(new Vector2(xScale,0));
         uvs.Add(new Vector2(0,0));
      }
      //Update mesh to engine
      void ProcessMesh(){
         mesh.Clear();

         mesh.vertices = verts.ToArray();
         mesh.triangles = tris.ToArray();
         mesh.uv = uvs.ToArray();

         mesh.Optimize();
         mesh.RecalculateNormals();

         r.material=material;
      }
   }
}
