//By Carson Rueber

//Possible todo:
//Might end up seeing repeated code as we do more implementations of this class
//So maybe put some more funcitons into here for doing certain things,
//But we'll get to that when we get to that

using System;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;

//Base class for handling the loading and handling of raster files
public abstract class RasterHandler : IDisposable {
	//The dataset
	public Dataset dataset;
	//Some info about the dataset
	public double datasetMin, datasetMax, datasetMean, datasetStdDev;


	//Load the raster data into a new resultant texture of width and height
	//ShapeFileRenderer is needed because we do stuff in screen space
	public abstract Texture2D loadToTexture(int width, int height, ShapeFileRenderer shapeFileRenderer);

	virtual public void Dispose() {
		dataset.Dispose();
	}

	//Convert from raster space to lat/longs and vice versa
	public Vector2Double rasterSpaceToWorld(Vector2Double rasterPixel, Dataset dataset) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double(argout[0] + (argout[1] * rasterPixel.x), argout[3] + (argout[5] * rasterPixel.y));
	}
	public Vector2Double worldToRasterSpace(Vector2Double coords, Dataset dataset) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double((coords.x - argout[0]) / argout[1], (coords.y - argout[3]) / argout[5]);
	}
}
