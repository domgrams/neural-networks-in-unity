
using System.Collections.Generic;
using UnityEngine;

public class SilkNeuralNetwork : NeuralNetwork
{
    private SilkNeuron[][] Layers;
    private NeuronFunction Function;

    public System.Random random;

    private const float epsilon = 0.000000001f;

    private SilkNeuron nonDifferential;
    private int nonDifferentialIndex;
    private float lastError;

    public static bool wasNonDiff = false;

    public SilkNeuralNetwork(NeuronFunction function, int seed = 0, params int[] layerSizes )
    {
        Function = function;
        LayerSizes = layerSizes;
        Depth = LayerSizes.Length - 1; // number of layers - Layers.Length


        random = new System.Random(seed);
        InitializeNeurons();
    }

    private void InitializeNeurons()
    {
        Layers = new SilkNeuron[Depth][];
        for (int i = 0; i < Depth; i++)
        {

            Layers[i] = new SilkNeuron[LayerSizes[i + 1]];

            for (int j = 0; j < Layers[i].Length; j++)
            {
                Layers[i][j] = new SilkNeuron(new float[LayerSizes[i]], 0);
                Layers[i][j].Randomize(random);

            }
        }
    }


    public float Test(FloatNeuron[] inputs, float[] correctOutputs)
    {
        FloatNeuron[] lastLayer = Run(inputs);
        return CalculateScore(lastLayer, correctOutputs);
    }

    public FloatNeuron[] Run(params FloatNeuron[] inputs)
    {
        FloatNeuron[] prevLayer = inputs;
        for (int i = 0; i < Depth; i++)
        {

            for (int j = 0; j < Layers[i].Length; j++)
            {
                Layers[i][j].Run(prevLayer, Function);
                Layers[i][j].ResultsTogether += Layers[i][j].Result;
            }
            prevLayer = Layers[i];
        }
        return Layers[Depth - 1];
    }


    public float TrainWithSet(List<ColorDataPoint> cdps, float strength = 1f, float heroStrength = 1f)
    {
        InputNeuron[] inputs = new InputNeuron[cdps[0].neuronInputs.Length];
        for (int j = 0; j < inputs.Length; j++)
        {
            inputs[j] = new InputNeuron(0);
        }
        float error = 0;
        foreach (ColorDataPoint cdp in cdps)
        {
            int j = 0;
            foreach(InputNeuron inputNeuron in cdp.neuronInputs)
            {
                inputs[j].Result += inputNeuron.Result;
                j++;
            }


            error += Test(cdp.neuronInputs, cdp.outputs);


            nonDifferential = null;
            nonDifferentialIndex = -5;


            CalculateLastLayerNodeValues(cdp.outputs);
            for (int i = Depth - 2; i >= 0; i--)
            {
                CalculateHiddenLayerNodeValues(i);
            }
        }


        if (wasNonDiff)
        {
            wasNonDiff = false;
            Logger.Print("Non-differentiable reduction: " + (lastError - error) + ".");
            if (Mathf.Abs(lastError - error) > epsilon)
            {
                ColorAI.nonDiffPointResults[ColorAI.function] += (lastError - error);
                ColorAI.nonDiffPoints[ColorAI.function] += 1;
            }
        }
        lastError = error;

        if (nonDifferential == null)
        {
            Move(0, inputs, strength);
            for (int i = 1; i < Depth; i++)
            {
                Move(i, Layers[i - 1], strength);
            }
        }
        else
        {
            if (nonDifferentialIndex == -1)
            {
                HeroMove(inputs, nonDifferential, heroStrength);
            }
            else
            {
                HeroMove(Layers[nonDifferentialIndex], nonDifferential, heroStrength);
            }
        }


        return error;
    }

    public float TrainWithSample(FloatNeuron[] inputs, float[] correctOutputs, float strength = 1f, float heroStrength = 1f)
    {
        


        float error = Test(inputs, correctOutputs);

        if(wasNonDiff)
        {
            wasNonDiff = false;
            if(Mathf.Abs(lastError - error) > epsilon)
            {
                Logger.Print("Non-differentiable reduction: " + (lastError - error) + ".");
                ColorAI.nonDiffPointResults[ColorAI.function] += (lastError - error);
                if(ColorAI.worstStep[ColorAI.function] > (lastError - error))
                {
                    ColorAI.worstStep[ColorAI.function] = (lastError - error);
                }
                ColorAI.nonDiffPoints[ColorAI.function] += 1;
            }
        }
        lastError = error;

        nonDifferential = null;
        nonDifferentialIndex = -5;


        CalculateLastLayerNodeValues(correctOutputs);
        for (int i = Depth - 2; i >= 0; i--)
        {
            CalculateHiddenLayerNodeValues(i);
        }

        if(nonDifferential == null)
        {
            Move(0, inputs, strength);
            for (int i = 1; i < Depth; i++)
            {
                Move(i, Layers[i - 1], strength);
            }
        }
        else
        {
            if(nonDifferentialIndex == -1)
            {
                HeroMove(inputs, nonDifferential, heroStrength);
            }
            else
            {
                HeroMove(Layers[nonDifferentialIndex], nonDifferential, heroStrength);
            }

            Move(0, inputs, strength);
            for (int i = 1; i < Depth; i++)
            {
                Move(i, Layers[i - 1], strength);
            }
        }


        return error;
    }



