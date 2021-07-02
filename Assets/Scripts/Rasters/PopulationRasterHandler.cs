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

	//Default constructor
	public PopulationRasterHandler(PopulationRasterType populationType) {
		inputVrtFilename = populationTypeFilenameLookup[(int)populationType];
		outputTifFilename = Application.temporaryCachePath + "/Warped" + populationType.ToString() + ".tif";

	}



	//Preprocess the input data
	public override bool preprocessData() {
		//First check if the data has already been processed

		LuaFunction checkIfDatasetIsWarped = LuaSingleton.lua.GetFunction("RasterUtilities.checkIfDatasetIsWarped");
		bool needToWarp = !(bool)checkIfDatasetIsWarped.Call(outputTifFilename)[0];

		if (needToWarp) {
			LuaFunction warpVrt = LuaSingleton.lua.GetFunction("RasterUtilities.warpVrt");
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
		LuaFunction thisFunc = LuaSingleton.lua.GetFunction("RasterUtilities.loadRasterToTexture");
		return (Texture2D)thisFunc.Call(dataset, width, height)[0];
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