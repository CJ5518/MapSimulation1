using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Logger {
	public static string outputFilePath = null;
	private static StreamWriter outputFile = null;

	public static void Log(object obj) {
		if (Application.isBatchMode) {
			if (outputFilePath == null) {
				Debug.Log(obj);
			} else {
				if (outputFile == null) {
					outputFile = new StreamWriter(outputFilePath);
					Log(obj);
				} else {
					outputFile.WriteLine(obj.ToString());
				}
			}
		} else {
			Debug.Log(obj);
		}
	}

	public static void onExit() {
		if (outputFile != null) {
			outputFile.Flush();
		}
	}

	public static void LogWarning(object obj) {
		if (Application.isBatchMode) {
			Log(obj);
		} else {
			Debug.LogWarning(obj);
		}
	}
	public static void LogError(object obj) {
		if (Application.isBatchMode) {
			Log(obj);
		} else {
			Debug.LogError(obj);
		}
	}
}
