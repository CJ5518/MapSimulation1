//By Carson Rueber
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//Works with some data
//A bit of a neglected class that I have forgotten about
public class DataHandler {
	public static string dataPath = Application.streamingAssetsPath + "/Data";
	public static string aiportLocationFilepath = dataPath + "/Airports_Sorted.geojson";
	//See AirportData.cs
	public static string aiportMatrixFilepath = dataPath + "/AirportMatrix.txt";
	public static string aiportDistanceMatrixFilepath = dataPath + "/AirportDistanceMatrix.txt";
	//USA Shape file path
	public static string usaShapeFilepath = Application.streamingAssetsPath + "/USA_Reprojected3.shp";

	//Creates an array of airports the size of desiredAirportCount
	/*public static Simulation.Airport[] loadAirports(int desiredAirportCount) {

		DataSource dataSource = Ogr.Open(aiportLocationFilepath, 0);
		Layer layer = dataSource.GetLayerByIndex(0);

		int actualAirportCount = (int)layer.GetFeatureCount(1);
		if (desiredAirportCount > actualAirportCount) actualAirportCount = desiredAirportCount;

		layer.ResetReading();

		List<Simulation.Airport> airports = new List<Simulation.Airport>(desiredAirportCount);

		for (int q = 0; airports.Count < desiredAirportCount; q++) {
			Feature feature = layer.GetNextFeature();
			Geometry geometry = feature.GetGeometryRef();
			//argout[0] is longitude
			double[] argout = new double[2];
			geometry.GetPoint(0, argout);
			Simulation.Airport airport = new Simulation.Airport(argout[0], argout[1], feature.GetFieldAsString("Loc_Id"));
			//Index will be -1 if the airport is invalid
			if (airport.index >= 0)
				airports.Add(airport);
		}

		layer.Dispose();
		dataSource.Dispose();

		return airports.ToArray();
	}*/
}