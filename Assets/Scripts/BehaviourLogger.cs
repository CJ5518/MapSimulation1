using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*
Document where things are done:


Record intervention times if applicable, including start and end times.
Log the state selected by the user, including time and duration of interaction.
Document any changes in data streams visible to the user on graphs.
Note changes to map overlay and data presented on the map.

*/


public static class BehaviourLogger {
	private static StreamWriter file;
	//Called by sim stats because this project is a mess
	public static void initLogger(string filename) {
		file = new StreamWriter(filename);
		logItem("StartLogging");
	}

	//Don't put a newline into message, function does it for you
	public static void logItem(string message) {
		System.DateTime time = System.DateTime.Now;

		string dateString = $"{time.Year}_{time.Month}_{time.Day}_{time.Hour}_{time.Minute}_{time.Second}_{time.Millisecond}: ";
		file.Write(dateString);
		file.Write(message);
		file.Write("\n");

	}

	//Also called in SimStats
	public static void endLogger() {
		file.Close();
	}
}
