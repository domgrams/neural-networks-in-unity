using System.Collections.Generic;
using Unity;
using UnityEngine;

public interface IBiasedNeuron
{
    float GetBias();
    void SetBias(float b);
}

public interface IBiasedNeuron2D
{
    float GetBias(int i, int j);
    void SetBias(int i, int j, float b);
}   

public interface IWeightedNeuron1
{
    void SetWeight(int i, float w);
    float GetWeight(int i);

}
public abstract class Neuron
{

    public virtual float CalculateScore(float supposedResult)
    {
        return 0f;
    }
    public virtual float CalculateScoreDerivative(float supposedResult)
    {
        return 0f;
    }
    public virtual Neuron Randomize()
    {
        return this;
    }
}

public abstract class FloatNeuron : Neuron
{

    public virtual float Result { get; set; }
    public virtual float ResultBeforeFun { get; set; }


    public virtual float Run(FloatNeuron[] prevLayer, NeuronFunction fun)
    {
        return 0f;
    }
}

public abstract class FloatNeuronWithNodeValue : FloatNeuron
{
    public virtual float NodeValue { get; set; }
}


public class InputNeuron : FloatNeuron
{   
    public override float Result { get { return result; } set { result = value; } }
    public override float ResultBeforeFun { get { return resultBeforeFun; } set { resultBeforeFun = value; } }


    private float result;
    private float resultBeforeFun;

    public static InputNeuron[] GenerateInputs(params float[] inputs)
    {
        InputNeuron[] inputLayer = new InputNeuron[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputLayer[i] = new InputNeuron(inputs[i]);
        }
        return inputLayer;
    }

    public InputNeuron(float input)
    {
        Result = input;
    }
}



public class FullyConnectedNeuron : FloatNeuronWithNodeValue, IBiasedNeuron, IWeightedNeuron1
{
    public override float NodeValue { get { return nodeValue; } set { nodeValue = value; } }
    public override float Result { get { return result; } set { result = value; } }
    public override float ResultBeforeFun { get { return resultBeforeFun; } set { resultBeforeFun = value; } }

    private float result;
    private float resultBeforeFun;
    private float nodeValue;


    private readonly float[] Weights;
    private float Bias;

    public override float Run(FloatNeuron[] prevLayer, NeuronFunction fun)
    {
        float final = 0;
        for (int i = 0; i < prevLayer.Length; i++)
        {
            final += prevLayer[i].Result * Weights[i];
        }
        final += Bias;

        ResultBeforeFun = final;
        Result = fun.Get(final);
        return Result;
    }

    public Neuron Randomize(System.Random random)
    {
        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = ((float)random.NextDouble() - 0.5f) * 0.1f;
        }
        Bias = (float)random.NextDouble() * 0.2f;
        return this;
    }

    public float GetWeight(int i)
    {
        return Weights[i];
    }
    public FullyConnectedNeuron(float[] weights, float bias)
    {
        Weights = weights;
        Bias = bias;
    }

    public void SetWeight(int i, float w)
    {
        Weights[i] = w;
    }   

    public void SetBias(float b)
    {
        Bias = b;
    }
    public override float CalculateScoreDerivative(float supposedResult)
    {
        float error = Result - supposedResult;
        return 2 * error;
    }
    public override float CalculateScore(float supposedResult)
    {
        float error = Result - supposedResult;
        return error * error;
    }

    public float GetBias()
    {
        return Bias;
    }
}

public class PixelNeuron : FloatNeuronWithNodeValue
{

    public override float NodeValue { get => nodeValue; set => nodeValue = value; }
    public override float Result { get { return result; } set { result = value; } }
    public override float ResultBeforeFun { get { return resultBeforeFun; } set { resultBeforeFun = value; } }

    private float result;
    private float resultBeforeFun;
    private float nodeValue;

    public PixelNeuron()
    {

    }

