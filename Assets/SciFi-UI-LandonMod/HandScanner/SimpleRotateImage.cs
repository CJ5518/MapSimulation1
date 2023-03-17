using UnityEngine;

public class SimpleRotateImage : MonoBehaviour {

	RectTransform rect;
	public float speed = 1f;
	
    void Awake() {
		rect = GetComponent<RectTransform>();
    }
	
    void Update() {
		rect.Rotate(new Vector3(0, 0, speed * Time.deltaTime));
    }
}