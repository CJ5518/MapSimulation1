using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NDesk.Options;


//UNTESTED

public class GlobalSettings {
	public static void initFromCommandLine(string[] argv) {
		for (int q = 0; q < argv.Length; q++) {
			Debug.Log(q + ": " + argv[q]);
		}
		string data = null;
		bool help   = false;
		int verbose = 0;
	/*	var p = new OptionSet () {
			{ "t|maxSimTime=",      v => data = v },
			{ "v|verbose",  v => { ++verbose; } },
			{ "h|?|help",   v => help = v != null },
		};
		List<string> extra = p.Parse (argv);*/
	}
}