    public float CalculateExp()
    {
        Result = Mathf.Exp(ResultBeforeFun);
        return Result;
    }



}

public class MacroUnit : Neuron, IBiasedNeuron
{
    private readonly LayerCNN layerCNN;

    public LayerTypeCNN LayerType { get => layerCNN.LayerType; } 
    public int WindowWidth { get => layerCNN.WindowWidth; }
    public int WindowHeight { get => layerCNN.WindowHeight; }

    public int Width { get => layerCNN.MacroUnitWidth; }
    public int Height { get => layerCNN.MacroUnitHeight; }
    public string LayerInfo { get => layerCNN.Info; }

    public int Stride { get => layerCNN.Stride; }
    public int Padding { get => layerCNN.Padding; }

    public PixelNeuron Single { get => Neurons[0, 0]; }

    public PixelNeuron[,] Neurons;
    private float Bias;

    private LinkedChain<LinkedChain2<float>> Weights;
    

    public MacroUnit(int inputWidth, int inputHeight)
    {
        Neurons = new PixelNeuron[inputHeight, inputWidth];
        for (int i = 0; i < inputHeight; i++)
        {
            for (int j = 0; j < inputWidth; j++)
            {
                Neurons[i, j] = new PixelNeuron();
            }
        }
        layerCNN = new ConvolutionalLayerCNN(LayerTypeCNN.Convolutional);
        layerCNN.MacroUnitHeight = inputHeight;
        layerCNN.MacroUnitWidth = inputWidth;
    }

    public MacroUnit(LayerCNN layerCNN, int previousLayerDepth)
    {
        this.layerCNN = layerCNN;

        initializeNeurons();

        initializeConvolutional(previousLayerDepth);
    }

    public MacroUnit(LayerCNN layerCNN, int previousLayerDepth, bool pooling)
    {
        this.layerCNN = layerCNN;

        initializeNeurons();

        if (!pooling) initializeConvolutional(previousLayerDepth);
        else Weights = null;

        
    } 

    public override Neuron Randomize()
    {
       
        Weights.Reset();
        while (Weights.Alive)
        {
            Weights.Get().Reset();
            while (Weights.Get().Alive)
            {
                Weights.Get().Selector2.Contents = (Random.value - 0.5f) * 0.3f;
                Weights.Get().Next2();
            }
            Weights.Next();
        }
        Bias = (Random.value) * 0.005f;
        //for (int i = 0; i < Bias.GetLength(0); i++)
        //{
        //    for (int j = 0; j < Bias.GetLength(1); j++)
        //    {
        //        Bias[i, j] = (Random.value - 0.5f) * 2f;
        //    }
        //}
        return this;
    }

