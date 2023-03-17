using UnityEngine;

//Contains basic metadata of a compartment
public class CompartmentInfo {
	public string longName;
	public string shortName;
	public Color mapDisplayColor;

	//Default constructor
	public CompartmentInfo() {
		longName = "";
		shortName = "";
		mapDisplayColor = new Color(99.0f / 255.0f, 0, 86.0f / 255.0f);
	}
}

public class ParameterInfo {
	public string longName;
	public string shortName;
	public string description;

	//Default constructor
	public ParameterInfo() {
		longName = "";
		shortName = "";
		description = "";
	}
}

public class SimulationModel {
	//Counts of states and reactions (boxes and arrows)
	public int compartmentCount;
	public int reactionCount;
	public int parameterCount;
	//How does one reaction affect each compartment
	public int[,] stoichiometry;
	//The specifics of the compartments involved and their parameters for each reaction
	public int[][] propensityDetails;
	//Array of the parameters of the model
	public float[] parameters;
	//Info about the compartments
	public CompartmentInfo[] compartmentInfoArray;
	//Info about the parameters
	public ParameterInfo[] parameterInfoArray;

	//The starting state, kindof a dumb way to do this but needed in the short term
	public int startingStateIdx;
	//The state that gets people dropped into it, also kindof dumb but needed it in the short term
	public int droppingStateIdx;

	
	public SimulationModel(int compartmentCount, int reactionCount, int parameterCount, int startingStateIdx, int droppingStateIdx) {
		this.compartmentCount = compartmentCount;
		this.reactionCount = reactionCount;
		this.parameterCount = parameterCount;
		this.startingStateIdx = startingStateIdx;
		this.droppingStateIdx = droppingStateIdx;

		compartmentInfoArray = new CompartmentInfo[compartmentCount];
		for (int q = 0; q < compartmentCount; q++) {
			compartmentInfoArray[q] = new CompartmentInfo();
		}

		parameterInfoArray = new ParameterInfo[parameterCount];
		for (int q = 0; q < parameterCount; q++) {
			parameterInfoArray[q] = new ParameterInfo();
		}

		stoichiometry = new int[reactionCount, compartmentCount];
		propensityDetails = new int[reactionCount][];
		parameters = new float[parameterCount];
	}


}
