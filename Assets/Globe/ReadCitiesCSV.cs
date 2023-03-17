using UnityEngine;

public class ReadCitiesCSV : MonoBehaviour {

	public TextAsset csvFile;
	string[,] data;
	
	void Start() {
		data = getCSVGrid(csvFile);
		
	}

	void Update() {
		drawAirportsSphere(data);
	}



	// draw cities using lat/lng as world coords
	// lat in col 1, lng in col 2, and labels as row 0
	void drawCitiesRect(string[,] grid) {
		for (int i = 1; i < grid.GetUpperBound(1); i++) {
			Vector3 pos = new Vector3(float.Parse(grid[2, i]), 0f, float.Parse(grid[1, i]));
			Debug.DrawRay(pos, Vector3.up);
		}
	}

	void drawCitiesSphere(string[,] grid) {
		for (int i = 1; i < grid.GetUpperBound(1); i++) {
			float lat = float.Parse(grid[1, i]);
			float lng = float.Parse(grid[2, i]);
			float pop = float.Parse(grid[5, i]);
			Vector3 pos = Quaternion.AngleAxis(lng + 180, -Vector3.up)
				* Quaternion.AngleAxis(lat, -Vector3.right)
				* new Vector3(0, 0, 149);

			Color c = Color.Lerp(Color.red, Color.white, LogTransform(pop));
			Debug.DrawRay(pos, pos.normalized * pop / 1000000f, c);
		}
	}
	float LogTransform(float value) {
		return (Mathf.Log(value, 1.000001f) - 9900000) / 7582000f;
	}




	void drawAirportsSphere(string[,] grid) {
		for (int i = 1; i < grid.GetUpperBound(1); i++) {
			float lat = float.Parse(grid[2, i]);
			float lng = float.Parse(grid[3, i]);
			float pop = float.Parse(grid[0, i]);
			Vector3 pos = Quaternion.AngleAxis(lng + 180, -Vector3.up)
				* Quaternion.AngleAxis(lat, -Vector3.right)
				* new Vector3(0, 0, 149);
			Color c = Color.Lerp(Color.red, Color.white, LogTransform(pop));
			Debug.DrawRay(pos, pos.normalized * pop / 1000000f, c);
		}
	}





	#region CSV
	// split a CSV file into a 2D string array
	static public string[,] getCSVGrid(TextAsset csv) {
		string csvText = csv.text;

		//split the data on split line character
		string[] lines = csvText.Split("\n"[0]);

		// find the max number of columns
		int totalColumns = 0;
		for (int i = 0; i < lines.Length; i++) {
			string[] row = lines[i].Split(',');
			totalColumns = Mathf.Max(totalColumns, row.Length);
		}

		// creates new 2D string grid to output to
		string[,] outputGrid = new string[totalColumns + 1, lines.Length + 1];
		for (int y = 0; y < lines.Length; y++) {
			string[] row = lines[y].Split(',');
			for (int x = 0; x < row.Length; x++) {
				outputGrid[x, y] = row[x];
			}
		}

		return outputGrid;
	}
	
	// Gets the value from the CSV File at index(row,col).
	void getValueAtIndex(int row, int col) {
		Debug.Log(data[row, col]);
	}
	
	// outputs the content of a 2D array.
	static public void DisplayGrid(string[,] grid) {
		string textOutput = "";
		for (int y = 0; y < grid.GetUpperBound(1); y++) {
			for (int x = 0; x < grid.GetUpperBound(0); x++) {
				textOutput += grid[x, y];
				textOutput += ",";
			}
			textOutput += "\n";
		}
		Debug.Log(textOutput);
	}
	#endregion
}