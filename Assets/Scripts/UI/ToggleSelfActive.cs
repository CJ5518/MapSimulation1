using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSelfActive : MonoBehaviour {
	public void Toggle() {
		gameObject.SetActive(!gameObject.activeInHierarchy);
	}
}
