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
	//Automatically set to
	Vector2Double minExtents, maxExtents;

	//Has the data been processed and is ready to load into a texture?
	protected bool dataHasBeenProcessed;

	//Preprocess the data
	public abstract bool preprocessData(int pixelSize);

	//Load the raster data into a new resultant texture of width and height
	//ShapeFileRenderer is needed because we do stuff in screen space
	public abstract Texture2D loadToTexture(int width, int height);

	virtual public void Dispose() {
		if (dataset != null) {
			dataset.Dispose();
		}
	}

	

	//Warps a vrt file by first warping all of the files listed in it
	//Returns the resultant dataset
	//Assumes all the tifs are in the same folder as the vrt
	//TODO: Change that, there is a thing in the xml that says whether or not the tif path is
	//relative to the vrt or not
	protected static Dataset warpVrt(string inputVrtFilename, string outputTifFilename, int pixelSize, string algorithm) {
		
		//Read the xml of the vrt
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(inputVrtFilename);

		//List of the resultant filenames for building the new vrt
		List<string> inputFilenames = new List<string>();

		//Iterate over all the band nodes
		foreach (XmlNode node in xmlDocument.DocumentElement.SelectSingleNode("VRTRasterBand").ChildNodes) {
			//If it is a tif
			if (node.Name == "ComplexSource") {
				//Filename of the tif
				string filename = Directory.GetParent(inputVrtFilename) + "/" + node.SelectSingleNode("SourceFilename").InnerText;

				inputFilenames.Add(filename);
			}
		}

		Dataset[] datasets = new Dataset[inputFilenames.Count];
		for (int q = 0; q < inputFilenames.Count; q++) {
			datasets[q] = Gdal.Open(inputFilenames[q], Access.GA_ReadOnly);
		}

		//Warp drive

		Vector2Double worldPixelSize = Projection.getPixelSizeInLatLong(pixelSize);

		GDALWarpAppOptions options = genWarpOptionsFromString(buildSuggestedWarpOptionsString(
			(worldPixelSize.x / 5).ToString(), (worldPixelSize.y / 5).ToString(), "sum"
		));

		//First warp
		Dataset intermediate = Gdal.Warp(
			Application.temporaryCachePath + "/temp.tif",
			datasets, options, null, null
		);

		options = genWarpOptionsFromString(buildSuggestedWarpOptionsString(
			worldPixelSize.x.ToString(), worldPixelSize.y.ToString(), "sum"
		));

		//Second warp
		Dataset ret = Gdal.Warp(outputTifFilename, new Dataset[] { intermediate }, options, null, null);

		intermediate.Dispose();

		return ret;
	}


	//Creates the suggested warp options string based on the suggested editable factors
	public static string buildSuggestedWarpOptionsString(string pixelSizeX, string pixelSizeY, string algorithm) {
		//Stolen from the women poplation data
		//It works so we'll keep it here
		Vector2Double extentMin = new Vector2Double(-152.5876388888889039, 24.4918631200275598);
		Vector2Double extentMax = new Vector2Double(-66.9981474405200288, 62.5851388888888920);

		string ret = "-tr " + pixelSizeX + " " + pixelSizeY + " -r " + algorithm +
			" -te " + extentMin.x.ToString() + " " + extentMin.y.ToString() +
			" " + extentMax.x.ToString() + " " + extentMax.y.ToString() +
			" -wm 500 -multi -overwrite -tap -et 0 -co \"TILED=YES\" -co \"COMPRESS=LZW\" -wo \"INIT_DEST=NO_DATA\"";

		return ret;
	}

	//Take an options string and converts it to GDALWarpAppOptions
	//string should be in the format "-tr 1 4 -override," as in how one
	//would pass these options on the command line
	public static GDALWarpAppOptions genWarpOptionsFromString(string options) {
		//Take the options string and convert to GDALWarpAppOptions
		string[] optionsStrings = options
			.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

		return new GDALWarpAppOptions(optionsStrings);
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
