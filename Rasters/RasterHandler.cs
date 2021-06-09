//By Carson Rueber

//Possible todo:
//Might end up seeing repeated code as we do more implementations of this class
//So maybe put some more functions into here for doing certain things,
//But we'll get to that when we get to that

using System;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;

//Base class for handling the loading and handling of raster files
public abstract class RasterHandler : IDisposable {
	//The dataset
	public Dataset dataset = null;
	//Some info about the dataset
	public double datasetMin, datasetMax, datasetMean, datasetStdDev;

	//Has the data been processed and is ready to load into a texture?
	protected bool dataHasBeenProcessed;


	//Load the raster data into a new resultant texture of width and height
	//ShapeFileRenderer is needed because we do stuff in screen space
	public abstract Texture2D loadToTexture(int width, int height, ShapeFileRenderer shapeFileRenderer);

	//Preprocess the data
	public abstract bool preprocessData(int pixelSize, ShapeFileRenderer shapeFileRenderer);

	virtual public void Dispose() {
		if (dataset != null) {
			dataset.Dispose();
		}
	}
	//Take an options string and converts it to GDALWarpAppOptions
	//string should be in the format "-tr 1 4 -override," as in how one 
	//would pass these options on the command line
	protected GDALWarpAppOptions genWarpOptionsFromString(string options) {
		//Take the options string and convert to GDALWarpAppOptions
		string[] optionsStrings = options
			.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

		return new GDALWarpAppOptions(optionsStrings);
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
