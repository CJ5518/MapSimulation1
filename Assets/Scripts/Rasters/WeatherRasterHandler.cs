using System.IO;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.GDAL;
using System.Xml;

class WeatherRasterHandler : RasterHandler {
	public override bool downloadData() {
		throw new System.NotImplementedException();
	}
	public override bool preprocessData() {
		throw new System.NotImplementedException();
	}
	public override Texture2D loadToTexture(int width, int height) {
		throw new System.NotImplementedException();
	}
}