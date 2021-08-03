//By Carson Rueber
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using OSGeo.GDAL;
using OSGeo.OGR;

//Works with some data
public class DataHandler {
	public static string dataPath = Application.streamingAssetsPath + "/Data";
	public static string aiportLocationFilepath = dataPath + "/Airports_Sorted.geojson";
	//See AirportPassengerData.cs
	public static string aiportMatrixFilepath = dataPath + "/AirportMatrix.txt";

	//Creates an array of airports the size of desiredAirportCount
	public static Simulation.Airport[] loadAirports(int desiredAirportCount) {

		DataSource dataSource = Ogr.Open(aiportLocationFilepath, 0);
		Layer layer = dataSource.GetLayerByIndex(0);

		int actualAirportCount = (int)layer.GetFeatureCount(1);
		if (desiredAirportCount > actualAirportCount) actualAirportCount = desiredAirportCount;

		layer.ResetReading();

		Simulation.Airport[] airports = new Simulation.Airport[desiredAirportCount];
		
		for (int q = 0; q < desiredAirportCount; q++) {
			Feature feature = layer.GetNextFeature();
			Geometry geometry = feature.GetGeometryRef();
			//argout[0] is longitude
			double[] argout = new double[2];
			geometry.GetPoint(0, argout);
			Simulation.Airport airport = new Simulation.Airport(argout[0], argout[1], feature.GetFieldAsString("Loc_Id"));
			airports[q] = airport;
		}

		layer.Dispose();
		dataSource.Dispose();

		return airports;
	}
}