using UnityEngine;

public class CursorManager : MonoBehaviour {
	public Texture2D cursor;
	public Texture2D crosshair;


	void Start() {
		SetCursor();
	}

	void SetCursor() {
		Vector2 offset = new Vector2(0f,0f);
		Cursor.SetCursor(cursor, offset, CursorMode.Auto);
	}
	void SetCrosshair() {
		Vector2 offset = new Vector2(crosshair.width / 2f, crosshair.height / 2f);
		Cursor.SetCursor(crosshair, offset, CursorMode.Auto);
	}
}