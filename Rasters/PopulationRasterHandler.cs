using System;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;

public class PopulationRasterHandler : RasterHandler {
	const string filename = @"F:\Data\tif\resampled_image.tif";

	//The important band
	Band rasterBand;

	public PopulationRasterHandler() {
		//Open the dataset
		dataset = Gdal.Open(filename, Access.GA_ReadOnly);

		//The population data only has 1 band
		rasterBand = dataset.GetRasterBand(1);

		//Collect statistics

		rasterBand.GetStatistics(0, 1, out datasetMin, out datasetMax, out datasetMean, out datasetStdDev);
	}
	public override Texture2D loadToTexture(int width, int height, ShapeFileRenderer shapeFileRenderer) {
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
		int rasterPixelsPerImagePixel = (int)(pixelSize / rasterProjectedPixelSize.x);

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
				Vector2Double rasterCoords = worldToRasterSpace(worldCoords);

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
				float val = (float)(numberOfPeople / (datasetMax * 5.0));

				if (numberOfPeople != 0.0)
					texture.SetPixel(x, y, new Color(val, val, val, 1.0f));

			}
		}
		texture.Apply();
		return texture;
	}
}
