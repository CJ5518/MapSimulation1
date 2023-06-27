using UnityEngine;
using System.Collections.Generic;

//Base class for a movement model in the simulation
//The biggest of base classes known to man, but a lot of the models needed to be classes anyway so
public abstract class SimulationMovementModel {
	//Gets the number of zombies that the cell identified by the giver index should
	//give to the cell identified by the receiver index
	public abstract float getCellSpreadValue(int idxGiver, int idxReceiver, Simulation simulation);


	//Does basic adjustments to "amount" based on the data layers, same for every model anyway
	public float doBasicDataLayers(float amount, ref Simulation.Cell receiverCell, ref Simulation.Cell giverCell) {
		float road,water,height;

		water = amount * (-receiverCell.waterLevel * waterFactor);
		road = amount * (receiverCell.roadPercent * roadFactor);
		
		//If positive, means going up
		int elevationDiff = receiverCell.elevation - giverCell.elevation;

		height = amount * ((-elevationDiff/3000.0f) * roadFactor);

		amount += road;
		amount += water;
		amount += height;
		return amount;
	}

	public float waterFactor = 1.0f;
	public float heightFactor = 1.0f;
	public float roadFactor = 1.0f;

	//Instead of these two functions, maybe something like
	//movementModel.parameters or smthn

	//Return the names of the sliders that this movement model uses
	//i.e. editable parameters
	//Includes an option to give only the parameters that are 
	public abstract ParameterSliderSettings[] getSliderSettings(bool duringSimulation);

	//Update the movement model parameters given the values
	//The values are given in the same order they received from getSlierNames, which is why we need
	//the duringSimulation bool passed through here as well
	public abstract void updateSliderValues(float[] values, bool duringSimulation);
}
