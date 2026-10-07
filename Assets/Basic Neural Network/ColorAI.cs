using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ColorGuess
{
    public ColorName colorName;
    public float chance;
    public ColorGuess(ColorName colorName, float chance)
    {
        this.colorName = colorName;
        this.chance = chance;
    }
}

public class ColorAI : MonoBehaviour
{
    public List<ColorDataPoint> colorDataPoints;
    public TrainingSet trainingSet;
    public VisualizeColorAI visualization;
    public const int ColorsLength = 12;
    public bool RestartAI;
    public bool Training;
    public bool VisualizeDuringTraining = false;
    public bool Testing;
    public bool InAction = false;

    public float ErrorValue;

    public float strength = 1f;
    public int TPF = 50;
    public int NumberOfTrainSamples = 0;

    public int middle;
    public Color inMiddleColor;

    public float ActivationFunctionCoefficient;
    public int[] layerSizes = new int[] { 3, 7, 17, 12 };

    public Color color;
    public ColorName guessedColorName;
    public ColorName colorName;
    public bool Add;
    public List<ColorGuess> chances;

    public static int function = 0;

    private int startingThing;
    private int seed = 20001;

    private SilkNeuralNetwork network;

    public static readonly float[] functionStrenghts = { 0.01f, 0.01f, 0.01f, 0.01f, 0.01f };
    public static readonly float[] functionNonDiffStrenghts = { 0.1f, 1f, 1f, 10f, 1f };
    public static int[] nonDiffPoints = new int[5];
    public static float[] nonDiffPointResults = new float[5];
    public static float[] resultsAfterWhile = new float[5];
    public static float[] worstStep = new float[5] { 10, 10, 10, 10, 10 };

    private void Start()
    {
        Restart(0);
    }

    // Start is called before the first frame update
    void Restart(int seed)
    {
        NumberOfTrainSamples = 0;

        NeuronFunction nFunction;
        
        if (function == 1)
        {
            nFunction = new PReLU(1f);
            (nFunction as PReLU).NanIn0 = false;
        }
        else if(function == 2)
        {
            nFunction = new PReLU(1f);
            //nFunction = new Atangens(1);
        }
        else if (function == 3)
        {
            nFunction = new PReLU(1f);
            //nFunction = new Sigmoid(1);
        }
        else if (function == 4)
        {
            nFunction = new PReLU(1f);
            //nFunction = new Abs(1);
        }
        else
        {
            nFunction = new PReLU(1f);
        }

        strength = functionStrenghts[function];
        network = new SilkNeuralNetwork(nFunction, seed, layerSizes);

        //network = new HeroNeuralNetwork(layerSizes);
        //network.Function = nFunction;
        //network.Randomize();
        //network.SetInputParameters(3, 1, 1);
        //network.AddLayer(LayerTypeCNN.Classic, size: 17);
        //network.AddLayer(LayerTypeCNN.Classic, size: 17);
        //network.AddLayer(LayerTypeCNN.Classic, size: 12);

    }

