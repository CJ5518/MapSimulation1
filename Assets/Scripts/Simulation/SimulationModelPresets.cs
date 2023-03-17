using UnityEngine;



public static class SimulationModelPresets {
	public static SimulationModel getPreset(int idx) {
		switch (idx) {
			case 1:
			return basicSIR();
			case 2:
			return recovBackToSus();
			case 3:
			return newModel();
		}
		throw new System.Exception("Not a valid preset");
	}

	public static SimulationModel basicSIR() {
		SimulationModel ret;

		ret = new SimulationModel(3, 2, 2, 0, 1);
		//S
		ret.compartmentInfoArray[0].longName = "Susceptible";
		ret.compartmentInfoArray[0].shortName = "S";
		ret.compartmentInfoArray[0].mapDisplayColor = new Color(0.0f, 0, 1.0f);
		//I
		ret.compartmentInfoArray[1].longName = "Infected";
		ret.compartmentInfoArray[1].shortName = "I";
		ret.compartmentInfoArray[1].mapDisplayColor = new Color(1.0f, 0, 0.0f);
		//R
		ret.compartmentInfoArray[2].longName = "Recovered";
		ret.compartmentInfoArray[2].shortName = "R";
		ret.compartmentInfoArray[2].mapDisplayColor = new Color(0.0f, 1.0f, 0.0f);

		//S to I
		ret.propensityDetails[0] = new int[4] {1,0,1,1};
		ret.stoichiometry[0,0] = -1;
		ret.stoichiometry[0,1] = 1;
		ret.stoichiometry[0,2] = 0;

		//I to R
		ret.propensityDetails[1] = new int[3] {0,1,0};
		ret.stoichiometry[1,0] = 0;
		ret.stoichiometry[1,1] = -1;
		ret.stoichiometry[1,2] = 1;

		//Death rate
		ret.parameters[0] = 0.1f;
		ret.parameterInfoArray[0].longName = "Recovery rate";
		ret.parameterInfoArray[0].shortName = "σ";
		ret.parameterInfoArray[0].description = "Description for sigma";

		ret.parameters[1] = 1.0f;
		ret.parameterInfoArray[1].longName = "Infection rate";
		ret.parameterInfoArray[1].shortName = "β";
		ret.parameterInfoArray[1].description = "Description for beta";

		return ret;
	}

	public static SimulationModel recovBackToSus() {
		SimulationModel ret;

		ret = new SimulationModel(3, 3, 3, 0, 1);
		//S
		ret.compartmentInfoArray[0].longName = "Susceptible";
		ret.compartmentInfoArray[0].shortName = "S";
		ret.compartmentInfoArray[0].mapDisplayColor = new Color(0.0f, 0, 1.0f);
		//I
		ret.compartmentInfoArray[1].longName = "Infected";
		ret.compartmentInfoArray[1].shortName = "I";
		ret.compartmentInfoArray[1].mapDisplayColor = new Color(1.0f, 0, 0.0f);
		//R
		ret.compartmentInfoArray[2].longName = "Recovered";
		ret.compartmentInfoArray[2].shortName = "R";
		ret.compartmentInfoArray[2].mapDisplayColor = new Color(0.0f, 1.0f, 0.0f);

		//S to I
		ret.propensityDetails[0] = new int[4] {1,0,1,1};
		ret.stoichiometry[0,0] = -1;
		ret.stoichiometry[0,1] = 1;
		ret.stoichiometry[0,2] = 0;

		//I to R
		ret.propensityDetails[1] = new int[3] {0,1,0};
		ret.stoichiometry[1,0] = 0;
		ret.stoichiometry[1,1] = -1;
		ret.stoichiometry[1,2] = 1;

		//R to S
		ret.propensityDetails[2] = new int[3] {0,2,2};
		ret.stoichiometry[2,0] = 1;
		ret.stoichiometry[2,1] = 0;
		ret.stoichiometry[2,2] = -1;

		//Recov rate rate
		ret.parameters[0] = 0.3f;
		ret.parameterInfoArray[0].longName = "Recovery rate";
		ret.parameterInfoArray[0].shortName = "Q";
		ret.parameterInfoArray[0].description = "Description for Q param recov rate";
		//Spread rate
		ret.parameters[1] = 1.0f;
		ret.parameterInfoArray[1].longName = "Infection rate";
		ret.parameterInfoArray[1].shortName = "β";
		ret.parameterInfoArray[1].description = "Description for beta";
		//Recov back to sus from infected rate
		ret.parameters[2] = 0.01f;
		ret.parameterInfoArray[2].longName = "Back to sus rate";
		ret.parameterInfoArray[2].shortName = "Sreturn";
		ret.parameterInfoArray[2].description = "Description for sus return";

		return ret;
	}

