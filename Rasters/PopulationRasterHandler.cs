//By Carson Rueber

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;
using System.Xml;

//Population raster data handler
public class PopulationRasterHandler : RasterHandler {
	//File paths
	const string inputVrtFilename = @"F:\Data\tif\out.vrt";
	string outputVrtFilename = Application.temporaryCachePath + "/" + "WarpedPopulation.vrt";

	//The important band
	Band rasterBand;

	//ShapeFileRenderer, needed for some render-space transformations
	ShapeFileRenderer shapeFileRenderer;

	//Default constructor
	public PopulationRasterHandler() {}

	//Preprocess the input data
	private void preprocessData(ShapeFileRenderer shapeFileRenderer) {
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

				//Open it
				Dataset dataset = Gdal.Open(filename, Access.GA_ReadOnly);

				//Warp options

				//We can set the raster pixel size here
				double resolution = 1.0 / shapeFileRenderer.scalingFactor;
				string options = "-tr " + resolution + " " + resolution + " -r sum -wm 500 -overwrite -wo \"INIT_DEST=NO_DATA\"";

				//Take the options string and convert to GDALWarpAppOptions
				string[] optionsStrings = options
					.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				GDALWarpAppOptions warpOptions = new GDALWarpAppOptions(optionsStrings);

				//Output
				string outputFilename = Application.temporaryCachePath +
					"/Warped_" + Path.GetFileNameWithoutExtension(filename) + ".tif";
				outputFilenames.Add(outputFilename);

				//Warp drive
				Gdal.Warp(
					outputFilename,
					new Dataset[] { dataset }, warpOptions, null, null
				);
			}
		}

		//Build the vrt and set it as our dataset
		dataset = Gdal.wrapper_GDALBuildVRT_names(
			outputVrtFilename,
			outputFilenames.ToArray(),
			new GDALBuildVRTOptions(new string[] { "-overwrite" }),
			null, null
		);


		//The population data only has 1 band
		rasterBand = dataset.GetRasterBand(1);

		//Collect statistics while we're here
		rasterBand.GetStatistics(0, 1, out datasetMin, out datasetMax, out datasetMean, out datasetStdDev);
	}

	public override Texture2D loadToTexture(int width, int height, ShapeFileRenderer shapeFileRenderer) {
		//Process the data
		preprocessData(shapeFileRenderer);
		//Output texture
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

		//Gather info about the raster
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);

		//Calulcate the pixel size, in pixels
		int pixelSize = Screen.width / width;


		//The size of a raster pixel, scaled up to render space
		Vector2Double rasterProjectedPixelSize = new Vector2Double(argout[1], argout[5]) * shapeFileRenderer.scalingFactor;

		//The number of raster pixels that make up one of our texture pixels
		//Technically the sqrt of the actual figure, this is the number in a single row
		int rasterPixelsPerImagePixel = (int)Math.Floor((pixelSize / rasterProjectedPixelSize.x) + 0.5);

		//Raster data buffer
		double[] rasterData = new double[rasterPixelsPerImagePixel * rasterPixelsPerImagePixel];

		//For every pixel in the image
		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.height; y++) {
				//Default texture color
				texture.SetPixel(x, y, Color.clear);

				//Convert these coords to world coords
				Vector2 screenCoords = new Vector2(x * pixelSize, y * pixelSize);
				Vector2 projectedCoords = shapeFileRenderer.renderSpaceToProjection(screenCoords);
				Vector2Double worldCoords = Projection.projectionToLatLongs((Vector2Double)projectedCoords);

				//Get raster coords from the world coords
				Vector2Double rasterCoords = worldToRasterSpace(worldCoords, dataset);

				//If not in bounds, skip
				if (
					!(rasterCoords.x >= 0 &&
					rasterCoords.y >= 0 &&
					rasterCoords.x + rasterPixelsPerImagePixel < dataset.RasterXSize &&
					rasterCoords.y + rasterPixelsPerImagePixel < dataset.RasterYSize)
				) {
					continue;
				}

				//Read in the raster data
				rasterBand.ReadRaster(
					(int)rasterCoords.x, (int)rasterCoords.y,
					rasterPixelsPerImagePixel, rasterPixelsPerImagePixel,
					rasterData,
					rasterPixelsPerImagePixel, rasterPixelsPerImagePixel,
					0, 0
				);
				//Iterate over the raster data and count the number of people
				double numberOfPeople = 0.0;
				for (int q = 0; q < rasterData.Length; q++) {
					if (!double.IsNaN(rasterData[q])) {
						numberOfPeople += rasterData[q];
					}
				}

				//Output the color
				float val = (float)(numberOfPeople / (datasetMax * 1.0));

				if (numberOfPeople != 0.0) {
					val = Mathf.Sqrt(val);
					texture.SetPixel(x, y, new Color(val, val, val, 1.0f));
				}
			}
		}
		
		//We're finished
		texture.Apply();
		return texture;
	}



	//Count the number of people in the given dataset
	double countDataset(Dataset dataset) {
		double sum = 0.0;

		double[] data = new double[dataset.RasterXSize * dataset.RasterYSize];
		Band b = dataset.GetRasterBand(1);
		b.ReadRaster(
			0, 0,
			dataset.RasterXSize, dataset.RasterYSize,
			data, dataset.RasterXSize, dataset.RasterYSize, 0, 0
		);

		for (int q = 0; q < data.Length; q++) {
			if (double.IsNaN(data[q])) continue;
			sum += data[q];
		}


		return sum;
	}
}