    private void Move(int layerIndex, FloatNeuron[] lastNeurons, float strength)
    {
        float newWeight;
        float newBias;
        float neuronValue;

        

        SilkNeuron neuron;
        for (int i = 0; i < Layers[layerIndex].Length; i++)
        {
            neuron = Layers[layerIndex][i];
            neuronValue = neuron.NodeValue * strength;

            if (float.IsNaN(neuron.NodeValue2))
            {
                for (int j = 0; j < lastNeurons.Length; j++)
                {
                    newWeight = neuron.GetWeight(j) - neuronValue * lastNeurons[j].Result;
                    neuron.SetWeight(j, newWeight);
                }


                newBias = neuron.GetBias() - neuronValue;
                neuron.SetBias(newBias);
            }
            
        }

    }

    private void HeroMove(FloatNeuron[] lastNeurons, SilkNeuron neuron, float strength)
    {
        Logger.Print("Searching for hero neuron...");
        float newWeight;
        float newBias;



        float neuronValue;
        float neuronValue2;
        bool found = false;
        for (int j = 0; j < lastNeurons.Length && !found; j++)
        {
            if (lastNeurons[j].Result != 0)
            {
                neuronValue = neuron.NodeValue * lastNeurons[j].Result;
                neuronValue2 = neuron.NodeValue2 * lastNeurons[j].Result;
                Debug.Log(neuronValue + ", " + neuronValue2);
                if(neuronValue == 0 && neuronValue2 == 0)
                {
                    Debug.Log("zeros from...");
                    Debug.Log(neuron.NodeValue + ", " + neuron.NodeValue2);
                    Debug.Log("from...");
                    Debug.Log(neuron.NodeValue + ", " + neuron.NodeValue2);
                }
                if (neuronValue2 < 0 && neuronValue2 <= -neuronValue)
                {
                    Logger.Print("Adapting hero by right w.");
                    Logger.Print(neuron.GetWeight(j) + " - " + neuronValue2 * strength);
                    newWeight = neuron.GetWeight(j) - neuronValue2 * strength;
                    neuron.SetWeight(j, newWeight);
                    found = true;
                }
                else if(neuronValue > 0 && neuronValue >= -neuronValue2)
                {
                    Logger.Print("Adapting hero by left w.");
                    Logger.Print(neuron.GetWeight(j) + " - " + neuronValue * strength);
                    newWeight = neuron.GetWeight(j) - neuronValue * strength;
                    neuron.SetWeight(j, newWeight);
                    found = true;
                }
            }
        }

        if (!found)
        {
            neuronValue = neuron.NodeValue;
            neuronValue2 = neuron.NodeValue2;
            if (neuronValue2 < 0 && neuronValue2 <= -neuronValue)
            {
                Logger.Print("Adapting hero by right b.");
                Logger.Print(neuron.GetBias() + " - " + neuronValue2 * strength);
                newBias = neuron.GetBias() - neuronValue2 * strength;
                neuron.SetBias(newBias);
            }
            else if (neuronValue > 0 && neuronValue >= -neuronValue2)
            {
                Logger.Print("Adapting hero by left b.");
                Logger.Print(neuron.GetBias() + " - " + neuronValue * strength);
                newBias = neuron.GetBias() - neuronValue * strength;
                neuron.SetBias(newBias);
            }
            else if (Mathf.Abs(neuronValue) > Mathf.Abs(neuronValue2) && neuronValue2 != 0)
            {
                Logger.Print("Oh no, adapting hero by right b.");
                Logger.Print(neuron.GetBias() + " - " + neuronValue2 * strength);
                newBias = neuron.GetBias() - neuronValue2 * strength;
                neuron.SetBias(newBias);
            }
            else
            {
                Logger.Print("Oh no, adapting hero by left b.");
                Logger.Print(neuron.GetBias() + " - " + neuronValue * strength);
                newBias = neuron.GetBias() - neuronValue * strength;
                neuron.SetBias(newBias);
            }
        }
    }

