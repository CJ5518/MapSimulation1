// Decompiled with JetBrains decompiler
// Type: MapTest
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DADC71AF-6ED1-41B5-9B7D-530B78799929
// Assembly location: C:\Users\carso\Desktop\Build\MapSimulation0_Data\Managed\Assembly-CSharp.dll

using ShapeImporter;
using System.Collections.Generic;
using UnityEngine;

public class OldMapTest : MonoBehaviour {
	private List<MapTest.Entry> entries;
	private BigLineRenderer bigLineRenderer;

	private void Start() {
		Application.targetFrameRate = 60;
		ShapeFile shapeFile = new ShapeFile();
		shapeFile.ReadShapes(Application.dataPath + "/GeneralizedCountries.shp");
		for (int index1 = 0; index1 < shapeFile.MyRecords.Count; ++index1) {
			ShapeFileRecord myRecord = shapeFile.MyRecords[index1];
			this.bigLineRenderer = new BigLineRenderer();
			this.bigLineRenderer.gameObject.transform.SetParent(GameObject.Find("Canvas/MapImage").transform);
			for (int index2 = 0; index2 < myRecord.Points.Count; ++index2) {
				Vector2d point = myRecord.Points[index2];
				this.bigLineRenderer.AddPoint(new Vector2((float)((double)Screen.width / 360.0 * (180.0 + point.x)), (float)Screen.height - (float)((double)Screen.height / 180.0 * (90.0 - point.y))));
			}
			this.bigLineRenderer.finishAdding();
		}
	}

	private void OnGUI() {
	}

	private struct Entry {
		public string state;
		public string fips;
		public double lon;
		public double lat;
	}
}
