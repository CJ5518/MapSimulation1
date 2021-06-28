//By Carson Rueber

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShapeImporter;

//Renders a shape file using BigLineListRenderer
//Also provides the shapes in their projected/scaled form
//Render space must be set up before creating this
public class ShapeFileRenderer {
	//The shape file
	public ShapeFile shapeFile;
	//The rendering object
	public BigLineListRenderer bigLineListRenderer;
	//List of arrays holding the points of the projected and scaled shapes, for your pleasure
	public List<Vector2[]> renderShapes;
	//The non-projected shapes, directly extracted from the shape file
	public List<Vector2Double[]> nonProjectedShapes;

	//Init ShapeFileRenderer with a loaded shapeFile
	public ShapeFileRenderer(ShapeFile shapeFile, Transform parent) {
		this.shapeFile = shapeFile;
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

		renderShapes = new List<Vector2[]>();
		nonProjectedShapes = new List<Vector2Double[]>();


		//Load the data
		for (int shapeIndex = 0; shapeIndex < shapeFile.MyRecords.Count; shapeIndex++) {
			ShapeFileRecord myRecord = shapeFile.MyRecords[shapeIndex];

			renderShapes.Add(new Vector2[myRecord.Points.Count]);
			nonProjectedShapes.Add(new Vector2Double[myRecord.Points.Count]);

			for (int q = 0; q < myRecord.Points.Count; q++) {
				//Only use doubles for the precise part
				Vector2Double point = myRecord.Points[q];
				point = Projection.projectVector(point);
				Vector2 floatPoint = new Vector2((float)point.x, (float)point.y);

				floatPoint = Projection.projectionToRenderSpace(floatPoint);

				bigLineListRenderer.addPoint(floatPoint);
				renderShapes[shapeIndex][q] = floatPoint;
				nonProjectedShapes[shapeIndex][q] = point;
			}
			bigLineListRenderer.finishLine();
		}
	}
}