    // Update is called once per frame
    void Update()
    {
        if (network == null) return;
        if (!InAction) return;

        if (RestartAI)
        {
            RestartAI = false;
            Restart(Random.Range(0,20000));
        }
        if (Add)
        {
            ColorDataPoint cdp = new ColorDataPoint(color, colorName);
            Debug.Log(cdp);
            trainingSet.Samples.Append(new TrainingSample(cdp.inputs, cdp.outputs));
            Add = false;
        }
        if (Training)
        {
            if (trainingSet == null) return;
            float val = 0f;
            
            if (NumberOfTrainSamples >= 200000)
            {
                resultsAfterWhile[function] += ErrorValue;
                function++;
                if(function >= 4)
                {
                    startingThing++;
                    if(startingThing == 250)
                    {
                        Logger.Clear();
                        Logger.Print(nonDiffPoints);
                        Logger.Print(nonDiffPointResults);
                        Logger.Print(resultsAfterWhile);
                        Logger.Print(worstStep);
                        Training = false;
                        return;
                    }
                    function = 0;
                    seed = Random.Range(0, 20000);
                }
                Restart(seed);
                return; 
            }
            ColorDataPoint cdp = colorDataPoints[network.random.Next(colorDataPoints.Count)];

            bool again = false;
            for (int i = 0; i < TPF; i++)
            {
                if (!SilkNeuralNetwork.wasNonDiff || again){
                    if (again)
                    {
                        SilkNeuralNetwork.wasNonDiff = false;
                    }
                    
                    cdp = colorDataPoints[network.random.Next(colorDataPoints.Count)];
                    again = false;
                }
                else
                {
                    again = true;
                }
                val += network.TrainWithSample(cdp.neuronInputs, cdp.outputs, strength, strength * functionNonDiffStrenghts[function]);
            }

            NumberOfTrainSamples += TPF;

            ErrorValue = val/TPF;

            if (VisualizeDuringTraining)
            {
                visualization.Generate();
            }
        }
        if (Testing)
        {
            
            
            ColorDataPoint cdp = new ColorDataPoint(color, colorName);
            //Debug.Log(color);
            //Debug.Log(colorName);
            //Debug.Log(cdp.inputs);
            ErrorValue = network.Test(cdp.neuronInputs, cdp.outputs);
             


            //FloatNeuron[] middleNeurons = network.GetLayer(middle);

            //inMiddleColor = new Color(middleNeurons[0].Result, middleNeurons[1].Result, middleNeurons[2].Result);

            //string text = "";
            //for (int i = 0; i < outputNeurons.Length; i++)
            //{
            //    text += outputNeurons[i].Result + "/";
            //}
            //Debug.Log(text);

            FloatNeuron[] outputNeurons = network.GetLayer(network.Depth - 1);

            chances.Clear();
            int max = 0;
            for (int i = 0; i < ColorsLength; i++)
            {
                for (int j = 0; j < outputNeurons.Length; j++)
                {
                    if (outputNeurons[max].Result < outputNeurons[j].Result)
                    {
                        max = j;
                    }
                }

                chances.Add(new ColorGuess((ColorName)max, outputNeurons[max].Result));
                outputNeurons[max].Result = -1;
                guessedColorName = (ColorName)max;
            }
        }
    }

    public ColorName PublicRun(Color input)
    {
        ColorDataPoint cdp = new ColorDataPoint(input, ColorName.Black);
        network.Run(cdp.neuronInputs);
        FloatNeuron[] outputNeurons = network.GetLayer(network.Depth - 1);

        int max = 0;
        for (int j = 0; j < outputNeurons.Length; j++)
        {
            if (outputNeurons[max].Result < outputNeurons[j].Result)
            {
                max = j;
            }
        }

        return (ColorName)max;
    }

    private void OnValidate()
    {
        if (Training)
        {
            Testing = false;
        }
        else
        {
            Testing = true;
        }
    }

}
public enum ColorName
{
    Black,
    White,
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
    Brown,
    Grey,
    Purple,
    Pink,
    Turqoise
}

//[System.Serializable]
public class ColorDataPoint
{

    public float[] inputs;
    public InputNeuron[] neuronInputs;
    public float[] outputs;



    public Color color;
    public ColorName colorName;

    public ColorDataPoint(Color input, ColorName output)
    {
        color = input;
        colorName = output;

        neuronInputs = InputNeuron.GenerateInputs(input.r, input.g, input.b);
        inputs = new float[] { input.r, input.g, input.b };
        outputs = new float[ColorAI.ColorsLength];
        outputs[(int)output] = 1;
    }

    public override string ToString()
    {
        string test = "";
        for (int i = 0; i < inputs.Length; i++)
        {
            test += inputs[i] + "/";
        }
        test += "\r\n";
        for (int i = 0; i < outputs.Length; i++)
        {
            test += outputs[i] + "/";
        }
        return test;
    }
}