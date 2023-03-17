using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableListOfStateMasks : MonoBehaviour {
    [SerializeField]
    public List<Texture2D> stateTextures;
    [SerializeField]
    public Texture2D noneTexture;
}
