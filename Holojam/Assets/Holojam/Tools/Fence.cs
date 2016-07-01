//Fence.cs
//Created by Aaron C Gaudette on 24.06.16
//Builds holobounds visualizer

using UnityEngine;
using System.Collections.Generic;

namespace Holojam{
	[RequireComponent(typeof(Holobounds))]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class Fence : MonoBehaviour{
		public Material material;
		public ActorManager actorManager;
		
		public float minRange = 1.5f;
		public float maxAlpha = 0.6f;
		
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		int quadIndex = 0;
		
		void Update(){
			if(actorManager!=null && actorManager.buildActor!=null)
				material.color=new Color(
					material.color.r,material.color.g,material.color.b,
					maxAlpha*(1-(holobounds.Distance(actorManager.buildActor.transform.position)/minRange))
				);
			else material.color=new Color(
				material.color.r,material.color.g,material.color.b,
				maxAlpha
			);
		}
		
		Holobounds holobounds;
		Mesh mesh; Renderer r;
		void Start(){
			holobounds=GetComponent<Holobounds>();
			mesh=GetComponent<MeshFilter>().mesh;
			r=GetComponent<MeshRenderer>();
			GenerateMesh();
			ProcessMesh();
		}
		
		void GenerateMesh(){
			Vector3 c0 = holobounds.v0+Vector3.up*holobounds.ceiling;
			Vector3 c1 = holobounds.v1+Vector3.up*holobounds.ceiling;
			Vector3 c2 = holobounds.v2+Vector3.up*holobounds.ceiling;
			Vector3 c3 = holobounds.v3+Vector3.up*holobounds.ceiling;
			//Build walls
			Quad(holobounds.v0,c0,c1,holobounds.v1);
			Quad(holobounds.v1,c1,c2,holobounds.v2);
			Quad(holobounds.v2,c2,c3,holobounds.v3);
			Quad(holobounds.v3,c3,c0,holobounds.v0);
		}
		void Quad(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br){
			//Set vertices (duplicates added for flat shading)
			verts.Add(bl); verts.Add(tl); verts.Add(tr);
			verts.Add(tr); verts.Add(br); verts.Add(bl);
			//Build triangles
			for(int i=0;i<6;++i)tris.Add(quadIndex*6+i);
			//Update for next quad
			quadIndex++;
		}
		//Update mesh to engine
		void ProcessMesh(){
			mesh.Clear();
			mesh.vertices = verts.ToArray();
			mesh.triangles = tris.ToArray();
			mesh.Optimize();
			mesh.RecalculateNormals();
			r.material=material;
		}
	}
}