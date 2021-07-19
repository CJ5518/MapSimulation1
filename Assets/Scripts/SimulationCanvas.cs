using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SimulationCanvas : MonoBehaviour
{
    #region Refernces Vars
    public Main main;

    const float statisticsUpdatesPerSecond = 20.0f;
    float lastStatsTime = -100.0f;


    [Header("Canvas Objects")]
    public PieChart overallChart;
    public Text statisticsEditLabel;

    //Slider/text combos
    public SliderTextCombo alphaSliderText;
    public SliderTextCombo gammaSliderText;
    public SliderTextCombo spreadRateSliderText;
    public SliderTextCombo sigmaSliderText;
    public SliderTextCombo deltaSliderText;

    public Slider contactProbabilitySlider;
    public Slider infectionRateSlider;
    public Text betaText;

    public Text r0Text;

    //Toggles
    public Toggle drawInfectedToggle;
    public Toggle drawRecoveredToggle;
    public Toggle drawDeadToggle;
    public Toggle moveZombiesToggle;

    public Toggle drawProportionToggle;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateCanvas()
    {
        //r0
        r0Text.text = "r0: " + (main.simulation.data.beta / main.simulation.data.gamma).ToString("f2");

        //Alpha, beta, gamma, etc.
        main.simulation.data.beta = infectionRateSlider.value * contactProbabilitySlider.value;

        main.simulation.data.alpha = alphaSliderText.slider.value;
        main.simulation.data.gamma = gammaSliderText.slider.value;
        main.simulation.data.sigma = sigmaSliderText.slider.value;
        main.simulation.data.delta = deltaSliderText.slider.value;
        main.simulation.data.spreadRate = spreadRateSliderText.slider.value;

        //Toggles
        main.simulation.data.drawRecovered = drawRecoveredToggle.isOn;
        main.simulation.data.drawInfected = drawInfectedToggle.isOn;
        main.simulation.data.drawDead = drawDeadToggle.isOn;

        main.simulation.data.drawProportion = drawProportionToggle.isOn;
        main.simulation.data.moveZombies = moveZombiesToggle.isOn;
    }


    //Updates the statistics label based on the pixel the mouse is over, and targetDemographic
    int lastIndex = -1;
    public unsafe void updateStatisticsLabel() {
        float totalSusceptible = 0.0f;
        float totalInfected = 0.0f;
        float totalRecovered = 0.0f;
        float totalExposed = 0.0f;
        float totalVaccinated = 0.0f;
        double totalPeople = 0.0;

        //Pixel coord on the draw texture
        Vector2 pixel = main.backgroundMovableImage.getPixelFromScreenCoord(Input.mousePosition);
        int index = main.simulation.coordToIndex(pixel);

        if (!main.simulation.cellIsValid(index) && main.simulation.cellIsValid(lastIndex))
            index = lastIndex;

        if (main.simulation.cellIsValid(index)) {
            lastIndex = index;
            Simulation.Cell cell = main.simulation.readCells[index];

            //Gather statistics for the entire thing
            if (Time.realtimeSinceStartup - lastStatsTime >= 1.0f / statisticsUpdatesPerSecond) {
                lastStatsTime = Time.realtimeSinceStartup;
                totalSusceptible = 0;
                totalInfected = 0;
                totalRecovered = 0;
                totalExposed = 0;
                totalPeople = 0;
                totalVaccinated = 0;
                for (int q = 0; q < main.simulation.readCells.Length; q++) {
                    Simulation.Cell readCell = main.simulation.readCells[q];
                    totalSusceptible += readCell.susceptible[main.targetDemographic];
                    totalInfected += readCell.infected[main.targetDemographic];
                    totalRecovered += readCell.recovered[main.targetDemographic];
                    totalExposed += readCell.exposed[main.targetDemographic];
                    totalVaccinated += readCell.vaccinated[main.targetDemographic];
                    totalPeople += (double)readCell.numberOfPeople[main.targetDemographic];
                }

                //Set the string to the statistics
                string finalString =
                    ((Population)main.targetDemographic).ToString() + "\n" +
                    cell.susceptible[main.targetDemographic].ToString("F3") + "\n" +
                    cell.vaccinated[main.targetDemographic].ToString("F3") + "\n" +
                    cell.infected[main.targetDemographic].ToString("F3") + "\n" +
                    cell.recovered[main.targetDemographic].ToString("F3") + "\n" +
                    cell.exposed[main.targetDemographic].ToString("F3") + "\n" +
                    "Totals:" + "\n" +
                    totalSusceptible.ToString("F3") + "\n" +
                    totalVaccinated.ToString("F3") + "\n" +
                    totalInfected.ToString("F3") + "\n" +
                    totalRecovered.ToString("F3") + "\n" +
                    totalExposed.ToString("F3") + "\n";
                statisticsEditLabel.text = finalString;
            }
        }
    }
}
