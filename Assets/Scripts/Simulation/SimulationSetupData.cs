using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class SimulationSetupData {
	//Is set to true if we enter the guided script scene, if we don't, this will stay false
	//And we'll use some simulation defaults
	public static bool useTheseNumbers = false;

	//Use the stochastic model or no?
	//Tau leaping, gillespie, etc.
	public static bool useStochasticModel = false;

    #region Parameters
    //S to E
    public static float contactRate = 0.0f;
	public static int confidenceContact = 0;
	public static float infectionProbability = 0.0f;
	public static int confidenceInfectionProb = 0;

	//E to I
	public static float latencyEI = 0.0f;
	public static int confidenceEI = 0;

	//I to R
	public static float recoveryRate = 0.0f;
	public static int confidenceIR = 0;

	//S to V
	public static float vaccinationAvailability = 0.0f;
	public static int confidenceAvailability = 0;
	//Maybe don't worry about this one
	public static float vaccinationUptake = 0.0f;
	public static int confidenceUptake = 0;

	//V to E
	public static float breakthroughRate = 0.0f;
	public static int confidenceVE = 0;

	//I to D
	public static float mortalityRate = 0.0f;
	public static int confidenceID = 0;

	//I to other
	public static float infectionLength;

	//R to S
	public static float waningImmunity = 0.0f;
	public static int confidenceRS = 0;

	#endregion Parameters

	#region Layers

	public static int movementModel = 0;

	public static float diseaseImmobilization = 0.0f;
	public static int confidenceDiseaseImmobilization = 0;

	public static float airplaneSecurityAgainstExposed = 0.0f;
	public static int confidenceAirplaneSecurityAgainstExposed = 0;

	public static float airplaneSecurityAgainstInfected = 0.0f;
	public static int confidenceAirplaneSecurityAgainstInfected = 0;

	public static bool enableAirports = true;

	//Get elated
	//Thank you Ryan, I now feel elated
	public static float elevation = 0.0f;
	public static int confidenceElevation = 0;

	public static float highway = 0.0f;
	public static int confidenceHighway = 0;

	public static float water = 0.0f;
	public static int confidenceWater = 0;

	public static float incomeInfectionRate = 2.0f;
	public static int confidenceIncomeInfectionRate = 0;

	public static float incomeVaccineEffectiveness = 2.0f;
	public static int confidenceIncomeVaccineEffectiveness = 0;

	public static float incomeDiseaseOnsetTime = 2.0f;
	public static int confidenceIncomeDiseaseOnsetTime = 0;

	public static float incomeMortalityRate = 2.0f;
	public static int confidenceIncomeMortalityRate = 0;

	public static float incomeWaningImmunity = 2.0f;
	public static int confidenceIncomeWaningImmunity = 0;

	public static float vaccinationHesitancy = 0.0f;
	public static int confidenceVaccinationHesitancy = 0;

	#endregion Layers

	#region Predictions

	public static float PredictionInfectionLength = 0.0f;
	public static float PredictionInfectionCount = 0.0f;
	public static float PredictionDeadCount = 0.0f;
	public static string PredictionHomeState = "ID";
	public static bool PredictionAffectHomeState = false;

	#endregion Predictions

	public static void WriteToCSV()
    {
		var csv = new StringBuilder();

		csv.AppendLine($"Question Title, Rate, Confidence");
		csv.AppendLine($"ContactRate, {contactRate}, {confidenceContact}");
		csv.AppendLine($"Infection Probability, {infectionProbability}, {confidenceInfectionProb}");
		csv.AppendLine($"Infection Latency, {latencyEI}, {confidenceEI}");
		csv.AppendLine($"Recovery Rate, {recoveryRate}, {confidenceIR}");
		csv.AppendLine($"Vaccination Availability, {vaccinationAvailability}, {confidenceAvailability}");
		csv.AppendLine($"Vaccination Uptake, {vaccinationUptake}, {confidenceUptake}");
		csv.AppendLine($"Breakthrough Rate, {breakthroughRate}, {confidenceVE}");
		csv.AppendLine($"Mortality Rate, {mortalityRate}, {confidenceID}");
		csv.AppendLine($"Waning Immunity Rate, {waningImmunity}, {confidenceRS}");
		csv.AppendLine($"Movement Model, {(movementModel == 0f ? "Gravity Model" : "Custom Model")}");
		csv.AppendLine($"Disease Immobilization, {diseaseImmobilization}, {confidenceDiseaseImmobilization}");
		csv.AppendLine($"Airplane Security Against Exposed, {airplaneSecurityAgainstExposed}, {confidenceAirplaneSecurityAgainstExposed}");
		csv.AppendLine($"Airplane Security Against Infected, {airplaneSecurityAgainstInfected}, {confidenceAirplaneSecurityAgainstInfected}");
		csv.AppendLine($"Elevation, {elevation}, {confidenceElevation}");
		csv.AppendLine($"Highway, {highway}, {confidenceHighway}");
		csv.AppendLine($"Water, {water}, {confidenceWater}");
		csv.AppendLine($"Income Infection Rate, {incomeInfectionRate}, {confidenceIncomeInfectionRate}");
		csv.AppendLine($"Income Vaccine Effectiveness, {incomeVaccineEffectiveness}, {confidenceIncomeVaccineEffectiveness}");
		csv.AppendLine($"Income Disease Onset Time, {incomeDiseaseOnsetTime}, {confidenceIncomeDiseaseOnsetTime}");
		csv.AppendLine($"Income Mortality Rate, {incomeMortalityRate}, {confidenceIncomeMortalityRate}");
		csv.AppendLine($"Income Waning Immunity, {incomeWaningImmunity}, {confidenceIncomeWaningImmunity}");
		csv.AppendLine($"Vaccination Hesistancy, {vaccinationHesitancy}, {confidenceVaccinationHesitancy}");

		//File.WriteAllText(@"D:\Desktop\test.csv", csv.ToString());
	}

}
