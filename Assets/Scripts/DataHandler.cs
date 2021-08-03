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

	public static void loadAirportPassengerData() {
		AirportPassengerData x = new AirportPassengerData();
		Debug.Log(x.getValue("ATL", "JFK"));
	}
}