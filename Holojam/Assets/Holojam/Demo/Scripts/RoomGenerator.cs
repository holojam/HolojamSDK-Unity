//RoomGenerator.cs
//Created by Aaron C Gaudette on 23.06.16

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoomGenerator : MonoBehaviour{
	public Holojam.Holobounds holobounds;
	
	public Material floorMaterial, ceilingMaterial;
	public Material[] wallMaterials = new Material[4];
	
	List<Vector3> verts = new List<Vector3>();
	List<int> tris = new List<int>();
	int quadIndex = 0;
	
	Mesh mesh; Renderer r;
	void Start(){
		mesh=GetComponent<MeshFilter>().mesh;
		r=GetComponent<MeshRenderer>();
		if(holobounds==null){
			Debug.LogWarning("RoomGenerator: Holobounds reference is null!");
			return;
		}
		GenerateMesh();
		ProcessMesh();
	}
	
	void GenerateMesh(){
		//Floor
		Quad(holobounds.v0,holobounds.v1,holobounds.v2,holobounds.v3);
		//Ceiling
		Vector3 c0 = holobounds.v0+Vector3.up*holobounds.ceiling;
		Vector3 c1 = holobounds.v1+Vector3.up*holobounds.ceiling;
		Vector3 c2 = holobounds.v2+Vector3.up*holobounds.ceiling;
		Vector3 c3 = holobounds.v3+Vector3.up*holobounds.ceiling;
		Quad(c3,c2,c1,c0);
		//Walls
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
		mesh.subMeshCount=6;
		for(int i=0;i<6;++i)mesh.SetTriangles(tris.GetRange(6*i,6),i);
		mesh.Optimize();
		mesh.RecalculateNormals();
		
		Material[] mats = new Material[6];
		mats[0]=floorMaterial;
		mats[1]=ceilingMaterial;
		for(int i=0;i<4;++i)mats[2+i]=wallMaterials[i];
		r.materials=mats;
	}
}