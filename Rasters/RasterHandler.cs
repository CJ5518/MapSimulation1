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
	protected Dataset warpVrt(string inputVrtFilename, string outputVrtFilename, int pixelSize, string algorithm) {
		//Read the xml of the vrt
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(inputVrtFilename);

		//List of the resultant filenames for building the new vrt
		List<string> outputFilenames = new List<string>();

		//Iterate over all the band nodes
		foreach (XmlNode node in xmlDocument.DocumentElement.SelectSingleNode("VRTRasterBand").ChildNodes) {
			//If it is a tif
			if (node.Name == "ComplexSource") {
				//Filename of the tif
				string filename = Directory.GetParent(inputVrtFilename) + "/" + node.SelectSingleNode("SourceFilename").InnerText;

				Vector2Double worldPixelSize = Projection.getPixelSizeInLatLong(pixelSize);

				//Set the size of the pixels to diff, the size of a screen pixel
				string options = buildSuggestedWarpOptionsString(
					worldPixelSize.x.ToString(), worldPixelSize.y.ToString(),
					algorithm
				);

				GDALWarpAppOptions warpOptions = genWarpOptionsFromString(options);

				//Output
				string outputFilename = Application.temporaryCachePath +
					"/Warped_" + Path.GetFileNameWithoutExtension(filename) + ".tif";

				try {
					//Warp drive
					Dataset ds = Gdal.Warp(
						outputFilename,
						new Dataset[] { Gdal.Open(filename, Access.GA_ReadOnly) }, warpOptions, null, null
					);
					ds.Dispose();
					//Add it to the vrt list
					outputFilenames.Add(outputFilename);
				}
				catch (System.Exception error) {
					Debug.Log("An error occured in Gdal.Warp: " + error.Message);
				}
			}
		}
		//Build the vrt and set it as our dataset
		return Gdal.wrapper_GDALBuildVRT_names(
			outputVrtFilename,
			outputFilenames.ToArray(),
			new GDALBuildVRTOptions(new string[] { "-overwrite" }),
			null, null
		);
	}


	//public static Dataset

	//Creates the suggested warp options string based on the suggested editable factors
	public static string buildSuggestedWarpOptionsString(string pixelSizeX, string pixelSizeY, string algorithm) {
		return "-tr " + pixelSizeX + " " + pixelSizeY + " -r " + algorithm + 
			" -wm 500 -overwrite -wo \"INIT_DEST=NO_DATA\"";
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
