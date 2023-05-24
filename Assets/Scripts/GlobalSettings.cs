using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Options;
using System.IO;

public class GlobalSettings {
	// -t or -stopTime
	public static float stopTime = 200.0f;
	// -o or -outputPath
	public static string outputPath = "./";
	public static string outputFilePostfix = "";
	// -nowrite
	public static bool writeOutputFiles = true;

	// -deterministic
	public static bool useDeterministic = false;
	// -airports
	public static bool useAirports = false;

	//Not set by any specific arg
	public static bool quitApplication = false;


	public static void initFromCommandLine(string[] argv) {
		for (int q = 0; q < argv.Length; q++) {
			Logger.Log(q + ": " + argv[q]);
		}

		OptionSet p = new OptionSet ()
			.Add ("stop=|stopTime=", v => float.TryParse(v, out stopTime))
			.Add ("o=|outputPath=", v => {outputPath = v; writeOutputFiles = true;})
			.Add ("nowrite", v => writeOutputFiles = v == null)
			.Add ("opost=|outputFilePostfix=", v => outputFilePostfix = v)
			.Add ("deterministic", v => useDeterministic = true)
			.Add ("airports", v => useAirports = true)
			.Add ("h:|help:", (string v) => {printHelpMessage(v); quitApplication = true;});
		p.Parse(argv);

		Logger.Log(stopTime);
		Logger.Log(writeOutputFiles);
	}

	//Checks for the exit conditions of the simulation
	//If anything is amiss it will set quitApplication to true
	public static void Update() {
		if (SimulationManager.simulation.dtSimulated >= stopTime) {
			quitApplication = true;
		}
	}

	private static Dictionary<string, string> helpTextDictionary;

	static GlobalSettings() {
		helpTextDictionary = new Dictionary<string, string>();
		string line;
		//Are we currently in the lines of a help text section?
		bool inMessage = false;
		string[] helpSectionTitles = new string[1];
		string message = "";
		//Load in the help text file
		using (StreamReader streamReader = new StreamReader(Application.streamingAssetsPath + "/helpText.txt")) {
			while ((line = streamReader.ReadLine()) != null) {
				if (inMessage) {
					if (line == "%%end%%") {
						for (int q = 0; q < helpSectionTitles.Length; q++) {
							helpTextDictionary.Add(helpSectionTitles[q], message);
						}

						inMessage = false;
					} else {
						message += line + "\n";
					}
				} else  {
					if (line.Contains(":")) {
						helpSectionTitles = line.Split(',');
						//The last string will contain the :, gotta get rid of that
						string last = helpSectionTitles[helpSectionTitles.Length-1];
						helpSectionTitles[helpSectionTitles.Length - 1] = last.Substring(0,last.Length-1);

						inMessage = true;
					}
				}
			}
		}
	}

	private static void printHelpMessage(string arg) {
		if (arg == null) {
			arg = "default";
		}
		Logger.Log(helpTextDictionary[arg]);
	}
}