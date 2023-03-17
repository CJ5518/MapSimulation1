using UnityEngine;

public class CursorHelper {
	public static bool cursorIsDifferent;
	public static Texture2D clickyCursor = Resources.Load("Cursors/ClickyCursor") as Texture2D;
	public static void setClickyCursor() {
		if (!cursorIsDifferent) {
			Cursor.SetCursor(clickyCursor, new Vector2(27, 0), CursorMode.Auto);
			cursorIsDifferent = true;
		}
	}

	public static void resetCursor() {
		if (cursorIsDifferent) {
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			cursorIsDifferent = false;
		}
	}
/*
	//Turned out to not be necessary, but it's still cool
	static CursorHelper() {

	
	}
*/ 
}