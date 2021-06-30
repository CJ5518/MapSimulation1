//By Carson Rueber

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;
using System.Xml;
using NLua;

//Enum for the different population rasters
public enum PopulationRasterType {
	ChildrenUnderFive,
	ElderlySixtyPlus,
	Men,
	FullPopulation,
	Women,
	WomenOfReproductiveAge,
	Youth15To24,
	PopulationTypeCount
}

//Population raster data handler
public class PopulationRasterHandler : RasterHandler {
	//File paths
	string inputVrtFilename;
	string outputTifFilename;

	//Maps from the enum to the vrt files
	//Starts at a Data folder so you'll need to prepend the rest of the file path
	private string[] populationTypeFilenameLookup = {
		@"F:\Data\tif\ChildrenUnderFive\ChildrenUnderFive.vrt",
		@"F:\Data\tif\ElderlySixtyPlus\ElderlySixtyPlus.vrt",
		@"F:\Data\tif\Men\Men.vrt",
		@"F:\Data\tif\FullPopulation\population_usa_2019-07-01.vrt",
		@"F:\Data\tif\Women\Women.vrt",
		@"F:\Data\tif\WomenOfReproductiveAge\WomenOfReproductiveAge.vrt",
		@"F:\Data\tif\Youth15To24\Youth15To24.vrt"
	};

	//The important band
	Band rasterBand;

	Lua lua;

	//Default constructor
	public PopulationRasterHandler(PopulationRasterType populationType) {
		inputVrtFilename = populationTypeFilenameLookup[(int)populationType];
		outputTifFilename = Application.temporaryCachePath + "/Warped" + populationType.ToString() + ".tif";

		//Init Lua
		lua = new Lua();
		lua.LoadCLRPackage();
		lua.DoFile(Application.streamingAssetsPath + @"\Lua\RasterUtilities.lua");
	}



	//Preprocess the input data
	public override bool preprocessData() {
		//First check if the data has already been processed

		LuaFunction checkIfDatasetIsReady = lua.GetFunction("checkIfDatasetIsReady");
		bool needToWarp = !(bool)checkIfDatasetIsReady.Call(outputTifFilename)[0];

		if (needToWarp) {
			LuaFunction warpVrt = lua.GetFunction("warpVrt");
			warpVrt.Call(inputVrtFilename, outputTifFilename, "sum");
		}

		dataset = Gdal.Open(outputTifFilename, Access.GA_ReadOnly);

		//The population data only has 1 band
		rasterBand = dataset.GetRasterBand(1);

		//Collect statistics while we're here
		rasterBand.GetStatistics(0, 1, out datasetMin, out datasetMax, out datasetMean, out datasetStdDev);

		//If the above lines didn't error, things probably worked out
		//Isn't my error handling just perfect?
		dataHasBeenProcessed = true;

		//Success or failure
		return dataHasBeenProcessed;
	}

	public override bool downloadData() {
		throw new System.NotImplementedException();
	}

	public override Texture2D loadToTexture(int width, int height) {
		//Make sure data has been processed
		if (!dataHasBeenProcessed) {
			preprocessData();
		}

		//Output texture
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

		//Gather info about the raster
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);

		//Calulcate the pixel size, in pixels
		int pixelSize = Screen.width / width;

		//The number of raster pixels that make up one of our texture pixels
		//We set this to one because we BELIEVE that we warped it properly
		int rasterPixelsPerImagePixel = 1;
		//Raster data buffer
		double[] rasterData = new double[rasterPixelsPerImagePixel * rasterPixelsPerImagePixel];

		//Get the corner coords of the screen
		Vector2Double screenCoords = new Vector2Double(0, 0);
		Vector2Double projectedCoords = Projection.renderSpaceToProjection(screenCoords);
		Vector2Double worldCoords = Projection.projectionToLatLongs(projectedCoords);

		//For every pixel in the image
		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.height; y++) {
				//Default texture color
				texture.SetPixel(x, y, Simulation.floatToColor(0));

				//Get raster coords from the world coords
				Vector2Int rasterCoords = (Vector2Int)
					(worldToRasterSpace(worldCoords, dataset) + new Vector2Double(0.5,0.5));
				rasterCoords.x += x;
				rasterCoords.y -= y;

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
				Color32 color = Simulation.floatToColor((float)numberOfPeople);

				//Just to make sure the color is set just right
				texture.SetPixels32(x, y, 1, 1, new Color32[] { color });
			}
		}
		texture.Apply();
		return texture;
	}


	//Dispose method
	public override void Dispose() {
		base.Dispose();
		lua.Dispose();
	}

	//Count the number of people in the given dataset
	public double countDataset(Dataset dataset) {
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