//By Carson Rueber

using System;
using UnityEngine;
using OSGeo.GDAL;
using NLua;

public enum RasterType {
	Population, //Must be the same as the enum name
	Elevation,
	VaccRate,
	RasterTypeCount
}

public enum Population {
	ChildrenUnderFive,
	ElderlySixtyPlus,
	Men,
	FullPopulation,
	Women,
	WomenOfReproductiveAge,
	Youth15To24,
	PopulationCount //Must be [EnumName]Count
}

public class RasterHandler {
	private RasterType majorType;
	private int? minorType;

	private string outputTifFilename;
	private Dataset dataset;
	private LuaTable RasterUtilities;

	public RasterHandler(RasterType major, int? minor = null) {
		majorType = major;
		minorType = minor;
		RasterUtilities = LuaSingleton.lua.GetTable("RasterUtilities");
		
		LuaFunction getWarpedFilename = (LuaFunction)RasterUtilities["getWarpedFilename"];
		object[] returnValues = getWarpedFilename.Call(major, minor);
		outputTifFilename = (string)returnValues[0];
		getWarpedFilename.Dispose();
	}

	//Loads the first band of the dataset to a texture
	public Texture2D loadToTexture(int width, int height) {
		dataset = Gdal.Open(outputTifFilename, Access.GA_ReadOnly);
		//The first band
		Band rasterBand = dataset.GetRasterBand(1);
		//Output texture
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

		//Raster data buffer
		double[] doubleBuffer = new double[1];
		Int16[] elevationBuffer = new Int16[1];

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
					(Projection.worldToRasterSpace(worldCoords, dataset) + new Vector2Double(0.5, 0.5));
				rasterCoords.x += x;
				rasterCoords.y -= y;

				//If not in bounds, skip
				if (
					!(rasterCoords.x >= 0 && rasterCoords.y >= 0 &&
					rasterCoords.x + 1 < dataset.RasterXSize && rasterCoords.y + 1 < dataset.RasterYSize)
				) {
					continue;
				}
				Color32 color = new Color32(128,128,128,128);
				switch (majorType) {
					case RasterType.Population:
						rasterBand.ReadRaster(
							rasterCoords.x, rasterCoords.y, 1, 1,
							doubleBuffer,
							1,1,0,0
						);
						color = loadPopulationData(doubleBuffer[0]);
						break;
					case RasterType.Elevation:
						rasterBand.ReadRaster(
							rasterCoords.x, rasterCoords.y, 1, 1,
							elevationBuffer,
							1,1,0,0
						);
						color = loadElevationData(elevationBuffer[0]);
						break;
					case RasterType.VaccRate:
						rasterBand.ReadRaster(
							rasterCoords.x, rasterCoords.y, 1, 1,
							doubleBuffer,
							1,1,0,0
						);
						color = loadVaccRateData(doubleBuffer[0]);
						break;
				}

				//Just to make sure the color is set just right
				texture.SetPixels32(x, y, 1, 1, new Color32[] { color });
			}
		}
		texture.Apply();
		return texture;
	}

	//Functions to facilitate loading textures from specific rasters
	//Called for each raster pixel, see above

	Color32 loadPopulationData(double data) {
		//Get number of people
		double numberOfPeople = double.IsNaN(data) ? 0.0 : data;

		//Output the color
		return Simulation.floatToColor((float)numberOfPeople);
	}

	Color32 loadElevationData(Int16 data) {
		return Simulation.intToColor((int)data);
	}

	//Could combine this with population, maybe have a generic 'double' function
	Color32 loadVaccRateData(double data) {
		return Simulation.floatToColor((float)data);
	}

}