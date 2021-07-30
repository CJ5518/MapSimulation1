//By Carson Rueber

using UnityEngine;
using ShapeImporter;
using OSGeo.GDAL;

//Projects lat/longs into texture area coordinates and back again, among other things
public class Projection {
	//Width and height of the working texture
	public const int width = 1920 / 4;
	public const int height = 1080 / 4;

	//Single number lat/longs
	public static float projectLongitude(float x) {
		return (width / 360.0f) * (180.0f + x);
	}
	public static double projectLongitude(double x) {
		return (width / 360.0) * (180.0 + x);
	}
	public static float projectLatitude(float y) {
		return (height / 180.0f) * (90.0f - y);
	}
	public static double projectLatitude(double y) {
		return (height / 180.0) * (90.0 - y);
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
		return (x / (width / 360.0f)) - 180.0f;
	}
	public static double projectionToLongitude(double x) {
		return (x / (width / 360.0)) - 180.0;
	}
	public static float projectionToLatitude(float y) {
		return 90.0f - (y / (height / 180.0f));
	}
	public static double projectionToLatitude(double y) {
		return 90.0 - (y / (height / 180.0));
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

	//Offset between the 'real' texture and the big surrounding one
	public static Vector2Int textureOffset;

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
		float factorX = Mathf.Abs(width / (projectedMax.x - projectedMin.x));
		float factorY = Mathf.Abs(height / (projectedMax.y - projectedMin.y));

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

	//Project to render space

	public static Vector2 projectionToRenderSpace(Vector2 point) {
		//Move to bottom left
		point -= renderSpaceProjectedMin;
		point.y = height - (point.y + height);

		//Scale up
		point *= renderSpaceScalingFactor;

		//Offset
		point += textureOffset;

		return point;
	}
	public static Vector2Double projectionToRenderSpace(Vector2Double point) {
		//Move to bottom left
		point -= renderSpaceProjectedMin;
		point.y = height - (point.y + height);

		//Scale up
		point *= renderSpaceScalingFactor;

		//Offset
		point += textureOffset;

		return point;
	}

	//Render space to projection

	public static Vector2 renderSpaceToProjection(Vector2 point) {
		//Scale down
		point /= renderSpaceScalingFactor;

		//Move back to where it ought to be
		point.y = -point.y;
		point += renderSpaceProjectedMin;

		return point;
	}

	public static Vector2Double renderSpaceToProjection(Vector2Double point) {
		//Scale down
		point /= renderSpaceScalingFactor;

		//Move back to where it ought to be
		point.y = -point.y;
		point += renderSpaceProjectedMin;

		return point;
	}

	//Render space to lat/longs
	public static Vector2Double renderSpaceToLatLongs(Vector2Double point) {
		return projectionToLatLongs(renderSpaceToProjection(point));
	}

	public static Vector2 renderSpaceToLatLongs(Vector2 point) {
		return projectionToLatLongs(renderSpaceToProjection(point));
	}

	public static void setTextureOffsetByTextures(Texture2D large, Texture2D small) {
		textureOffset.x = (large.width - small.width) / 2;
		textureOffset.y = (large.height - small.height) / 2;
	}


	//Returns pixel size in lat long given pixel size in screen space
	public static Vector2Double getPixelSizeInLatLong() {
		//Get two screen pixels and convert them to lat longs
		Vector2Double corner = new Vector2Double(0, 0);
		Vector2Double projectedCornerCoords = renderSpaceToProjection(corner);
		Vector2Double worldCornerCoords = projectionToLatLongs(projectedCornerCoords);

		Vector2Double other = new Vector2Double(1, 1);
		Vector2Double projectedOtherCoords = renderSpaceToProjection(other);
		Vector2Double worldOtherCoords = projectionToLatLongs(projectedOtherCoords);

		//Take the absolute value of their difference
		Vector2Double ret = new Vector2Double(
			System.Math.Abs(worldCornerCoords.x - worldOtherCoords.x),
			System.Math.Abs(worldCornerCoords.y - worldOtherCoords.y)
		);
		return ret;
	}

	//Raster coordinate functions
	//Convert from raster space to lat/longs and vice versa
	public static Vector2Double rasterSpaceToWorld(Vector2Double rasterPixel, Dataset dataset) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double(argout[0] + (argout[1] * rasterPixel.x), argout[3] + (argout[5] * rasterPixel.y));
	}
	public static Vector2Double worldToRasterSpace(Vector2Double coords, Dataset dataset) {
		double[] argout = new double[6];
		dataset.GetGeoTransform(argout);
		return new Vector2Double((coords.x - argout[0]) / argout[1], (coords.y - argout[3]) / argout[5]);
	}
}