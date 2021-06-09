//By Carson Rueber

//So right now, pixel size has to be less than 10 or there is a noticebale scar
//in the data because the rasters become too small
//Possible solution is to combine the tiny rasters beforehand
//There is also a scar at pixelSize=8

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;
using System.Xml;

//Population raster data handler
public class PopulationRasterHandler : RasterHandler {
	//File paths
	string inputVrtFilename;
	string outputVrtFilename;

	//Which population to use
	public enum PopulationType {
		ChildrenUnderFive,
		ElderlySixtyPlus,
		Men,
		FullPopulation,
		Women,
		WomenOfReproductiveAge,
		Youth15To24,
		PopulationTypeCount
	}
	//Maps from the enum to the vrt files
	//These should be changed to something within Unity when we do a release
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
	public PopulationRasterHandler(PopulationType populationType) {
		inputVrtFilename = populationTypeFilenameLookup[(int)populationType];
		outputVrtFilename = Application.temporaryCachePath + "/Warped" + populationType.ToString() + ".vrt";
	}

	//Preprocess the input data
	public override bool preprocessData(int pixelSize, ShapeFileRenderer shapeFileRenderer) {
		//Innocent until proven guilty
		dataHasBeenProcessed = true;

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

				//Open the tif
				Dataset dataset = Gdal.Open(filename, Access.GA_ReadOnly);

				//Get pixel size in lat long
				Vector2 corner = new Vector2(0, 0) * pixelSize;
				Vector2Double projectedCornerCoords = shapeFileRenderer.renderSpaceToProjection(corner);
				Vector2Double worldCornerCoords = Projection.projectionToLatLongs(projectedCornerCoords);

				Vector2 other = new Vector2(1, 0) * pixelSize;
				Vector2Double projectedOtherCoords = shapeFileRenderer.renderSpaceToProjection(other);
				Vector2Double worldOtherCoords = Projection.projectionToLatLongs(projectedOtherCoords);

				//Diff is now the size of a screen pixel in lat longs
				double diff = System.Math.Abs(worldCornerCoords.x - worldOtherCoords.x);

				//Set warp options

				//Set the size of the pixels to diff, the size of a screen pixel
				string options = "-tr " + diff + " " + diff + " -r sum -wm 500 -overwrite -wo \"INIT_DEST=NO_DATA\"";

				GDALWarpAppOptions warpOptions = genWarpOptionsFromString(options);

				//Output
				string outputFilename = Application.temporaryCachePath +
					"/Warped_" + Path.GetFileNameWithoutExtension(filename) + ".tif";

				try {
					//Warp drive
					Gdal.Warp(
						outputFilename,
						new Dataset[] { dataset }, warpOptions, null, null
					);
					//Add it to the vrt list
					outputFilenames.Add(outputFilename);
				}
				catch (System.Exception error) {
					Debug.Log("An error occured in Gdal.Warp: " + error.Message);
				}
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

		//Success or failure
		return dataHasBeenProcessed;
	}

	public override Texture2D loadToTexture(int width, int height, ShapeFileRenderer shapeFileRenderer) {
		//Make sure data has been processed
		if (!dataHasBeenProcessed) {
			preprocessData(Screen.height / height, shapeFileRenderer);
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

		//For every pixel in the image
		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.height; y++) {
				//Default texture color
				texture.SetPixel(x, y, Color.Lerp(Color.clear, Color.black, 0.1f));

				//Convert these coords to world coords
				Vector2Double screenCoords = new Vector2Double(x * pixelSize, y * pixelSize);
				Vector2Double projectedCoords = shapeFileRenderer.renderSpaceToProjection(screenCoords);
				Vector2Double worldCoords = Projection.projectionToLatLongs(projectedCoords);

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

				if (numberOfPeople != 0.0) {
					double val = (numberOfPeople / (datasetMax));
					val = System.Math.Sqrt(val);
					float f = (float)val;
					texture.SetPixel(x, y, new Color(f, f, f, 1.0f));
				}
			}
		}
		texture.Apply();
		return texture;
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
