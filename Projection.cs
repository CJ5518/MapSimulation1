//By Carson Rueber

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
}