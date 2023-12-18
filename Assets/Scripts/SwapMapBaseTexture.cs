using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapMapBaseTexture : MonoBehaviour {

	public Texture sateliteTex;
	public Texture populationTex;
	public Texture highwayTex;
	
	public Renderer mesh;

	public void ChangeMaterial(int index){
		Logger.Log(index);
		if(index == 0){
			mesh.material.SetTexture("_BaseTex", sateliteTex);
			BehaviourLogger.logItem("MapTextureSetToSatellite");
		}
		if (index == 1) {
			mesh.material.SetTexture("_BaseTex", populationTex);
			BehaviourLogger.logItem("MapTextureSetToPopulation");
		}
		if(index == 2){
			mesh.material.SetTexture("_BaseTex", highwayTex);
			BehaviourLogger.logItem("MapTextureSetToHighways");
		}
	}
}