    public void Run(LinkedChain<MacroUnit> prevMacroUnits)
    {


        PixelNeuron selectedNeuron;
        float weightedResult;
        int k;
        int l;
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
               //Log.Elements(LayerInfo, "with coords", i, "/", j);

                prevMacroUnits.Reset();
                Weights.Reset();

                selectedNeuron = Neurons[i, j];
                selectedNeuron.ResultBeforeFun = 0f;

                //Log.Elements("weights");
                while (prevMacroUnits.Alive) // prevLayer: MU0 --> MU1 --> ... --> MU{prevLayer.Size-1}
                                             // Weights: Edge0 --> Edge1 --> ... --> MU{prevLayer.Size-1}
                {
                    //try
                    //{
                    Weights.Get().Reset();
                    //}
                    //catch
                    //{
                    //    Log.Elements(LayerInfo);
                    //    Log.Elements(i, j);
                    //    Log.Elements(prevMacroUnits.Start.Depth);
                    //    Log.Finish();
                    //    Log.Elements(Weights.Start.Depth);
                    //    Log.Finish();

                    //    throw new System.Exception();
                    //}
                    while (Weights.Get().Alive) // Weights.Get() (filters): F[0,0] --> F[0,1] --> ... F[0,layerCNN.WindowWidth] --> F[1,0] --> ... --> F[lCNN.WH,lCNN.WW]
                    {

                        k = i * Stride - Padding + Weights.Get().Coordinates1;
                        l = j * Stride - Padding + Weights.Get().Coordinates2;

                        if (k >= 0 && l >= 0 && k < prevMacroUnits.Get().Height && l < prevMacroUnits.Get().Width)
                        {

                            weightedResult = Weights.Get().Get() * prevMacroUnits.Get().Neurons[k, l].Result;
                            selectedNeuron.ResultBeforeFun += weightedResult;

                           //Debug.Log(Weights.Get().Coordinates1+"/"+ Weights.Get().Coordinates2);
                           //Log.Elements(prevMacroUnits.Get().Neurons[k, l].Result + " * " + Weights.Get().Get() + " = " + weightedResult);
                            
                        }
                        else
                        {
                           //Log.Elements(Weights.Get().Coordinates1, "/", Weights.Get().Coordinates2);
                           //Log.Elements("not existing");
                        }
                        Weights.Get().Next2();
                    }
                    prevMacroUnits.Next();
                    Weights.Next();
                }


                //Log.Finish();

                selectedNeuron.ResultBeforeFun += Bias;
                selectedNeuron.Result = ConvolutionalNeuralNetwork.ActivationFunction.Get(selectedNeuron.ResultBeforeFun);

               //Log.Elements(Neurons[i, j].ResultBeforeFun,"== ReLU ==>", Neurons[i, j].Result);
               //Log.Elements("Bias:",Bias);
               //Log.Finish();
            }
        }
    }

    public void Print()
    {
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Log.Elements("PXN");
            }
            Log.Elements("\r\n");
        }
        Log.Elements("Bias:", Bias);
        Log.Elements("Weights:\r\n");
        Weights.Reset();
        while (Weights.Alive)
        {
            Log.Elements("\r\n");
            Weights.Get().Reset();
            while (Weights.Get().Alive)
            {
                if (Weights.Get().Coordinates2 == 0) Log.Elements("\r\n");
                Log.Elements(Weights.Get().Get());
                Weights.Get().Next2();
            }
            Weights.Next();
        }
        Log.Finish();
    }

    public void RunPooling(MacroUnit macroUnit)
    {
        float thisResult; // the result of the iterated neuron
        float max;

        for (int i = 0; i < Neurons.GetLength(0); i++)
        {
            for (int j = 0; j < Neurons.GetLength(1); j++)
            {
                // maximum pooling
                max = 0;
                for (int k = 0; k < Stride; k++)
                {
                    for (int l = 0; l < Stride; l++)
                    {
                        thisResult = macroUnit.Neurons[i * Stride + k, j * Stride + l].Result;

                        if (thisResult > max)
                        {
                            max = thisResult;
                        }

                    }
                }
                Neurons[i, j].Result = max;
                Neurons[i, j].ResultBeforeFun = max;

            }
        }
    }

    public float GetBias()
    {
        return Bias;
    }

    public void SetBias( float b)
    {
        Bias = b;
    }

    public void ResetWeightsSelector()
    {
        Weights.Reset();
        Weights.Get().Reset();
    }
    public void NextWeightsSelector()
    {
        Weights.Next();
    }

    public float CalculatePenultimateNodeValue()
    {
        float num = Single.NodeValue * Weights.Get().Get();
        NextWeightsForPenultimateNodeValue();
        return num;
    }

    private void NextWeightsForPenultimateNodeValue()
    {
        Weights.Get().Next2();
        if (!Weights.Get().Alive)
        {
            Weights.Next();
            if (Weights.Alive)
            {
                Weights.Get().Reset();
            }
        }
    }

    void initializeConvolutional(int previousLayerDepth)
    {
        Weights = new LinkedChain<LinkedChain2<float>>(new LinkedChain2<float>(0f));
        initializeWeights();
        for (int g = 1; g < previousLayerDepth; g++)
        {
            Weights.Append(new LinkedChain2<float>(0f));
            initializeWeights();
        }
    }



    public float CalculateHiddenLayerNodeValue(int i, int j, bool debug = false)
    {
        float partialNodeValue = 0f;

        int kStart = (int)Mathf.Max(0, Mathf.Ceil((i + Padding - WindowHeight + 1) / (float)Stride));
        int lStart = (int)Mathf.Max(0, Mathf.Ceil((j + Padding - WindowWidth + 1) / (float)Stride));

        int kEnd = Mathf.Min(Height - 1, (i + Padding) / Stride);
        int lEnd = Mathf.Min(Width - 1, (j + Padding) / Stride);

        int weightIndex1;
        int weightIndex2;

        float backNodeValue;
        float activationDerivation;
        float weight;

        // doing it backwards for the Weights LinkedChain2 to find it
        for (int k = kEnd; k >= kStart; k--)
        {
            for (int l = lEnd; l >= lStart; l--)
            {
                weightIndex1 = i + Padding - k * Stride;
                weightIndex2 = j + Padding - l * Stride;

                //if(Random.value < 0.000001f)
                //{
                //    Log.Elements("next coords", i, Height, j, Width);
                //    Log.Elements("k' and l'", k, l);
                //    Log.Elements("k interval", kStart, kEnd);
                //    Log.Elements("l interval", lStart, lEnd);
                //    Log.Elements("padd and stride", Padding, Stride);
                //    Log.Finish();

                //    Debug.Log(weightIndex1 + ", " + weightIndex2);
                //}

                backNodeValue = Neurons[k, l].NodeValue;
                activationDerivation = ConvolutionalNeuralNetwork.ActivationFunction.GetDerivative(Neurons[k, l].ResultBeforeFun);

                weight = Weights.Get()[weightIndex1, weightIndex2].Get();

                partialNodeValue += backNodeValue * activationDerivation * weight;

                if (debug)
                {
                   //Log.Elements("+ (", backNodeValue, "*", activationDerivation, "*", weight, ")");
                }

            }
        }
        if (debug)
        {
       //Log.Elements("=", partialNodeValue, " & \r\n");
        }
        return partialNodeValue;
    }


    public void Move(LinkedChain<MacroUnit> prevMacroUnits,float strength, bool lastLayer = false)
    {
        int i;
        int j;

        int m;
        int n;

        float nodeValue;
        float activationDerivation;
        float prevResult;
        float weightMove;

        float biasMove = 0f;

        bool debug = false;
        int debugI = -1;
        int debugJ = -1;
        int debugF = -1;
        int f;


        for (int k = 0; k < Height; k++)
        {
            for (int l = 0; l < Width; l++)
            {

                nodeValue = Neurons[k, l].NodeValue;

                activationDerivation = lastLayer ? 1 : ConvolutionalNeuralNetwork.ActivationFunction.GetDerivative(Neurons[k, l].ResultBeforeFun);

                biasMove += nodeValue * activationDerivation;

                //f = 0;
                Weights.Reset();
                prevMacroUnits.Reset();
                while (Weights.Alive)
                {
                    Weights.Get().Reset();
                    while (Weights.Get().Alive)
                    {
                        i = Weights.Get().Coordinates1;
                        j = Weights.Get().Coordinates2;

                        m = k * Stride - Padding + i;
                        n = l * Stride - Padding + j;

                        if (m >= 0 && n >= 0 && m < prevMacroUnits.Get().Height && n < prevMacroUnits.Get().Width)
                        {
                            prevResult = prevMacroUnits.Get().Neurons[m, n].Result;

                        }
                        else
                        {
                            prevResult = 0f;
                        }

                        //try
                        //{
                        //}
                        //catch
                        //{
                        //    Log.Elements(k, l);
                        //    Log.Elements(i, j);
                        //    Log.Elements(Stride, Padding);
                        //    Log.Elements(prevMacroUnits.Get().Width,prevMacroUnits.Get().Height);
                        //    Log.Finish();
                        //    throw new System.Exception();
                        //}


                        weightMove = nodeValue * activationDerivation * prevResult;



                        //if (debug && debugI == i && debugJ == j && debugF == f)
                        //{
                        //Log.Elements(i, "/", j, "< ij | kl >", k, "/", l,"f is",f);
                        //Log.Elements(k,"/",l,nodeValue, "*", activationDerivation, "*", prevResult, "=", weightMove);
                        //}
                        
                        //if (Random.value < 0.00001f)
                        //{
                        //    Debug.Log(LayerInfo + " weights: " + Weights.Get().Coordinates1 + ", " + Weights.Get().Coordinates2 + " / " + Weights.Get().Count1 + ", " + Weights.Get().Count2 + " : " + Weights.Get().Get() + " weightMove "+weightMove+" strength "+strength);
                        //    Debug.Log(nodeValue + " * " + activationDerivation + " * " + prevResult);
                        //}


                        
                        Weights.Get().Selector2.Contents -= weightMove * strength;
                        //if (Mathf.Abs(Weights.Get().Selector2.Contents) > 1000000000)
                        //{
                        //    Weights.Get().Selector2.Contents = Mathf.Sign(Weights.Get().Selector2.Contents * 1000000000);
                        //}

                        Weights.Get().Next2();
                    }

                    //f++;
                    Weights.Next();
                    prevMacroUnits.Next();
                }


                //Log.Finish();
            }
        }

        Bias -= biasMove * strength;

        //Log.Elements(LayerInfo,"bias move:", biasMove, "=>", Bias);
        //Log.Finish();





        // old version
        //
        //for (int i = 0; i < WindowHeight; i++)
        //{
        //    for (int j = 0; j < WindowWidth; j++)
        //    {

        //        prevMacroUnits.Reset();
        //        while (prevMacroUnits.Alive)
        //        {
        //            weight = 0;
        //            for (int k = 0; k < Height; k++)
        //            {
        //                for (int l = 0; l < Width; l++)
        //                {
        //                    weight += ConvolutionalNeuralNetwork.ActivationFunction.GetDerivative(Neurons[i, j].NodeValue) * prevMacroUnits.Get().Neurons[(k - 1) * Stride - Padding + i, (l - 1) * Stride - Padding + j].Result;
        //                }
        //            }
        //            Weights.Get().Selector2.Contents -= weight;

        //            prevMacroUnits.Next();
        //        }
        //    }
        //}
    }

    void initializeWeights()
    {
        for (int s = 1; s < WindowWidth; s++)
        {
            Weights.Get().Append2(0f);
        }
        for (int r = 1; r < WindowHeight; r++)
        {
            Weights.Get().Append1(0f);
            for (int s = 1; s < WindowWidth; s++)
            {
                Weights.Get().Append2(0f);
            }
        }
    }

    void initializeNeurons()
    {
        Neurons = new PixelNeuron[Height, Width];
        Bias = 0f;

        for (int k = 0; k < Height; k++)
        {
            for (int l = 0; l < Width; l++)
            {
                Neurons[k,l] = new PixelNeuron();
            }
        }
    }

    public override string ToString()
    {
        string text = "";
        for (int i = 0; i < Neurons.GetLength(0); i++)
        {
            for (int j = 0; j < Neurons.GetLength(1); j++)
            {
                string num = Neurons[i, j].Result.ToString();
                text += num.Substring(0,Mathf.Min(90,num.Length)) + "; ";
            }
            text += "\r\n";
        }
        return text;
    }
    //public float CalculateSoftMax(float divisor)
    //{
    //    return Single.Result / divisor;
    //}

}