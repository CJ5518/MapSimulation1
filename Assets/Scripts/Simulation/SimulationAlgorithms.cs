using UnityEngine;

//Holds the state
public struct DiseaseState {
	public int[] state;
	public float dt;

	public DiseaseState(int stateCount) {
		state = new int[stateCount];
		dt = 0;
		
		setToZero();
	}

	public DiseaseState(DiseaseState other) {
		state = new int[other.stateCount];
		dt = other.dt;
		for (int q = 0; q< other.stateCount; q++) {
			state[q] = other.state[q];
		}
	}

	public int stateCount {
		get {
			return state.Length;
		}
	}

	//Get the sum
	public int numberOfPeople {
		get {
			//A for loop would be more general, but are we really going to change the number of states?
			//As it would turn out, yes
			int ret = 0;
			for (int q = 0; q < stateCount; q++) {
				ret += state[q];
			}
			return ret;
		}
	}

	public void setToZero() {
		for (int q = 0; q < stateCount; q++) {
			state[q] = 0;
		}
		dt = 0;
	}
}

//https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe
public static class ThreadSafeRandom {
    private static readonly System.Random _global = new System.Random();
    [System.ThreadStatic] private static System.Random _local;

    public static int Next() {
        if (_local == null) {
            int seed;
            lock (_global) {
                seed = _global.Next();
            }
            _local = new System.Random(seed);
        }
        return _local.Next();
    }

	public static double NextDouble() {
        if (_local == null) {
            int seed;
            lock (_global) {
                seed = _global.Next();
            }
            _local = new System.Random(seed);
        }
		return _local.NextDouble();
	}
}

public static class SimulationAlgorithms {
	public delegate float PropensityFunctionTypes(ref DiseaseState state, ref SimulationModel model, int[] argv);
	public const int propensityFunctionTypeCount = 2;

	public static PropensityFunctionTypes[] propensityFuncTypes;

	//When railroading dt with tau leaping, how much are we willing to raise tau to meet the railroad demand
	//Also basically a "minimum" amount, you probably shouldn't lower from .1
	const float maxTauRaiseAmount = 0.1f;

	//Static initializer
	static SimulationAlgorithms() {
		//Set up propensity functions
		propensityFuncTypes = new PropensityFunctionTypes[propensityFunctionTypeCount];

		//PLEASE FOR THE LOVE OF GOD
		//Before editing these please look for the DIRTY trick in the HOR function which assumes that the order 1 reaction
		//is on idx 0 and the order 2 reaction is on idx 1
		//It occurs to me that we abandoned that route, instead just hardcoding a tau value
		
		//Basic type, state (idx 1) * param (idx 2)
		propensityFuncTypes[0] = (ref DiseaseState state, ref SimulationModel model, int[] argv) => {
			return (float)state.state[argv[1]] * model.parameters[argv[2]];
		};

		//Grey arrow, page 16 of the book, thing that depends on the density of infected
		// (param * state1 * state2) / NumberOfPeopleInState
		// (idx3 * idx2 * idx1) / Num
		propensityFuncTypes[1] = (ref DiseaseState state, ref SimulationModel model, int[] argv) => {
			return model.parameters[argv[3]] * ((state.state[argv[2]] * (float)state.state[argv[1]]) / (float)state.numberOfPeople);
		};
	}

	//Runs the correct propensity function given the magic numbers to describe it
	public static float dispatchPropensityFunction(ref DiseaseState state, ref SimulationModel model, int[] argv) {
		return propensityFuncTypes[argv[0]](ref state, ref model, argv);
	}



	//Actual disease mathematics


	public static unsafe DiseaseState basicTick(DiseaseState state, ref SimulationModel model, float dt) {
		//Don't do anything for 0 people
		if (state.numberOfPeople == 0) return state;
		DiseaseState writeState = new DiseaseState(state);
		for (int q = 0; q < model.reactionCount; q++) {
			float res = dispatchPropensityFunction(ref writeState, ref model, model.propensityDetails[q]) * dt;
			for (int stoich = 0; stoich < model.compartmentCount; stoich++) {
				writeState.state[stoich] += (int)(model.stoichiometry[q, stoich] * res);
			}
		}
		writeState.dt += dt;
		return writeState;
	}

	//https://rosettacode.org/wiki/Statistics/Normal_distribution#Lua
	//Returns a normal random variable with mean and variance^2
	public static float gaussian(float mean, float variance) {
		return Mathf.Sqrt(-2 * variance * Mathf.Log((float)ThreadSafeRandom.NextDouble())) *
				Mathf.Cos(2 * Mathf.PI * (float)ThreadSafeRandom.NextDouble()) + mean;
	}
	public static int poissonNumber(float lambda) {
		//the Poisson random variable P(a,t) will, when at >= 1, 
		//be well approximated by a normal random variable with the same mean and variance

		int n = (int)(gaussian(lambda, lambda) + 0.5f);


		return n < 0 ? 0 : n;
	}

	//The two hat funcs, encapsulated into one
	//Set sigma to true to use the sigma function
	//https://aip.scitation.org/doi/10.1063/1.2159468
	public static float hatFuncs(int stateIdx, bool isSigmaFunc, ref DiseaseState lambdaState, ref SimulationModel model) {
		float sum = 0;
		for (int q = 0; q < model.propensityDetails.GetLength(0); q++) {
			int stoich = model.stoichiometry[q,stateIdx];
			//Pretty much just absolute value in our case, but the paper says square it
			if (isSigmaFunc) stoich *= stoich;
			sum += dispatchPropensityFunction(ref lambdaState, ref model, model.propensityDetails[q]) * stoich;
		}
		return sum;
	}

	public static unsafe float chooseTau(ref DiseaseState state, ref SimulationModel model, float epsilon) {
		//I won't lie, I kind of gave up here
		return 0.5f;
	}


	//Does a tau leaping step
	public static unsafe DiseaseState tauLeaping(DiseaseState state, ref SimulationModel model, float epsilon, bool railroadDt = false, float dt = 1.0f) {
		DiseaseState writeState = new DiseaseState(state);

		float tau = chooseTau(ref state, ref model, epsilon);
		//tau = 1.0f;

		float dtLeftToSimulate = dt - tau;

		//Bring tau up to dt if it is within the maxTauRaiseAmount
		if (dtLeftToSimulate <= maxTauRaiseAmount && railroadDt) tau = dt;

		for (int q = 0; q < model.reactionCount; q++) {
			float propensityResult = dispatchPropensityFunction(ref state, ref model, model.propensityDetails[q]);
			int reactionEvents = poissonNumber(propensityResult * tau);

			//In the case of 0 reaction events, we just skip ahead
			if (reactionEvents == 0) continue;

			for (int i = 0; i < model.compartmentCount; i++) {
				writeState.state[i] += model.stoichiometry[q, i] * reactionEvents;
			}
		}

		writeState.dt += tau;
		if (railroadDt && !(dtLeftToSimulate <= maxTauRaiseAmount)) {
			return tauLeaping(writeState, ref model, epsilon, railroadDt, dtLeftToSimulate);
		}

		return writeState;
	}
}
