
public class PartiallyConnectedNeuralNetwork : NeuralNetwork
{
    private PartiallyConnectedNeuron[][] Layers;
    private NeuronFunction Function;



    public PartiallyConnectedNeuralNetwork(NeuronFunction function, int[] layerSizes, params int[][][] connections)
    {
        Function = function;
        LayerSizes = layerSizes;
        Depth = LayerSizes.Length - 1; // number of layers - Layers.Length

        InitializeNeurons(connections);
    }

    private void InitializeNeurons(int[][][] connections)
    {
        Layers = new PartiallyConnectedNeuron[Depth][];
        for (int i = 0; i < Depth; i++)
        {

            Layers[i] = new PartiallyConnectedNeuron[LayerSizes[i + 1]];

            for (int j = 0; j < Layers[i].Length; j++)
            {   
                Layers[i][j] = new PartiallyConnectedNeuron(connections[i][j]);
                Layers[i][j].Randomize();
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
            }
            prevLayer = Layers[i];
        }
        return Layers[Depth - 1];
    }

    public float TrainWithSample(FloatNeuron[] inputs, float[] correctOutputs, float strength = 1f)
    {
        float error = Test(inputs, correctOutputs);

        CalculateLastLayerNodeValues(correctOutputs);

        for (int i = Depth - 2; i >= 0; i--)
        {
            CalculateHiddenLayerNodeValues(i);
        }

        Move(0, inputs, strength);
        for (int i = 1; i < Depth; i++)
        {
            Move(i, Layers[i - 1], strength);
        }
        return error;
    }



    private void Move(int layerIndex, FloatNeuron[] lastNeurons, float strength)
    {
        float newWeight;
        float newBias;
        float neuronValue;

        PartiallyConnectedNeuron neuron;
        for (int i = 0; i < Layers[layerIndex].Length; i++)
        {
            neuron = Layers[layerIndex][i];
            neuronValue = neuron.NodeValue * strength;
            for (int j = 0; j < neuron.WeightKeys.Length; j++)
            {
                newWeight = neuron.GetWeight(j) - neuronValue * lastNeurons[neuron.WeightKeys[j]].Result;
                neuron.SetWeight(j, newWeight);
            }


            newBias = neuron.GetBias() - neuronValue;
            neuron.SetBias(newBias);
        }

    }

    internal PartiallyConnectedNeuron[] GetLayer(int i)
    {
        return Layers[i];
    }

    private void CalculateLastLayerNodeValues(float[] correctOutputs)
    {
        float scoreDerivative;
        float funDerivative;

        PartiallyConnectedNeuron[] lastLayer = Layers[Depth - 1];
        for (int i = 0; i < lastLayer.Length; i++)
        {
            scoreDerivative = lastLayer[i].CalculateScoreDerivative(correctOutputs[i]);
            funDerivative = Function.GetDerivative(lastLayer[i].ResultBeforeFun);
            lastLayer[i].NodeValue = funDerivative * scoreDerivative;
        }
    }
    private void CalculateHiddenLayerNodeValues(int layerIndex)
    {
        float nodeValue;
        //Debug.Log(Layers.Length+" "+layerIndex);
        PartiallyConnectedNeuron[] thisLayer = Layers[layerIndex];
        PartiallyConnectedNeuron[] nextLayer = Layers[layerIndex + 1]; // the layer for which nodeValues were already calculated

        for (int i = 0; i < thisLayer.Length; i++)
        {
            nodeValue = 0f;
            for (int j = 0; j < thisLayer[i].WeightKeys.Length; j++)
            {
                nodeValue += nextLayer[thisLayer[i].WeightKeys[j]].NodeValue * nextLayer[thisLayer[i].WeightKeys[j]].GetWeight(i); // the nodeValue of the next neuron times the weigth connecting thisLayer[i] and nextLayer[j] 
            }
            thisLayer[i].NodeValue = nodeValue * Function.GetDerivative(thisLayer[i].ResultBeforeFun);
        }
    }

    private float CalculateScore(FloatNeuron[] lastLayer, float[] correctOutputs)
    {

        float score = 0;
        for (int i = 0; i < lastLayer.Length; i++)
        {
            score += lastLayer[i].CalculateScore(correctOutputs[i]);
        }
        return score;
    }
}       