using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Options;
using System.IO;

public class GlobalSettings {
	// -t or -stopTime
	public static float stopTime = 500.0f;
	// -o or -outputPath
	public static string outputPath = "./";
	public static string outputFilePostfix = "";
	// -nowrite
	public static bool writeOutputFiles = true;

	// -deterministic
	public static bool useDeterministic = false;
	// -airports
	public static bool useAirports = false;
	// -gravity
	public static bool useGravityModel = false;
	public static float[] gravityModelParams;

	//Simulation params, values of -1 indicate the user did not specify
	//If this is null, use ALL defaults
	public static float[] setupParams = null;

	public static string airportStartAt = "ATL";

	public static int ticksPerStatsUpdate = 1;

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
			.Add ("simlog=", v => Logger.outputFilePath = v)
			.Add ("opost=|outputFilePostfix=", v => outputFilePostfix = v)
			.Add ("deterministic", v => useDeterministic = true)
			.Add ("airports", v => useAirports = true)
			.Add ("ticksPerStats=", v => int.TryParse(v, out ticksPerStatsUpdate))
			.Add ("gravity=", v => {
				gravityModelParams = new float[2];
				string[] cmdParams = v.Split(',');
				gravityModelParams[0] = float.Parse(cmdParams[0]);
				gravityModelParams[1] = float.Parse(cmdParams[1]);
			})
			.Add ("params=|p=", v => {
				//7 parameters
				setupParams = new float[7];
				for(int q = 0; q < setupParams.Length; q++) {
					//default is -1
					setupParams[q] = -1.0f;
				}
				string[] paramDefs = v.Split(',');
				for (int q = 0; q < paramDefs.Length; q++) {
					string[] paramDeats = paramDefs[q].Split('=');
					setupParams[paramNameToIdx[paramDeats[0]]] = float.Parse(paramDeats[1]);
				}
			})
			.Add("startat=", v => {
				airportStartAt = v;
			})
			.Add ("h:|help:", (string v) => {printHelpMessage(v); quitApplication = true;});
		p.Parse(argv);

		Logger.Log(stopTime);
		Logger.Log(writeOutputFiles);
	}

	//Checks for the exit conditions of the simulation
	//If anything is amiss it will set quitApplication to true
	public static void Update() {
		if (Application.isBatchMode) {
			if (SimulationManager.simulation.dtSimulated >= stopTime) {
				quitApplication = true;
			}
		}
	}

	private static Dictionary<string, string> helpTextDictionary;
	private static Dictionary<string, int> paramNameToIdx;

	static GlobalSettings() {
		//Init help text
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

		//Init the parameter dictionary
		paramNameToIdx = new Dictionary<string, int>();
		paramNameToIdx.Add("id", 0);
		paramNameToIdx.Add("ir", 1);
		paramNameToIdx.Add("rs", 2);
		paramNameToIdx.Add("sv", 3);
		paramNameToIdx.Add("vs", 4);
		paramNameToIdx.Add("ei", 5);
		paramNameToIdx.Add("se", 6);
	}

	private static void printHelpMessage(string arg) {
		if (arg == null) {
			arg = "default";
		}
		Logger.Log(helpTextDictionary[arg]);
	}
}