//By Carson Rueber
//The only file to survive the purge

using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Unity.Jobs;
using Unity.Collections;
using OSGeo.OSR;
using OSGeo.GDAL;
using OSGeo.OGR;

//Test class
public class MapTest : MonoBehaviour {
	//Our shape file renderer
	ShapeFileRenderer shapeFileRenderer;

	//Controls the thickness of the lines of the shape file renderer
	Slider thicknessSlider;

	//Texture testing things
	Texture2D finalTexture = null;
	bool wantDraw = true;
	//The background image
	MovableRawImage movableRawImage;

	public struct DataEntry {
		public double lat;
		public double lon;
		public double pop;
	}

	void Start() {
		Application.targetFrameRate = 60;

		//Set up the shape file renderer
		shapeFileRenderer = new ShapeFileRenderer(
			@"C:\Users\carso\Desktop\DataPart2\USA_Final.shp", GameObject.Find("Canvas/Background").transform
		);

		//Set up thickness slider
		thicknessSlider = GameObject.Find("Canvas/ThicknessSlider").GetComponent<Slider>();
		thicknessSlider.value = 1.0f;
		thicknessSlider.onValueChanged.AddListener(OnThicknessSliderValueChanged);

		movableRawImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();

		//Generate the texture
		//Good to keep it at the screen aspect ratio
		double startTime = Time.realtimeSinceStartupAsDouble;

		//Texture scaling factor
		int pixelSize = 20;
		finalTexture = new Texture2D(1920 / pixelSize, 1080 / pixelSize, TextureFormat.RGBA32, false);
		for (int x = 0; x < finalTexture.width; x++) {
			for (int y = 0; y < finalTexture.height; y++) {
				bool isInShape = IsPointInPolygon(
					shapeFileRenderer.renderShapes[2], //FIX: Bit of a hack
					new Vector2(x * pixelSize, y * pixelSize)
				);
				finalTexture.SetPixel(x, y, isInShape ? Color.white : Color.clear);
			}
		}
		finalTexture.filterMode = FilterMode.Point;
		finalTexture.Apply();
		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds");


		//GDAL testing
		Gdal.AllRegister();

		startTime = Time.realtimeSinceStartupAsDouble;

		//Open the dataset
		string filename = @"F:\Data\tif\USA_lat_24_lon_-111_children_under_five.tif";
		Dataset dataset = Gdal.Open(filename, Access.GA_ReadOnly);
		//This starts counting at 1 for some reason
		Band rasterBand = dataset.GetRasterBand(1);

		//Just for testing
		const float USPopulation = 306000000.0f;

		//TODO:
		/*
		 * Get the number of raster pixels that should make up one of these pixels, see
		 * https://github.com/OSGeo/gdal/blob/master/gdal/swig/csharp/apps/GDALInfo.cs
		 * for some info on how to do that, then count the number of people there, divide by US pop
		 * over number of pixels, although that would make the average number be the max color
		 * so maybe not on that front, but you get the jist
		 */

		//Gather info about the raster
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		for (int q = 0; q < 6; q++) {
			Debug.Log(q + " " + argout[q]);
		}
		
		//The size of a raster pixel, scaled up to render space
		Vector2Double rasterProjectedPixelSize = new Vector2Double(argout[1], argout[5]) * shapeFileRenderer.scalingFactor;

		//The number of raster pixels that make up one of our texture pixels
		//Technically the sqrt of the actual figure, this is the number in a single row
		int rasterPixelsPerImagePixel = (int)(pixelSize / rasterProjectedPixelSize.x);

		Vector2Double topLeftCorner = rasterSpaceToWorld(dataset, Vector2Double.Zero);

		Debug.Log(rasterPixelsPerImagePixel);

		//Raster data buffer
		double[] rasterData = new double[rasterPixelsPerImagePixel * rasterPixelsPerImagePixel];

		//For every pixel in the image
		for (int x = 0; x < finalTexture.width; x++) {
			for (int y = 0; y < finalTexture.height; y++) {
				//Convert these coords to world coords
				Vector2 screenCoords = new Vector2(x * pixelSize, y * pixelSize);
				Vector2 projectedCoords = shapeFileRenderer.renderSpaceToProjection(screenCoords);
				Vector2Double worldCoords = Projection.projectionToLatLongs((Vector2Double)projectedCoords);

				//Get raster coords from the world coords
				Vector2Double rasterCoords = worldToRasterSpace(dataset, worldCoords);

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
					0,0
				);
				//Iterate over the raster data
				double numberOfPeople = 0.0;
				for (int q = 0; q < rasterData.Length; q++) {
					if (!double.IsNaN(rasterData[q])) {
						numberOfPeople += rasterData[q];
					}
				}
				Debug.Log(numberOfPeople);
			}
		}


		dataset.Dispose();

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds");

	}

	//Convert from raster space to lat/longs
	Vector2Double rasterSpaceToWorld(Dataset dataset, Vector2Double rasterPixel) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double(argout[0] + (argout[1] * rasterPixel.x), argout[3] + (argout[5] * rasterPixel.y));
	}
	Vector2Double worldToRasterSpace(Dataset dataset, Vector2Double coords) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double((coords.x - argout[0]) / argout[1], (coords.y - argout[3]) / argout[5]);
	}


	void OnThicknessSliderValueChanged(float value) {
		shapeFileRenderer.bigLineListRenderer.setLineThickness(value);
	}
	void OnGUI() {
		if (finalTexture != null && Event.current.type.Equals(EventType.Repaint) && wantDraw) {
			Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), finalTexture);
		}
	}

	private void Update() {
		//Texture testing to hide/show the texture
		if (Input.GetKeyDown(KeyCode.Q)) {
			wantDraw = !wantDraw;
		}
		Vector2 coord = movableRawImage.getLocalPositionInRectangle(Input.mousePosition);
		//Fix this too, same hack as above
		GameObject.Find("Canvas/Text").GetComponent<Text>().text = 
			IsPointInPolygon(shapeFileRenderer.renderShapes[2], coord) ? "inside" : "outside";
		
	}


	//https://stackoverflow.com/a/14998816
	public static bool IsPointInPolygon(Vector2[] polygon, Vector2 testPoint) {
		bool result = false;
		int j = polygon.Length - 1;
		for (int i = 0; i < polygon.Length; i++) {
			if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y) {
				if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x) {
					result = !result;
				}
			}
			j = i;
		}
		return result;
	}
}