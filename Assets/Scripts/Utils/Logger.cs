using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Logger {
	public static void Log(object obj) {
		if (Application.isBatchMode) {
			System.Console.WriteLine(obj);
		} else {
			Debug.Log(obj);
		}
	}

	public static void LogWarning(object obj) {
		if (Application.isBatchMode) {
			System.Console.WriteLine(obj);
		} else {
			Debug.LogWarning(obj);
		}
	}
	public static void LogError(object obj) {
		if (Application.isBatchMode) {
			System.Console.WriteLine(obj);
		} else {
			Debug.LogError(obj);
		}
	}
}