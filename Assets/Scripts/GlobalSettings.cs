using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//UNTESTED

public class GlobalSettings : MonoBehaviour {
	//The dictionary of the possible command line args
	public static Dictionary<string, object> args;
	private static int q;

	public static void initFromCommandLine(string[] argv) {
		for (q = 0; q < argv.Length; q++) {
			switch (argv[q]) {
				case "-maxSimTime":
				addArg<float>(argv, "maxSimTime", 1);
				break;
			}
		}
	}

	//EX: addArg("booleanFlagArg", 0);
	//EX: addArg("ArgWithSomeValues", 1, typepof(int));
	//-ArgWithSomeValues 17 -Other....
	//Automatically adjusts "q" the iterating integer
	private static void addArg<T>(string[] argv, string name, int numVals) {
		//args.Add(name, )
		if (numVals == 0) {
			args.Add(name, true);
		} else if (numVals == 1) {
			
		}
	}

	//Try to parse a string into some arbitrary type
	//ATTENTION: we have the wheere T : new just to avoid compile errors this function aint finished
	private static T tryParse<T>(string str) where T : new() {
		if (typeof(int) == typeof(T)) {
			//return (T)int.Parse(str);
		}
		return new T();
	}

	//Same as the above but only for flags, bool yes or nos
	private static void addFlag(string name) {
		args.Add(name, true);
	}
}