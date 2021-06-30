//By Carson Rueber

//Possible todo:
//Might end up seeing repeated code as we do more implementations of this class
//So maybe put some more functions into here for doing certain things,
//But we'll get to that when we get to that

using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;

//Base class for handling the loading and handling of raster files
public abstract class RasterHandler : IDisposable {
	//The dataset
	public Dataset dataset = null;
	//Some info about the dataset
	public double datasetMin, datasetMax, datasetMean, datasetStdDev;

	//Extents to force the new rasters into
	//Automatically set to, in the future
	Vector2Double minExtents, maxExtents;

	//Has the data been processed and is ready to load into a texture?
	protected bool dataHasBeenProcessed;

	//Downloads missing data
	public abstract bool downloadData();

	//Preprocess the data
	public abstract bool preprocessData();

	//Load the raster data into a new resultant texture of width and height
	public abstract Texture2D loadToTexture(int width, int height);


	//Disposes of the dataset
	virtual public void Dispose() {
		if (dataset != null) {
			dataset.Dispose();
		}
	}

	//Convert from raster space to lat/longs and vice versa
	public static Vector2Double rasterSpaceToWorld(Vector2Double rasterPixel, Dataset dataset) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double(argout[0] + (argout[1] * rasterPixel.x), argout[3] + (argout[5] * rasterPixel.y));
	}
	public static Vector2Double worldToRasterSpace(Vector2Double coords, Dataset dataset) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double((coords.x - argout[0]) / argout[1], (coords.y - argout[3]) / argout[5]);
	}
}
