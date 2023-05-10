using UnityEngine;

public class Logger {
	public static void Log(object msg) {
		Debug.Log(msg);
	}
	public static void LogError(object msg) {
		Debug.LogError(msg);
	}
}