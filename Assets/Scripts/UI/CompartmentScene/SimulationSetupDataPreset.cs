using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SimulationSetupDataPreset : ScriptableObject
{
	//S to E
	[Header("S to E")]
	public float contactRate = 0.0f;
	public int confidenceContact = 0;
	public float infectionProbability = 0.0f;
	public int confidenceInfectionProb = 0;

	//E to I
	[Header("E to I")]
	public float latencyEI = 0.0f;
	public int confidenceEI = 0;

	//I to R
	[Header("I to R")]
	public float recoveryRate = 0.0f;
	public int confidenceIR = 0;

	//S to V
	[Header("S to V")]
	public float vaccinationAvailability = 0.0f;
	public int confidenceAvailability = 0;
	public float vaccinationUptake = 0.0f;
	public int confidenceUptake = 0;

	//V to E
	[Header("V to E")]
	public float breakthroughRate = 0.0f;
	public int confidenceVE = 0;

	//I to D
	[Header("I to D")]
	public float mortalityRate = 0.0f;
	public int confidenceID = 0;

	[Header("I to other")]
	public float infectionTime = 0.0f;

	//R to S
	[Header("R to S")]
	public float resusceptibilityLatency = 0.0f;
	public int confidenceRS = 0;

	#region Layers

	public float movementModel = 0.0f;

	public float diseaseImmobilization = 0.0f;
	public int confidenceDiseaseImmobilization = 0;

	public float airplaneSecurityAgainstExposed = 0.0f;
	public int confidenceAirplaneSecurityAgainstExposed = 0;

	public float airplaneSecurityAgainstInfected = 0.0f;
	public int confidenceAirplaneSecurityAgainstInfected = 0;

	//Get elated
	public float elevation = 0.0f;
	public int confidenceElevation = 0;

	public float highway = 0.0f;
	public int confidenceHighway = 0;

	public float water = 0.0f;
	public int confidenceWater = 0;

	public float incomeInfectionRate = 0.0f;
	public int confidenceIncomeInfectionRate = 0;

	public float incomeVaccineEffectiveness = 0.0f;
	public int confidenceIncomeVaccineEffectiveness = 0;

	public float incomeDiseaseOnsetTime = 0.0f;
	public int confidenceIncomeDiseaseOnsetTime = 0;

	public float incomeMortalityRate = 0.0f;
	public int confidenceIncomeMortalityRate = 0;

	public float incomeWaningImmunity = 0.0f;
	public int confidenceIncomeWaningImmunity = 0;

	public float vaccinationHesitancy = 0.0f;
	public int confidenceVaccinationHesitancy = 0;

	#endregion Layers

	#region Predictions

	public float PredictionInfectionLength = 0.0f;
	public float PredictionInfectionCount = 0.0f;
	public float PredictionDeadCount = 0.0f;
	public string PredictionHomeState = "ID";
	public bool PredictionAffectHomeState = false;

	#endregion Predictions

}
