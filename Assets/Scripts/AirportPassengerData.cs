using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AirportPassengerData {
	private static string dataPath = Application.streamingAssetsPath + "/Data";
	static string aiportMatrixFilepath = dataPath + "/AirportMatrix.txt";
	private int[,] data;
	private Dictionary<string, int> lookupTable;

	public AirportPassengerData() {
		init();
	}

	public int getValue(string origin, string dest) {
		int originInt = lookupTable[origin];
		int destInt = lookupTable[dest];
		return data[originInt, destInt];
	}

	public void init() {
		TextReader textReader = File.OpenText(aiportMatrixFilepath);
		string[] firstLineBits = textReader.ReadLine().Split(' ');

		data = new int[firstLineBits.Length - 1, firstLineBits.Length - 1];
		lookupTable = new Dictionary<string, int>(firstLineBits.Length - 1);

		//Set the 3 letter code to point to an index from 0 - whatever
		for (int q = 1; q < firstLineBits.Length; q++) {
			lookupTable[firstLineBits[q]] = q - 1;
		}


		while (true) {
			string line = textReader.ReadLine();
			if (line == null) break;
			string[] lineBits = line.Split(' ');
			string originCode = lineBits[0];
			int originInt = lookupTable[originCode];
			for (int q = 1; q < lineBits.Length; q++) {
				string destCode = firstLineBits[q];
				int value = int.Parse(lineBits[q]);
				
				data[originInt, lookupTable[destCode]] = value;
			}
		}
	}

}