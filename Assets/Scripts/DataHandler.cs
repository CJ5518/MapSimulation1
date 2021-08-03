//By Carson Rueber
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Works with some data
public class DataHandler {
	static string dataPath = Application.streamingAssetsPath + "/Data";
	static string aiportLocationFilepath = dataPath + "/Airport_Sorted.geojson";
	static string aiportMatrixFilepath = dataPath + "/AirportMatrix.txt";

	public static void loadAirportData() {
		var data = new Dictionary<string, Dictionary<string, int>>();
		TextReader textReader = File.OpenText(aiportMatrixFilepath);
		string[] firstLineBits = textReader.ReadLine().Split(' ');

		while (true) {
			string line = textReader.ReadLine();
			if (line == null) break;
			string[] lineBits = line.Split(' ');
			string originCode = lineBits[0];
			for (int q = 1; q < lineBits.Length; q++) {
				string destCode = firstLineBits[q];
				int value = int.Parse(lineBits[q]);

				if (q == 1) {
					data.Add(originCode, new Dictionary<string, int>());
				}
				data[originCode][destCode] = value;
			}
		}
		Debug.Log(data["ATL"]["LAX"]);
	}
}