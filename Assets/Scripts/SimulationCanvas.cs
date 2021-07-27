using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SimulationCanvas : MonoBehaviour
{
    #region Refernces Vars
    public Main main;

    const float statisticsUpdatesPerSecond = 10.0f;
    float lastStatsTime = -100.0f;


    [Header("Canvas Objects")]
    public TimeBarGraph timeOverallGraph;
    public PieChart overallChart;
    public PieChart hoverChart;

    [Header("0 Alpha, 1 Gamma, 2 Spread, 3 Sigma, 4 Delta, 5 Contact, 6 Infection")]
    public Slider[] sliders;
    //public Text betaText;
    //public Text r0Text;

    #endregion
    public void UpdateCanvas()
    {
        //r0
        //r0Text.text = "r0: " + (main.simulation.data.beta / main.simulation.data.gamma).ToString("f2");
        updateStatisticsLabel();
    }
    #region Toggles
    /*
     * Connect Toggle to this; default state of toggle should match whatever default state the game loads with or it will be flopped
     */
    public void ToggleRec() {
        main.simulation.data.drawRecovered = !main.simulation.data.drawRecovered;
    }
    public void ToggleZom() {
        main.simulation.data.drawInfected = !main.simulation.data.drawInfected;
    }
    public void ToggleVaccinations() {
        main.simulation.data.drawVaccinated = !main.simulation.data.drawVaccinated;
    }
    //Removed draw proportion because it wasn't particularily useful or important
    public void ToggleElevation() {
        main.simulation.data.drawElevation = !main.simulation.data.drawElevation;
    }
    public void ToggleMove() {
        main.simulation.data.moveZombies = !main.simulation.data.moveZombies;
    }
    #endregion
    #region Sliders
    /*
     * Connect Sliders here; must be in same order
     */
    public void UpdateSliderValues() {
        //Alpha, beta, gamma, etc.
        main.simulation.data.beta = sliders[6].value * sliders[5].value;
        main.simulation.data.alpha = sliders[0].value;
        main.simulation.data.gamma = sliders[1].value;
        main.simulation.data.sigma = sliders[3].value;
        main.simulation.data.delta = sliders[4].value;
        main.simulation.data.spreadRate = sliders[2].value;
    }
    #endregion
    //Updates the statistics label based on the pixel the mouse is over, and targetDemographic
    int lastIndex = -1;
    public unsafe void updateStatisticsLabel() {
        //Find the hoverPixel
        Vector2 hoverLocation = getPixelFromScreenCoord(Input.mousePosition);
        int indexOfPixel = main.simulation.coordToIndex(hoverLocation);

        //Gather statistics if in correct update time
        if (Time.realtimeSinceStartup - lastStatsTime >= 1.0f / statisticsUpdatesPerSecond) {
            //Total Data//
            //Total vars setup
            float[] totals = new float[5];
            float totalPeople = 0;

            //Gather Total data
            for (int q = 0; q < main.simulation.readCells.Length; q++) {
                Simulation.Cell readCell = main.simulation.readCells[q];
                totals[3] += readCell.susceptible[main.targetDemographic];
                totals[1] += readCell.infected[main.targetDemographic];
                totals[0] += readCell.recovered[main.targetDemographic];
                totals[1] += readCell.exposed[main.targetDemographic];
                totals[4] += readCell.vaccinated[main.targetDemographic];
                totalPeople += readCell.numberOfPeople[main.targetDemographic];
            }
            overallChart.UpdateGraph(totals, totalPeople);
            timeOverallGraph.UpdateTimeBar(totals, totalPeople);
            //Hover Data//
            //Use last cell if this cell is broken
            if (!main.simulation.cellIsValid(indexOfPixel) && main.simulation.cellIsValid(lastIndex))
                indexOfPixel = lastIndex;
            //Gather data from valid cell
            if (main.simulation.cellIsValid(indexOfPixel)) {
                lastIndex = indexOfPixel;
                Simulation.Cell cell = main.simulation.readCells[indexOfPixel];
                float[] cellVals = new float[5];
                cellVals[3] = cell.susceptible[main.targetDemographic];
                cellVals[4] = cell.vaccinated[main.targetDemographic];
                cellVals[1] = cell.infected[main.targetDemographic];
                cellVals[0] = cell.recovered[main.targetDemographic];
                cellVals[2] = cell.exposed[main.targetDemographic];

                hoverChart.UpdateGraph(cellVals, cell.numberOfPeople[main.targetDemographic]);
            }
        }
    }

    //Get a pixel on the texture from a screen (commonly mouse) coordinate
    //Technically this function gets a texture coord an *any* quad clicked on
	public Vector2 getPixelFromScreenCoord(Vector2 coord) {
		RaycastHit hit;
		//Didn't hit anything
		if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			return Vector2.zero;

		
		Renderer rend = hit.transform.GetComponent<Renderer>();
		MeshCollider meshCollider = hit.collider as MeshCollider;

		Texture2D tex = (Texture2D)rend.material.mainTexture;

        //Prevents errors if we hit an untextured quad
        if (tex == null)
            return Vector2.zero;

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= tex.width;
		pixelUV.y *= tex.height;
		return new Vector2(pixelUV.x, pixelUV.y);
	}
}
