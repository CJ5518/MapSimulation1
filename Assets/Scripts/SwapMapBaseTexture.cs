using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapMapBaseTexture : MonoBehaviour {

	public Texture sateliteTex;
	public Texture populationTex;
	public Texture highwayTex;
	
	public Renderer mesh;

	public void ChangeMaterial(int index){
		Debug.Log(index);
		if(index == 0){
			mesh.material.SetTexture("_BaseTex", sateliteTex);
		}
		if (index == 1) {
			mesh.material.SetTexture("_BaseTex", populationTex);
		}
		if(index == 2){
			mesh.material.SetTexture("_BaseTex", highwayTex);
		}
	}
}