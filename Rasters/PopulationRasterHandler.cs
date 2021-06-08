//By Carson Rueber

//TODO: Clean it up it's a mess

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;
using System.Xml;

//Population raster data
public class PopulationRasterHandler : RasterHandler {
	//File paths
	const string inputVrtFilename = @"F:\Data\tif\out.vrt";
	string outputVrtFilename = Application.temporaryCachePath + "/" + "WarpedPopulation.vrt";

	//The important band
	Band rasterBand;

	//ShapeFileRenderer
	//Should really split up some of the functionality from this function
	ShapeFileRenderer shapeFileRenderer;

	public PopulationRasterHandler() {}

	private void warpData(int width, int height, ShapeFileRenderer shapeFileRenderer) {
		double startTime = Time.realtimeSinceStartupAsDouble;

		int pixelSize = Screen.width / width;

		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(inputVrtFilename);

		List<string> outputFilenames = new List<string>();

		foreach (XmlNode node in xmlDocument.DocumentElement.SelectSingleNode("VRTRasterBand").ChildNodes) {
			if (node.Name == "ComplexSource") {
				//Filename of the tif
				string filename = Directory.GetParent(inputVrtFilename) + "/" + node.SelectSingleNode("SourceFilename").InnerText;

				Dataset dataset = Gdal.Open(filename, Access.GA_ReadOnly);

				//Compute the new size of the dataset

				int sizeX = dataset.RasterXSize;
				int sizeY = dataset.RasterYSize;

				double[] argout = new double[6];
				dataset.GetGeoTransform(argout);

				//Warp the dataset

				//Options
				double resolution = 1.0 / shapeFileRenderer.scalingFactor;
				string options = "-tr " + resolution + " " + resolution + " -r sum -wm 2000 -overwrite -wo \"INIT_DEST=NO_DATA\"";

				string[] optionsStrings = options
					.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				GDALWarpAppOptions warpOptions = new GDALWarpAppOptions(optionsStrings);

				//Output
				string outputFilename = Application.temporaryCachePath +
					"/Warped_" + Path.GetFileNameWithoutExtension(filename) + ".tif";
				outputFilenames.Add(outputFilename);

				//Warp drive
				dataset = Gdal.Warp(
					outputFilename,
					new Dataset[] { dataset }, warpOptions, null, null
				);

				Debug.Log(outputFilename + " " + countDataset(dataset));
			}
		}

		//Build the vrt and set it as our dataset
		dataset = Gdal.wrapper_GDALBuildVRT_names(
			outputVrtFilename,
			outputFilenames.ToArray(),
			new GDALBuildVRTOptions(new string[] { "-overwrite" }),
			null, null
		);


		Debug.Log("Warping and building took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds");


		//The population data only has 1 band
		rasterBand = dataset.GetRasterBand(1);

		//Collect statistics
		rasterBand.GetStatistics(0, 1, out datasetMin, out datasetMax, out datasetMean, out datasetStdDev);
	}

	double countDataset(Dataset dataset) {
		double sum = 0.0;

		double[] data = new double[dataset.RasterXSize * dataset.RasterYSize];
		Band b = dataset.GetRasterBand(1);
		b.ReadRaster(
			0,0,
			dataset.RasterXSize, dataset.RasterYSize,
			data, dataset.RasterXSize, dataset.RasterYSize, 0, 0
		);
		
		for (int q = 0; q < data.Length; q++) {
			if (double.IsNaN(data[q])) continue;
			sum += data[q];
		}


		return sum;
	}

	public override Texture2D loadToTexture(int width, int height, ShapeFileRenderer shapeFileRenderer) {
		warpData(width, height, shapeFileRenderer);
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
		Debug.Log(rasterPixelsPerImagePixel + " raster pixels per image pixel " + (pixelSize / rasterProjectedPixelSize.x));

		//Vector2Double topLeftCorner = rasterSpaceToWorld(dataset, Vector2Double.Zero);


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
				//Iterate over the raster data
				double numberOfPeople = 0.0;
				for (int q = 0; q < rasterData.Length; q++) {
					if (!double.IsNaN(rasterData[q])) {
						numberOfPeople += rasterData[q];
					}
				}
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
}
