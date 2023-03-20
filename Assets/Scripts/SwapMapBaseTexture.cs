using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapMapBaseTexture : MonoBehaviour {

    public Texture sateliteTex;
    public Texture populationTex;
    public Texture highwayTex;
    
    public Renderer mesh;

    public void ChangeMaterial(int index){
        if(index < 1){
            mesh.material.SetTexture("_BaseTex", sateliteTex);
        }
        if(index > 1){
            mesh.material.SetTexture("_BaseTex", highwayTex);
        }
        else {
            mesh.material.SetTexture("_BaseTex", populationTex);
        }
    }
}