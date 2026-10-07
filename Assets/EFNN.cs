using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MathFunctionType
{
    SumOfTwo, TimesOfTwo
}
public class EFNN : MonoBehaviour
{
    public bool Restart;
    public MathFunctionType FunctionType;
    private MathFunction mathFunction;
    public bool Train;

    public float Strength;
    public int TPF;
    public int NT;

    public bool Test;
    public float[] Inputs;
    public float[] Outputs;
    public float Error;

    private FullyConnectedNeuralNetwork network;


    // Start is called before the first frame update
    void Start()
    {
        if (FunctionType == MathFunctionType.SumOfTwo)
        {
            mathFunction = new SumOfTwo();
        }
        if (FunctionType == MathFunctionType.TimesOfTwo)
        {
            mathFunction = new TimesOfTwo();
        }

        network = new FullyConnectedNeuralNetwork(new Atangens(1), mathFunction.GetLayerSizes());


        Inputs = new float[network.LayerSizes[0]];
        mathFunction.BindNetwork(network);
    }

    private void OnValidate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Restart)
        {
            Restart = false;
            Start();
        }
        else if (Train)
        {
            float error = 0;
            for (int i = 0; i < TPF; i++)
            {
                error += mathFunction.TrainWithSample(Strength);
            }
            Error = error / TPF;
            NT += TPF;
        }
        else if (Test)
        {
            FloatNeuron[] neuronInputs = new FloatNeuron[Inputs.Length];
            for (int i = 0; i < Inputs.Length; i++)
            {
                neuronInputs[i] = new InputNeuron(Inputs[i]);
            }

            FloatNeuron[] outputs = mathFunction.Test(neuronInputs).Item1;
            Error = mathFunction.Test(neuronInputs).Item2;
            
            Outputs = new float[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                Outputs[i] = outputs[i].Result;
            }
        }
        
    }
}

public abstract class MathFunction
{
    protected FullyConnectedNeuralNetwork network;
    
    public virtual void BindNetwork(FullyConnectedNeuralNetwork fcnn)
    {
        network = fcnn;
    }

    public abstract int[] GetLayerSizes();

    public abstract NeuronFunction GetActivationFunction();
    public abstract float TrainWithSample(float strength);

    public abstract (FloatNeuron[], float) Test(FloatNeuron[] testingSample);
}

public class SumOfTwo : MathFunction
{
    public override NeuronFunction GetActivationFunction()
    {
        return new Atangens(1f);
    }

    public override int[] GetLayerSizes()
    {
        return new int[] { 2,1 };
    }

    public override (FloatNeuron[], float) Test(FloatNeuron[] testingSample)
    {
        float[] correctOutputs = new float[] { testingSample[0].Result + testingSample[1].Result };
        FloatNeuron[] outputs = network.Run(testingSample);
        float error = network.Test(testingSample, correctOutputs);
        return (outputs, error);
    }

    public override float TrainWithSample(float strength)
    {
        float x = 0; //Random.value;
        float y = 0; //Random.value;
        return network.TrainWithSample(new FloatNeuron[] { new InputNeuron(x), new InputNeuron(y) }, new float[] { x+y }, strength);

    }
}

public class TimesOfTwo : MathFunction
{
    public override NeuronFunction GetActivationFunction()
    {
        return new Sigmoid(1f);
    }

    public override int[] GetLayerSizes()
    {
        return new int[] { 2, 8, 10, 15, 1 };
    }

    public override (FloatNeuron[], float) Test(FloatNeuron[] testingSample)
    {
        float[] correctOutputs = new float[] { testingSample[0].Result * testingSample[1].Result };
        FloatNeuron[] outputs = network.Run(testingSample);
        float error = network.Test(testingSample, correctOutputs);
        return (outputs, error);
    }

    public override float TrainWithSample(float strength)
    {
        float x = 0;// Random.value* Random.value*100;
        float y = 0;// Random.value*Random.value*100;
        return network.TrainWithSample(new FloatNeuron[] { new InputNeuron(x), new InputNeuron(y) }, new float[] { x * y }, strength);
    }
}