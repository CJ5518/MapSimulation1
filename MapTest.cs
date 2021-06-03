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
		int factor = 4;
		finalTexture = new Texture2D(1920 / factor, 1080 / factor, TextureFormat.RGBA32, false);
		for (int x = 0; x < finalTexture.width; x++) {
			for (int y = 0; y < finalTexture.height; y++) {
				bool isInShape = IsPointInPolygon(shapeFileRenderer.projectedShapes[2], new Vector2(x * factor, y * factor));
				finalTexture.SetPixel(x, y, isInShape ? Color.white : Color.clear);
			}
		}
		finalTexture.filterMode = FilterMode.Point;
		finalTexture.Apply();
		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds");


		//GDAL testing
		Gdal.AllRegister();
		Ogr.RegisterAll();

		startTime = Time.realtimeSinceStartupAsDouble;

		CSVLoaderJob[] jobs = new CSVLoaderJob[6];
		JobHandle[] jobHandles = new JobHandle[6];
		for (int q = 0; q < 6; q++) {
			string filename = @"F:\Data\csv\population_usa_2019-07-01_part_" + (q + 1) + "_of_6.csv";
			var job = new CSVLoaderJob() {
				//File.ReadLines(@"C:\file.txt").Count();
				filename = new NativeArray<char>(filename.ToCharArray(), Allocator.Persistent),
				//NEED a MUCH better way to count the lines in the files
				outputData = new NativeArray<DataEntry>(File.ReadAllLines(filename).Length, Allocator.Persistent)
			};
			jobHandles[q] = job.Schedule();
		}
		for (int q = 0; q < 6; q++) {
			jobHandles[q].Complete();
			Debug.Log(jobs[q].outputData[192]);
			jobs[q].outputData.Dispose();
		}
		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds");

	}



	void OnThicknessSliderValueChanged(float value) {
		shapeFileRenderer.bigLineListRenderer.setLineThickness(value);
	}
	void OnGUI() {
		if (finalTexture != null && Event.current.type.Equals(EventType.Repaint) && wantDraw) {
			Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), finalTexture);
		}
	}

	//TODO: Delete this function
	static string GDALInfoGetPosition(Dataset ds, double x, double y) {
		double[] adfGeoTransform = new double[6];
		double dfGeoX, dfGeoY;
		ds.GetGeoTransform(adfGeoTransform);

		dfGeoX = adfGeoTransform[0] + adfGeoTransform[1] * x + adfGeoTransform[2] * y;
		dfGeoY = adfGeoTransform[3] + adfGeoTransform[4] * x + adfGeoTransform[5] * y;

		return dfGeoX.ToString() + ", " + dfGeoY.ToString();
	}

	private void Update() {
		//Texture testing to hide/show the texture
		if (Input.GetKeyDown(KeyCode.Q)) {
			wantDraw = !wantDraw;
		}
		Vector2 coord = movableRawImage.getLocalPositionInRectangle(Input.mousePosition);
		//Vector2 coord = Input.mousePosition;
		GameObject.Find("Canvas/Text").GetComponent<Text>().text = IsPointInPolygon(shapeFileRenderer.projectedShapes[2], coord) ? "inside" : "outside";
	}

	public struct CSVLoaderJob : IJob {

		public NativeArray<DataEntry> outputData;

		//The pain of getting a string into this
		public NativeArray<char> filename;
		
		public void Execute() {
			char[] filenameChars = new char[filename.Length];
			filename.CopyTo(filenameChars);
			using (var reader = new StreamReader(new string(filenameChars))) {

				List<DataEntry> list = new List<DataEntry>();

				while (!reader.EndOfStream) {
					var line = reader.ReadLine();
					var values = line.Split(',');
					DataEntry dataEntry = new DataEntry();
					dataEntry.lat = Convert.ToDouble(values[0]);
					dataEntry.lon = Convert.ToDouble(values[1]);
					dataEntry.pop = Convert.ToDouble(values[2]);
					list.Add(dataEntry);
				}
				outputData.CopyFrom(list.ToArray());
			}
		}
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