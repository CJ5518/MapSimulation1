using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Logger : MonoBehaviour {
	public static void Log(object obj) {
		if (Application.isBatchMode) {
			System.Console.WriteLine(obj);
		} else {
			Logger.Log(obj);
		}
	}

	public static void LogWarning(object obj) {
		if (Application.isBatchMode) {
			System.Console.WriteLine(obj);
		} else {
			Logger.LogWarning(obj);
		}
	}
	public static void LogError(object obj) {
		if (Application.isBatchMode) {
			System.Console.WriteLine(obj);
		} else {
			Logger.LogError(obj);
		}
	}
}