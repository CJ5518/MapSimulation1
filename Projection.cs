//By Carson Rueber

using UnityEngine;
using ShapeImporter;

//Projects lat/longs into screen coordinates and back again
//https://stackoverflow.com/a/4565555
public class Projection {

	//Single number lat/longs
	public static float projectLongitude(float x) {
		return (Screen.width / 360.0f) * (180.0f + x);
	}
	public static double projectLongitude(double x) {
		return (Screen.width / 360.0) * (180.0 + x);
	}
	public static float projectLatitude(float y) {
		return (Screen.height / 180.0f) * (90.0f - y);
	}
	public static double projectLatitude(double y) {
		return (Screen.height / 180.0) * (90.0 - y);
	}

	//Vector lat/longs
	public static Vector2 projectVector(Vector2 vector) {
		Vector2 ret;
		ret.x = projectLongitude(vector.x);
		ret.y = projectLatitude(vector.y);
		return ret;
	}
	public static Vector2Double projectVector(Vector2Double vector) {
		Vector2Double ret;
		ret.x = projectLongitude(vector.x);
		ret.y = projectLatitude(vector.y);
		return ret;
	}


	//Single number projections
	public static float projectionToLongitude(float x) {
		return (x / (Screen.width / 360.0f)) - 180.0f;
	}
	public static double projectionToLongitude(double x) {
		return (x / (Screen.width / 360.0)) - 180.0;
	}
	public static float projectionToLatitude(float y) {
		return 90.0f - (y / (Screen.height / 180.0f));
	}
	public static double projectionToLatitude(double y) {
		return 90.0 - (y / (Screen.height / 180.0));
	}

	//Vector projections
	public static Vector2 projectionToLatLongs(Vector2 vector) {
		Vector2 ret;
		ret.x = projectionToLongitude(vector.x);
		ret.y = projectionToLatitude(vector.y);
		return ret;
	}
	public static Vector2Double projectionToLatLongs(Vector2Double vector) {
		Vector2Double ret;
		ret.x = projectionToLongitude(vector.x);
		ret.y = projectionToLatitude(vector.y);
		return ret;
	}


	//Render space information

	//The smallest x,y coords of the projected lat/long area
	//Defines the horizontal/vertical translation of render space
	public static Vector2 renderSpaceProjectedMin;

	//The scaling factor of render space from projection space
	public static float renderSpaceScalingFactor;

	//Set the render space parameters from a shape file
	//The parameters will be set so that the shapefile is large and in charge
	public static void setRenderSpaceByShapeFile(ShapeFile shapeFile) {
		Vector2 min, max, projectedMin, projectedMax;
		//Collect min and max
		min.x = (float)shapeFile.FileHeader.XMin;
		min.y = (float)shapeFile.FileHeader.YMin;
		max.x = (float)shapeFile.FileHeader.XMax;
		max.y = (float)shapeFile.FileHeader.YMax;

		projectedMin = Projection.projectVector(min);
		projectedMax = Projection.projectVector(max);

		//Scaling factor
		float factorX = Mathf.Abs(Screen.width / (projectedMax.x - projectedMin.x));
		float factorY = Mathf.Abs(Screen.height / (projectedMax.y - projectedMin.y));

		//Set our parameters

		//Pick the smallest one
		renderSpaceScalingFactor = factorX < factorY ? factorX : factorY;
		renderSpaceProjectedMin = projectedMin;
	}

	public static void setRenderSpaceByShapeFile(string filename) {
		ShapeFile shapeFile = new ShapeFile();
		shapeFile.ReadShapes(filename);
		setRenderSpaceByShapeFile(shapeFile);
	}

	public static Vector2 projectionToRenderSpace(Vector2 point) {
		//Move to bottom left
		point -= renderSpaceProjectedMin;
		point.y = Screen.height - (point.y + Screen.height);

		//Scale up
		point *= renderSpaceScalingFactor;

		return point;
	}
	public static Vector2Double projectionToRenderSpace(Vector2Double point) {
		//Move to bottom left
		point -= renderSpaceProjectedMin;
		point.y = Screen.height - (point.y + Screen.height);

		//Scale up
		point *= renderSpaceScalingFactor;

		return point;
	}

	public static Vector2 renderSpaceToProjection(Vector2 point) {
		//Scale down
		point /= renderSpaceScalingFactor;

		//Move back to where it ought to be
		point.y = -point.y;
		point += renderSpaceProjectedMin;

		return point;
	}
	//Render space to projected coords
	public static Vector2Double renderSpaceToProjection(Vector2Double point) {
		//Scale down
		point /= renderSpaceScalingFactor;

		//Move back to where it ought to be
		point.y = -point.y;
		point += renderSpaceProjectedMin;

		return point;
	}
}