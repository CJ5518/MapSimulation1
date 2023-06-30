using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CompartmentDataManager : MonoBehaviour
{
	public List<SimulationSetupDataPreset> presets = null;

	// Start is called before the first frame update
	void Start()
	{
		CompartmentEvents.OnQuestionValueChange += SetData;
		CompartmentEvents.OnConfidenceChange += SetConfidence;
		CompartmentEvents.OnLoadPreset += LoadPreset;
		LoadPreset(0);
	}

	private void SetData(CompartmentEnum.Questions question, float value)
	{
		//Logger.Log($"SetData {question.ToString()}, {value}");
		switch(question)
		{
			case CompartmentEnum.Questions.ContactRate:
			{
				SimulationSetupData.contactRate = value;
				return;
			}
			case CompartmentEnum.Questions.InfectionProbability:
			{
				SimulationSetupData.infectionProbability = value;
				return;
			}
			case CompartmentEnum.Questions.EILatency:
			{
				SimulationSetupData.latencyEI = value;
				return;
			}
			case CompartmentEnum.Questions.RecoveryRate:
			{
					if (SimulationSetupData.recoveryRate == value) return;
				SimulationSetupData.recoveryRate = value;
					if(SimulationSetupData.recoveryRate != (1 - SimulationSetupData.mortalityRate))
					{
						CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.MortalityRate, 1 - SimulationSetupData.recoveryRate);
					}
				return;
			}
			case CompartmentEnum.Questions.VaccinationDistribution:
			{
				SimulationSetupData.vaccinationAvailability = value;
				return;
			}
			case CompartmentEnum.Questions.VaccinationUptake:
			{
				SimulationSetupData.vaccinationUptake = value;
				return;
			}
			case CompartmentEnum.Questions.BreakthroughRate:
			{
				SimulationSetupData.breakthroughRate = value;
				return;
			}
			case CompartmentEnum.Questions.MortalityRate:
			{
					if (SimulationSetupData.mortalityRate == value) return;
					SimulationSetupData.mortalityRate = value;
					if (SimulationSetupData.mortalityRate != (1 - SimulationSetupData.recoveryRate))
					{
						CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.RecoveryRate, 1 - SimulationSetupData.mortalityRate);
					}
					return;
			}
			case CompartmentEnum.Questions.WaningImmunityRate:
			{
				SimulationSetupData.waningImmunity = value;
				return;
			}
			case CompartmentEnum.Questions.MovementModel:
				{
					SimulationSetupData.movementModel = (int)value;
					return;
				}
			case CompartmentEnum.Questions.DiseaseImmobilization:
				{
					SimulationSetupData.diseaseImmobilization = value;
					return;
				}
			case CompartmentEnum.Questions.AirplaneSecurityAgainstExposed:
				{
					SimulationSetupData.airplaneSecurityAgainstExposed = value;
					return;
				}
			case CompartmentEnum.Questions.AirplaneSecurityAgainstInfected:
				{
					SimulationSetupData.airplaneSecurityAgainstInfected = value;
					return;
				}
			case CompartmentEnum.Questions.Elevation:
				{
					SimulationSetupData.elevation = value;
					return;
				}
			case CompartmentEnum.Questions.Highways:
				{
					SimulationSetupData.highway = value;
					return;
				}
			case CompartmentEnum.Questions.Water:
				{
					SimulationSetupData.water = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeInfectionRate:
				{
					SimulationSetupData.incomeInfectionRate = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeVaccineEffectiveness:
				{
					SimulationSetupData.incomeVaccineEffectiveness = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeDiseaseOnsetTime:
				{
					SimulationSetupData.incomeDiseaseOnsetTime = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeMortalityRate:
				{
					SimulationSetupData.incomeMortalityRate = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeWaningImmunity:
				{
					SimulationSetupData.incomeWaningImmunity = value;
					return;
				}
			case CompartmentEnum.Questions.VaccinationHesitancy:
				{
					SimulationSetupData.vaccinationHesitancy = value;
					return;
				}
			case CompartmentEnum.Questions.PredictionInfectionLength:
				{
					SimulationSetupData.PredictionInfectionLength = value;
					return;
				}
			case CompartmentEnum.Questions.PredictionInfectionCount:
				{
					SimulationSetupData.PredictionInfectionCount = value;
					return;
				}
			case CompartmentEnum.Questions.PredictionDeadCount:
				{
					SimulationSetupData.PredictionDeadCount = value;
					return;
				}
			case CompartmentEnum.Questions.PredictionHomeState:
				{
					List<string> alphabeticalList = new List<string>(SimulationManager.stats.stateNames);
					alphabeticalList.Sort();
					SimulationSetupData.PredictionHomeState = alphabeticalList[(int)value];
					return;
				}
			case CompartmentEnum.Questions.PredictionAffectHomeState:
				{
					SimulationSetupData.PredictionAffectHomeState = (value == 0.0f);
					return;
				}
			case CompartmentEnum.Questions.InfectionDuration:
			{
				SimulationSetupData.infectionLength = value;
				return;
			}

			default:
			{
					Logger.LogError("CompartmentDataManager.SetData switch default");
					return;
			}
		}
	}

	private void SetConfidence(CompartmentEnum.Questions question, int value)
	{
		//Logger.Log($"SetData {question.ToString()}, {value}");
		switch (question)
		{
			case CompartmentEnum.Questions.ContactRate:
				{
					SimulationSetupData.confidenceContact = value;
					return;
				}
			case CompartmentEnum.Questions.InfectionProbability:
				{
					SimulationSetupData.confidenceInfectionProb = value;
					return;
				}
			case CompartmentEnum.Questions.EILatency:
				{
					SimulationSetupData.confidenceEI = value;
					return;
				}
			case CompartmentEnum.Questions.RecoveryRate:
				{
					SimulationSetupData.confidenceIR = value;
					return;
				}
			case CompartmentEnum.Questions.VaccinationDistribution:
				{
					SimulationSetupData.confidenceAvailability = value;
					return;
				}
			case CompartmentEnum.Questions.VaccinationUptake:
				{
					SimulationSetupData.confidenceUptake = value;
					return;
				}
			case CompartmentEnum.Questions.BreakthroughRate:
				{
					SimulationSetupData.confidenceVE = value;
					return;
				}
			case CompartmentEnum.Questions.MortalityRate:
				{
					SimulationSetupData.confidenceID = value;
					
					return;
				}
			case CompartmentEnum.Questions.WaningImmunityRate:
				{
					SimulationSetupData.confidenceRS = value;
					return;
				}
			case CompartmentEnum.Questions.DiseaseImmobilization:
				{
					SimulationSetupData.confidenceDiseaseImmobilization = value;
					return;
				}
			case CompartmentEnum.Questions.AirplaneSecurityAgainstExposed:
				{
					SimulationSetupData.confidenceAirplaneSecurityAgainstExposed = value;
					return;
				}
			case CompartmentEnum.Questions.AirplaneSecurityAgainstInfected:
				{
					SimulationSetupData.confidenceAirplaneSecurityAgainstInfected = value;
					return;
				}
			case CompartmentEnum.Questions.Elevation:
				{
					SimulationSetupData.confidenceElevation = value;
					return;
				}
			case CompartmentEnum.Questions.Highways:
				{
					SimulationSetupData.confidenceHighway = value;
					return;
				}
			case CompartmentEnum.Questions.Water:
				{
					SimulationSetupData.confidenceWater = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeInfectionRate:
				{
					SimulationSetupData.confidenceIncomeInfectionRate = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeVaccineEffectiveness:
				{
					SimulationSetupData.confidenceIncomeVaccineEffectiveness = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeDiseaseOnsetTime:
				{
					SimulationSetupData.confidenceIncomeDiseaseOnsetTime = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeMortalityRate:
				{
					SimulationSetupData.confidenceIncomeMortalityRate = value;
					return;
				}
			case CompartmentEnum.Questions.IncomeWaningImmunity:
				{
					SimulationSetupData.confidenceIncomeWaningImmunity = value;
					return;
				}
			case CompartmentEnum.Questions.VaccinationHesitancy:
				{
					SimulationSetupData.confidenceVaccinationHesitancy = value;
					return;
				}
			default:
				{
					Logger.LogError("CompartmentDataManager.SetData switch default");
					return;
				}
		}
	}

	public void PrintData()
	{
		Logger.Log($"Deterministic? {SimulationSetupData.useStochasticModel} \n" +
			$"Movement Model, {(SimulationSetupData.movementModel == 0 ? "Gravity Model" : "Custom Model")}\n" +
			$"Water, {SimulationSetupData.waterFactor}, {SimulationSetupData.confidenceElevation}\n" +
			$"Highway, {SimulationSetupData.roadFactor}, {SimulationSetupData.confidenceHighway}\n" +
			$"Elevation, {SimulationSetupData.heightFactor}, {SimulationSetupData.confidenceWater}\n" +
			$"Alpha, {SimulationSetupData.alpha}\n" +
			$"Beta, {SimulationSetupData.beta}\n" +
			$"SpreadRate, {SimulationSetupData.spreadRate}\n" +
			$"Done");
	}

	public void CSVData()
	{
		SimulationSetupData.WriteToCSV();
	}

	public void LoadPreset(int index)
	{
		//if (index == 0) return;
		if(index > presets.Count)
		{
			Logger.LogError("DataManager.LoadPreset: index out of bounds");
			return;
		}

		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.ContactRate, presets[index].contactRate);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.InfectionProbability, presets[index].infectionProbability);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.EILatency, presets[index].latencyEI);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.RecoveryRate, presets[index].recoveryRate);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.VaccinationDistribution, presets[index].vaccinationAvailability);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.VaccinationUptake, presets[index].vaccinationUptake);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.BreakthroughRate, presets[index].breakthroughRate);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.MortalityRate, presets[index].mortalityRate);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.WaningImmunityRate, presets[index].resusceptibilityLatency);
		//CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.MovementModel, presets[index].movementModel);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.DiseaseImmobilization, presets[index].diseaseImmobilization);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.AirplaneSecurityAgainstExposed, presets[index].airplaneSecurityAgainstExposed);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.AirplaneSecurityAgainstInfected, presets[index].airplaneSecurityAgainstInfected);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.Elevation, presets[index].elevation);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.Highways, presets[index].highway);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.Water, presets[index].water);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.IncomeInfectionRate, presets[index].incomeInfectionRate);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.IncomeVaccineEffectiveness, presets[index].incomeVaccineEffectiveness);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.IncomeDiseaseOnsetTime, presets[index].incomeDiseaseOnsetTime);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.IncomeMortalityRate, presets[index].incomeMortalityRate);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.IncomeWaningImmunity, presets[index].incomeWaningImmunity);
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.VaccinationHesitancy, presets[index].vaccinationHesitancy);

		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.InfectionDuration, presets[index].infectionTime);

		/*
		 * Temporary for the light demo
		 */
		Logger.Log("Here's a reminder that you've modified the preset functionality. When you try to plug everything back in, change this.");
		CompartmentEvents.SetNewQuestionValue(CompartmentEnum.Questions.MovementModel, 1f);

		//index--; //Offset for array
		//SimulationSetupData.contactRate = presets[index].contactRate;
		//SimulationSetupData.infectionProbability = presets[index].infectionProbability;
		//SimulationSetupData.latencyEI = presets[index].latencyEI;
		//SimulationSetupData.recoveryRate = presets[index].recoveryRate;
		//SimulationSetupData.vaccinationAvailability = presets[index].vaccinationAvailability;
		//SimulationSetupData.vaccinationUptake = presets[index].vaccinationUptake;
		//SimulationSetupData.breakthroughRate = presets[index].breakthroughRate;
		//SimulationSetupData.mortalityRate = presets[index].mortalityRate;
		//SimulationSetupData.resusceptibilityLatency = presets[index].resusceptibilityLatency;

	}

	public void LoadScene()
	{
		SceneManager.LoadScene(1);
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Q)) {
			PrintData();
		}
	}
}
