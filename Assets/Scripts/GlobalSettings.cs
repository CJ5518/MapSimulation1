using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//UNTESTED

public class GlobalSettings : MonoBehaviour {
	//The dictionary of the possible command line args
	public static Dictionary<string, object> args;

	public static void initFromCommandLine(string[] argv) {
		for (int q = 0; q < argv.Length; q++) {
			Debug.Log(q + ": " + argv[q]);
		}
	}
}