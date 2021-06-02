//By Carson Rueber

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShapeImporter;

//Renders a shape file using BigLineListRenderer
//Also provides the shapes in their projected form
//This functionality should probably be moved to a better shape file handler
public class ShapeFileRenderer {
	//The shape file
	public ShapeFile shapeFile;
	//The rendering object
	public BigLineListRenderer bigLineListRenderer;
	//List of arrays holding the points of the projected and scaled shapes, for your pleasure
	public List<Vector2[]> projectedShapes;

	//Init ShapeFileRenderer with a loaded shapeFile
	public ShapeFileRenderer(ShapeFile shapeFile, Transform parent) {
		Init(parent);
	}
	//Init ShapeFileRenderer with a filename to a shapeFile
	public ShapeFileRenderer(string filename, Transform parent) {
		shapeFile = new ShapeFile();
		shapeFile.ReadShapes(filename);
		Init(parent);
	}

	//Initialize the BigLineListRenderer
	private void Init(Transform parent) {
		//Create the object
		bigLineListRenderer = new BigLineListRenderer();
		bigLineListRenderer.gameObject.transform.SetParent(parent);

		projectedShapes = new List<Vector2[]>();

		//Collect min and max
		Vector2 min, max, projectedMin, projectedMax;
		min.x = (float)shapeFile.FileHeader.XMin;
		min.y = (float)shapeFile.FileHeader.YMin;
		max.x = (float)shapeFile.FileHeader.XMax;
		max.y = (float)shapeFile.FileHeader.YMax;

		projectedMin = Projection.projectVector(min);
		projectedMax = Projection.projectVector(max);

		//Scaling factor
		float factorX = Mathf.Abs(Screen.width / (projectedMax.x - projectedMin.x));
		float factorY = Mathf.Abs(Screen.height / (projectedMax.y - projectedMin.y));

		//Pick the smallest one
		float factor = factorX < factorY ? factorX : factorY;

		//Load the data
		for (int shapeIndex = 0; shapeIndex < shapeFile.MyRecords.Count; shapeIndex++) {
			ShapeFileRecord myRecord = shapeFile.MyRecords[shapeIndex];

			projectedShapes.Add(new Vector2[myRecord.Points.Count]);

			for (int q = 0; q < myRecord.Points.Count; q++) {
				//Only use doubles for the precise part
				Vector2Double point = myRecord.Points[q];
				point = Projection.projectVector(point);
				Vector2 floatPoint = new Vector2((float)point.x, (float)point.y);

				//Move to bottom left
				floatPoint -= projectedMin;
				floatPoint.y = Screen.height - (floatPoint.y + Screen.height);

				floatPoint *= factor;

				bigLineListRenderer.addPoint(floatPoint);
				projectedShapes[shapeIndex][q] = floatPoint;
			}
			bigLineListRenderer.finishLine();
		}
	}
}