    internal SilkNeuron[] GetLayer(int i)
    {
        return Layers[i];
    }

    

    private void CalculateLastLayerNodeValues(float[] correctOutputs)
    {
        float scoreDerivative;
        float funDerivative;

        SilkNeuron[] lastLayer = Layers[Depth - 1];
        for (int i = 0; i < lastLayer.Length; i++)
        {
            scoreDerivative = lastLayer[i].CalculateScoreDerivative(correctOutputs[i]);
            if(scoreDerivative == 0)
            {
                Debug.Log("perfect one!");
            }
            funDerivative = Function.GetDerivative(lastLayer[i].ResultBeforeFun);
            if(float.IsNaN(funDerivative))
            {
                nonDifferential = lastLayer[i];
                nonDifferentialIndex = Layers.Length - 2;
                lastLayer[i].SetNodeValue(
                    Function.GetDerivative(lastLayer[i].ResultBeforeFun - epsilon) * scoreDerivative,
                    Function.GetDerivative(lastLayer[i].ResultBeforeFun + epsilon) * scoreDerivative
                    );
            }
            else
            {
                lastLayer[i].SetNodeValue(funDerivative * scoreDerivative);
            }
        }
    }
    private void CalculateHiddenLayerNodeValues(int layerIndex)
    {
        float nodeValue;
        float nodeValue2;
        float funDerivative;
        //Debug.Log(Layers.Length+" "+layerIndex);
        SilkNeuron[] thisLayer = Layers[layerIndex];
        SilkNeuron[] nextLayer = Layers[layerIndex + 1]; // the layer for which nodeValues were already calculated

        for (int i = 0; i < thisLayer.Length; i++)
        {
            nodeValue = 0f;
            nodeValue2 = float.NaN;
            for (int j = 0; j < nextLayer.Length; j++)
            {
                nodeValue += nextLayer[j].NodeValue * nextLayer[j].GetWeight(i); // the nodeValue of the next neuron times the weigth connecting thisLayer[i] and nextLayer[j] 
                if (nextLayer[j].Doubled)
                {
                    if (float.IsNaN(nodeValue2)) nodeValue2 = 0f;
                    nodeValue2 += nextLayer[j].NodeValue2 * nextLayer[j].GetWeight(i);
                }
            }

            funDerivative = Function.GetDerivative(thisLayer[i].ResultBeforeFun);


            // if there is no derivative
            if(float.IsNaN(funDerivative))
            {
                Debug.Log("eq is working");
                funDerivative = Function.GetDerivative(thisLayer[i].ResultBeforeFun - epsilon);
                float funDerivative2 = Function.GetDerivative(thisLayer[i].ResultBeforeFun + epsilon);

                Debug.Log(nodeValue);
                Debug.Log(nodeValue2);

                // if there was no derivative in the backprop
                if (float.IsNaN(nodeValue2))
                {
                    nonDifferential = thisLayer[i];
                    nonDifferentialIndex = layerIndex - 1;
                    thisLayer[i].SetNodeValue(nodeValue * funDerivative, nodeValue * funDerivative2);
                }
                else
                    thisLayer[i].SetNodeValue(nodeValue * funDerivative, nodeValue2 * funDerivative2);
            }
            else
            {
                // if there was no derivative in the backprop
                if (float.IsNaN(nodeValue2))
                {
                    thisLayer[i].SetNodeValue(nodeValue * funDerivative);
                }
                else 
                    thisLayer[i].SetNodeValue(nodeValue * funDerivative, nodeValue2 * funDerivative);

            }
        }
    }

    public float CalculateScore(FloatNeuron[] lastLayer, float[] correctOutputs)
    {

        float score = 0;
        for (int i = 0; i < lastLayer.Length; i++)
        {
            score += lastLayer[i].CalculateScore(correctOutputs[i]);
        }
        return score;
    }

   
}




public class SilkNeuron : FullyConnectedNeuron
{
    public bool Doubled { get; private set; }
    public float ResultsTogether;

    public float NodeValue2 
    {
        get 
        { 
            return nodeValue2; 
        } 
        set 
        { 
            nodeValue2 = value; 
        } 
    }

    private float nodeValue2;

    public SilkNeuron(float[] weights, float bias) : base(weights, bias)
    {
    }

    public void SetNodeValue(float nodeValue1)
    {
        NodeValue = nodeValue1;
        NodeValue2 = float.NaN;
        Doubled = false;
    }
    public void SetNodeValue(float nodeValue1, float nodeValue2)
    {
        NodeValue2 = nodeValue2;
        NodeValue = nodeValue1;
        Doubled = true;
    }
}