	public static SimulationModel newModel() {
		SimulationModel ret;

		ret = new SimulationModel(6, 7, 7, 0, 2);
		ret.propensityDetails[0] = new int[3] {0,2,0};
		ret.propensityDetails[1] = new int[3] {0,2,1};
		ret.propensityDetails[2] = new int[3] {0,3,2};
		ret.propensityDetails[3] = new int[3] {0,0,3};
		ret.propensityDetails[4] = new int[3] {0,5,4};
		ret.propensityDetails[5] = new int[3] {0,1,5};
		ret.propensityDetails[6] = new int[4] {1,0,2,6};
		/*
		0 - Sus
		1 - Exposed
		2 - Infect
		3 - Recov
		4 - Dead
		5 - Vacc
		*/

		//S
		ret.compartmentInfoArray[0].longName = "Susceptible";
		ret.compartmentInfoArray[0].shortName = "S";
		ret.compartmentInfoArray[0].mapDisplayColor = new Color(90 / 255.0f, 150 / 255.0f, 255 / 255.0f);
		//E
		ret.compartmentInfoArray[1].longName = "Exposed";
		ret.compartmentInfoArray[1].shortName = "E";
		ret.compartmentInfoArray[1].mapDisplayColor = new Color(1.0f, 1.0f, 0.1f);
		//I
		ret.compartmentInfoArray[2].longName = "Infected";
		ret.compartmentInfoArray[2].shortName = "I";
		ret.compartmentInfoArray[2].mapDisplayColor = new Color(1.0f, 0, 0.0f);
		//R
		ret.compartmentInfoArray[3].longName = "Recovered";
		ret.compartmentInfoArray[3].shortName = "R";
		ret.compartmentInfoArray[3].mapDisplayColor = new Color(1.0f, 1.0f, 1.0f);
		//D
		ret.compartmentInfoArray[4].longName = "Dead";
		ret.compartmentInfoArray[4].shortName = "D";
		ret.compartmentInfoArray[4].mapDisplayColor = new Color(0,0,0.0f, 1.0f);
		//V
		ret.compartmentInfoArray[5].longName = "Vacc";
		ret.compartmentInfoArray[5].shortName = "V";
		ret.compartmentInfoArray[5].mapDisplayColor = new Color(102 / 255.0f, 3 / 255.0f, 252 / 255.0f);

		ret.parameters[0] = 0.05f;
		ret.parameterInfoArray[0].longName = "Inf->Dead";
		ret.parameterInfoArray[0].shortName = "D";
		ret.parameterInfoArray[0].description = "Infected to dead";
		ret.parameters[1] = 0.2f;
		ret.parameterInfoArray[1].longName = "Inf->Recov";
		ret.parameterInfoArray[1].shortName = "R";
		ret.parameterInfoArray[1].description = "Infected to recovered";
		ret.parameters[2] = 0.01f;
		ret.parameterInfoArray[2].longName = "Recov->Sus";
		ret.parameterInfoArray[2].shortName = "S";
		ret.parameterInfoArray[2].description = "Recovered back sus";
		ret.parameters[3] = 0.0f;
		ret.parameterInfoArray[3].longName = "Sus->Vacc";
		ret.parameterInfoArray[3].shortName = "V";
		ret.parameterInfoArray[3].description = "Recovered back sus";
		ret.parameters[4] = 0.0f;
		ret.parameterInfoArray[4].longName = "Vacc->Sus";
		ret.parameterInfoArray[4].shortName = "V-S";
		ret.parameterInfoArray[4].description = "Recovered back sus";
		ret.parameters[5] = 0.7f;
		ret.parameterInfoArray[5].longName = "Expo->Inf";
		ret.parameterInfoArray[5].shortName = "alpha";
		ret.parameterInfoArray[5].description = "Recovered back sus";
		ret.parameters[6] = 1.0f;
		ret.parameterInfoArray[6].longName = "Sus->Expo";
		ret.parameterInfoArray[6].shortName = "Beta";
		ret.parameterInfoArray[6].description = "Recovered back sus";

		ret.stoichiometry[0,0] = 0;
		ret.stoichiometry[0,1] = 0;
		ret.stoichiometry[0,2] = -1;
		ret.stoichiometry[0,3] = 0;
		ret.stoichiometry[0,4] = 1;
		ret.stoichiometry[0,5] = 0;
		ret.stoichiometry[1,0] = 0;
		ret.stoichiometry[1,1] = 0;
		ret.stoichiometry[1,2] = -1;
		ret.stoichiometry[1,3] = 1;
		ret.stoichiometry[1,4] = 0;
		ret.stoichiometry[1,5] = 0;
		ret.stoichiometry[2,0] = 1;
		ret.stoichiometry[2,1] = 0;
		ret.stoichiometry[2,2] = 0;
		ret.stoichiometry[2,3] = -1;
		ret.stoichiometry[2,4] = 0;
		ret.stoichiometry[2,5] = 0;
		ret.stoichiometry[3,0] = -1;
		ret.stoichiometry[3,1] = 0;
		ret.stoichiometry[3,2] = 0;
		ret.stoichiometry[3,3] = 0;
		ret.stoichiometry[3,4] = 0;
		ret.stoichiometry[3,5] = 1;
		ret.stoichiometry[4,0] = 1;
		ret.stoichiometry[4,1] = 0;
		ret.stoichiometry[4,2] = 0;
		ret.stoichiometry[4,3] = 0;
		ret.stoichiometry[4,4] = 0;
		ret.stoichiometry[4,5] = -1;
		ret.stoichiometry[5,0] = 0;
		ret.stoichiometry[5,1] = -1;
		ret.stoichiometry[5,2] = 1;
		ret.stoichiometry[5,3] = 0;
		ret.stoichiometry[5,4] = 0;
		ret.stoichiometry[5,5] = 0;
		ret.stoichiometry[6,0] = -1;
		ret.stoichiometry[6,1] = 1;
		ret.stoichiometry[6,2] = 0;
		ret.stoichiometry[6,3] = 0;
		ret.stoichiometry[6,4] = 0;
		ret.stoichiometry[6,5] = 0;

		return ret;
	}
}