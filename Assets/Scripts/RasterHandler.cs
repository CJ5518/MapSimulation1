//By Carson Rueber

using UnityEngine;
using OSGeo.GDAL;
using NLua;

public enum RasterType {
	Population, //Must be the same as the enum name
	Elevation,
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

	public RasterHandler(RasterType major, int? minor) {
		majorType = major;
		minorType = minor;
		RasterUtilities = LuaSingleton.lua.GetTable("RasterUtilities");
		
		LuaFunction getWarpedFilename = (LuaFunction)RasterUtilities["getWarpedFilename"];
		object[] returnValues = getWarpedFilename.Call(major, minor);
		outputTifFilename = (string)returnValues[0];
		getWarpedFilename.Dispose();
	}

	public void preprocessData() {
		dataset = Gdal.Open(outputTifFilename, Access.GA_ReadOnly);
	}

	//Loads the first band of the dataset to a texture
	public Texture2D loadToTexture(int width, int height) {
		//The first band
		Band rasterBand = dataset.GetRasterBand(1);
		//Output texture
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

		//Raster data buffer
		double[] rasterData = new double[1];

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

				//Read in the raster data
				rasterBand.ReadRaster(
					rasterCoords.x, rasterCoords.y,
					1, 1,
					rasterData,
					1, 1,
					0, 0
				);

				//Get number of people
				double numberOfPeople = double.IsNaN(rasterData[0]) ? 0.0 : rasterData[0];

				//Output the color
				Color32 color = Simulation.floatToColor((float)numberOfPeople);

				//Just to make sure the color is set just right
				texture.SetPixels32(x, y, 1, 1, new Color32[] { color });
			}
		}
		texture.Apply();
		return texture;
	}
}