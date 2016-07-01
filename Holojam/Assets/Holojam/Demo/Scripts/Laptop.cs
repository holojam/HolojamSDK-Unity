//Laptop.cs
//Created by Aaron C Gaudette on 23.06.16
//A simple script for rendering the dev computer

using UnityEngine;

[ExecuteInEditMode]
public class Laptop : MonoBehaviour{
	void Start(){
		if(!Application.isPlaying){
			transform.position=new Vector3(
				PlayerPrefs.GetFloat("Laptop_x0"),
				PlayerPrefs.GetFloat("Laptop_y0"),
				PlayerPrefs.GetFloat("Laptop_z0")
			);
			transform.rotation=new Quaternion(
				PlayerPrefs.GetFloat("Laptop_x1"),
				PlayerPrefs.GetFloat("Laptop_y1"),
				PlayerPrefs.GetFloat("Laptop_z1"),
				PlayerPrefs.GetFloat("Laptop_w1")
			);
		}
	}
	void OnApplicationQuit(){
		PlayerPrefs.SetFloat("Laptop_x0",transform.position.x);
		PlayerPrefs.SetFloat("Laptop_y0",transform.position.y);
		PlayerPrefs.SetFloat("Laptop_z0",transform.position.z);
		
		PlayerPrefs.SetFloat("Laptop_x1",transform.rotation.x);
		PlayerPrefs.SetFloat("Laptop_y1",transform.rotation.y);
		PlayerPrefs.SetFloat("Laptop_z1",transform.rotation.z);
		PlayerPrefs.SetFloat("Laptop_w1",transform.rotation.w);
	}
	void OnDrawGizmos(){
		Gizmos.color=new Color(0.8f,0.2f,0.1f);
		
		Vector3 c = transform.position, f = transform.forward, u = transform.up, l = -transform.right;
		float length = 0.32f, height = 0.225f, inset = 0.21f;
		Vector3[] points = new Vector3[6];
		points[0]=c+l*inset; points[1]=c-l*(length-inset);
		points[2]=points[1]-u*height; points[3]=points[2]-f*height;
		points[4]=points[3]+l*length; points[5]=points[4]+f*height;
		
		//Draw
		for(int i=0;i<6;++i)Gizmos.DrawLine(points[i],points[(i+1)%6]);
		Gizmos.DrawLine(points[2],points[5]); //Cross
		
	}
}