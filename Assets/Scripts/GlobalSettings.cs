using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Options;


//UNTESTED

public class GlobalSettings {
	// -t or -stopTime
	static float stopTime = 200.0f;
	// -o or -outputPath
	static string outputPath = "./";


	public static void initFromCommandLine(string[] argv) {
		for (int q = 0; q < argv.Length; q++) {
			Logger.Log(q + ": " + argv[q]);
		}

		OptionSet p = new OptionSet ()
			.Add ("stop=|stopTime=", v => float.TryParse(v, out stopTime))
			.Add ("o=|outputPath=", v => outputPath = v);
	}
}