using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum StateCNN
{
    Initializing,
    Ready,
    Testing,
    Training
}

public class ConvolutionalNeuralNetwork : NeuralNetwork
{

    public static readonly NeuronFunction ActivationFunction = new PReLU(1);

    LayerCNN[] layers;
    LastLayerCNN lastLayer;

    public StateCNN State { get => state; }
    public int InputChannels { get => inputChannels; }
    public int InputMacroUnitWidth { get => inputMacroUnitWidth; }
    public int InputMacroUnitHeight { get => inputMacroUnitHeight; }

    private StateCNN state;
    private int inputChannels;
    private int inputMacroUnitWidth;
    private int inputMacroUnitHeight;



    int layerIndexAdding = 0;
    bool inputParametersSet = false;


    public ConvolutionalNeuralNetwork(int depth)
    {
        Depth = depth;
        state = StateCNN.Initializing;
        layers = new LayerCNN[depth];
    }

    public ConvolutionalNeuralNetwork SetInputParameters(int channels, int muWidth, int muHeight)
    {
        checkRightState(StateCNN.Initializing, "set input parameters");
        inputChannels = channels;
        inputMacroUnitWidth = muWidth;
        inputMacroUnitHeight = muHeight;

        inputParametersSet = true;
        checkReadyInitialized();
        return this;
    }

    public ConvolutionalNeuralNetwork AddLayer(LayerTypeCNN layerType, int windowWidth = 1, int windowHeight = 1, int stride = 1, int size = 3, string info = "")
    {

        checkRightState(StateCNN.Initializing, "add layers");
        LayerCNN newLayerCNN;

        if(layerType == LayerTypeCNN.Convolutional || layerType == LayerTypeCNN.Classic)
        {
            if(layerIndexAdding == Depth - 1)
            {
                newLayerCNN = new LastLayerCNN(layerType);
                lastLayer = newLayerCNN as LastLayerCNN;
            }
            else if(layerIndexAdding == Depth - 2)
            {
                newLayerCNN = new PenultimateLayerCNN(layerType);
            }
            else
            {
                newLayerCNN = new ConvolutionalLayerCNN(layerType);
            }

            if (layerType == LayerTypeCNN.Classic)
            {
                stride = 1;

                if (layerIndexAdding != 0 && layers[layerIndexAdding - 1].LayerType != LayerTypeCNN.Classic)
                {
                    // is flattening
                    LayerCNN previousLayer = layers[layerIndexAdding - 1];
                    windowWidth = previousLayer.MacroUnitWidth;
                    windowHeight = previousLayer.MacroUnitHeight;
                }
                else
                {
                    // is classic
                    windowWidth = 1;
                    windowHeight = 1;
                }
                newLayerCNN.Padding = 0;
                newLayerCNN.MacroUnitHeight = 1;
                newLayerCNN.MacroUnitWidth = 1;
            }
            else if (layerType == LayerTypeCNN.Convolutional)
            {
                int lastLayerNeuronsCount;
                if (layerIndexAdding == 0)
                {
                    lastLayerNeuronsCount = InputChannels*InputMacroUnitHeight * InputMacroUnitWidth;
                    newLayerCNN.Padding = windowWidth / 2;
                    newLayerCNN.MacroUnitWidth = (InputMacroUnitWidth - windowWidth + 2 * newLayerCNN.Padding) / stride + 1;
                    newLayerCNN.MacroUnitHeight = (InputMacroUnitHeight - windowHeight + 2 * newLayerCNN.Padding) / stride + 1;
                }
                else
                {
                    LayerCNN previousLayer = layers[layerIndexAdding - 1];

                    lastLayerNeuronsCount = previousLayer.Size * previousLayer.MacroUnitHeight*previousLayer.MacroUnitWidth;
                    newLayerCNN.Padding = windowWidth / 2;
                    newLayerCNN.MacroUnitWidth = (previousLayer.MacroUnitWidth - windowWidth + 2 * newLayerCNN.Padding) / stride + 1;
                    newLayerCNN.MacroUnitHeight = (previousLayer.MacroUnitHeight - windowHeight + 2 * newLayerCNN.Padding) / stride + 1;
                }
                size = lastLayerNeuronsCount / (newLayerCNN.MacroUnitWidth * newLayerCNN.MacroUnitHeight) + 1;
            }
        }
        else
        {

            newLayerCNN = new PoolingLayerCNN();
            stride = windowWidth;
            windowHeight = windowWidth;
            newLayerCNN.Padding = 0;
            if (layerIndexAdding == 0)
            {
                newLayerCNN.MacroUnitHeight = InputMacroUnitHeight / stride;
                newLayerCNN.MacroUnitWidth = InputMacroUnitWidth / stride;
            }
            else
            {
                LayerCNN previousLayer = layers[layerIndexAdding - 1];
                newLayerCNN.MacroUnitHeight = previousLayer.MacroUnitHeight / stride;
                newLayerCNN.MacroUnitWidth = previousLayer.MacroUnitWidth / stride;
                
            }
        }

        if(info == "")
        {
            info = layerType.ToString() + " Layer at " + layerIndexAdding;
        }

        newLayerCNN.Stride = stride;
        newLayerCNN.Size = size;
        newLayerCNN.WindowWidth = windowWidth;
        newLayerCNN.WindowHeight = windowHeight;
        newLayerCNN.Info = info;

        int previousLayerSize = layerIndexAdding == 0 ? inputChannels : layers[layerIndexAdding - 1].Size;
        newLayerCNN.InitializeLayer(previousLayerSize);
        layers[layerIndexAdding] = newLayerCNN;

        layerIndexAdding++;
        checkReadyInitialized();
        return this;
    }

    public void Print()
    {
        foreach(LayerCNN layer in layers)
        {
            layer.Print();
        }
    }

    public LinkedChain<MacroUnit> Run(LinkedChain<MacroUnit> inputMacroUnits)
    {
        checkRightState(StateCNN.Ready, "run");
        state = StateCNN.Testing;


        layers[0].Run(inputMacroUnits);

        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].Run(layers[i - 1].MacroUnits);
        }

        setReady();
        return lastLayer.MacroUnits;
    }

    public float Test(LinkedChain<MacroUnit> inputMacroUnits, int output)
    {
        checkRightState(StateCNN.Ready, "test");
        state = StateCNN.Testing;

        float error = test(inputMacroUnits, output);

        setReady();
        return error;
    }

    private float test(LinkedChain<MacroUnit> inputMacroUnits, int output)
    {
        layers[0].Run(inputMacroUnits);

        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].Run(layers[i - 1].MacroUnits);
        }
        float error = lastLayer.CrossEntropyLoss(output);

        return error;
    }

    public float Train(LinkedChain<MacroUnit> inputs, int output, float strength = 1)
    {
        checkRightState(StateCNN.Ready, "train");
        state = StateCNN.Training;

        float testError = test(inputs, output);

        lastLayer.CalculateLastLayerNodeValues(output);

        for (int i = Depth - 2; i >= 0; i--)
        {
            layers[i].CalculateNodeValues(layers[i + 1].MacroUnits);
        }

        layers[0].Move(inputs, strength);
        for (int i = 1; i < Depth; i++)
        {
            layers[i].Move(layers[i - 1].MacroUnits, strength);
        }

        setReady();
        return testError;
    }


    private void checkRightState(StateCNN rightState, string exceptionMessage)
    {
        if (state != rightState) throw new InvalidOperationException("cannot "+exceptionMessage+" during " + state.ToString() + " state");

    }

    private void checkReadyInitialized()
    {
        if (layerIndexAdding == Depth && inputParametersSet)
        {
            setReady();
        }
    }
    private void setReady()
    {
        state = StateCNN.Ready;
    }


}

public enum LayerTypeCNN
{
    Convolutional,
    Classic,
    Pooling
}

    
public abstract class LayerCNN
{
    public int Size;
    public LinkedChain<MacroUnit> MacroUnits { get; protected set; }
    public abstract LayerTypeCNN LayerType { get; }

    public int WindowWidth;
    public int WindowHeight;

    public int MacroUnitWidth;
    public int MacroUnitHeight;

    public int Stride;
    public int Padding;

    public string Info;

    public abstract void InitializeLayer(int previousLayerDepth);

    public abstract void Run(LinkedChain<MacroUnit> prevMacroUnits);

    public virtual void CalculateNodeValues(LinkedChain<MacroUnit> backMacroUnits)
    {
    }

    public abstract void Move(LinkedChain<MacroUnit> prevMacroUnits, float strength = 1);


    public void Print()
    {
       Log.Elements(Info,"of size:", Size);
       Log.Elements("Window size:", WindowHeight, ",", WindowWidth,"| MacroUnit size:",MacroUnitHeight,",",MacroUnitWidth);
       Log.Elements("Stride:", Stride, "| Padding:", Padding);
        Log.Finish();
        MacroUnits.Reset();
        int i = 0;
        while (MacroUnits.Alive)
        {
           Log.Elements("MacroUnit #" + i + "\r\n");
            MacroUnits.Get().Print();
            MacroUnits.Next();
            i++;
        }
       Log.Finish();
    }
}

public class ConvolutionalLayerCNN : LayerCNN
{


    public override LayerTypeCNN LayerType { get => layerType; }

    LayerTypeCNN layerType;

    public ConvolutionalLayerCNN(LayerTypeCNN layerType)
    {
        this.layerType = layerType;
    }

    public override void InitializeLayer(int previousLayerDepth)
    {
        MacroUnits = new LinkedChain<MacroUnit>(new MacroUnit(this, previousLayerDepth));
        MacroUnits.Get().Randomize();

        for (int i = 1; i < Size; i++)
        {
            MacroUnits.Append(new MacroUnit(this, previousLayerDepth)).Contents.Randomize();
        }
    }

    public override void Run(LinkedChain<MacroUnit> prevMacroUnits)
    {
        MacroUnits.Reset();
        int i = 0;
        //Log.Elements(Info);
        Debug.Log(Info);
        while (MacroUnits.Alive)
        {
           //Log.Elements("Running",i,"th macrounit");
           //Log.Finish();
           //Debug.Log("from: "+MacroUnits.Get().ToString());
            MacroUnits.Get().Run(prevMacroUnits);
            //Debug.Log("to: " +MacroUnits.Get().ToString());

            MacroUnits.Next();

            i++;
        }
    }


    public override void CalculateNodeValues(LinkedChain<MacroUnit> backMacroUnits)
    {
       //Log.Elements("classic layer valuing", Info);
       //Log.Finish();

        float nodeValue;
        bool debug;

        backMacroUnits.Reset();
        while (backMacroUnits.Alive)
        {
            backMacroUnits.Get().ResetWeightsSelector();
            backMacroUnits.Next();
        }

        // TO DO: RESTRUCTURE CYCLES

        int mu = 0;
        MacroUnits.Reset();
        while (MacroUnits.Alive)
        {
           //Log.Elements("valuing", mu + "th macrounit");
            for (int i = 0; i < MacroUnitHeight; i++)
            {
                for (int j = 0; j < MacroUnitWidth; j++)
                {
                    nodeValue= 0f;
                    backMacroUnits.Reset();

                   //Log.Elements("nodeValues",i, "/",j,":");

                    while (backMacroUnits.Alive)
                    {
                        nodeValue += backMacroUnits.Get().CalculateHiddenLayerNodeValue(i, j, true);
                        backMacroUnits.Next();
                    }

                   //Log.Elements("=",nodeValue);
                   //Log.Finish();

                    MacroUnits.Get().Neurons[i, j].NodeValue = nodeValue;
                }
            }

            backMacroUnits.Reset();
            while (backMacroUnits.Alive)
            {
                backMacroUnits.Get().NextWeightsSelector();
                backMacroUnits.Next();
            }
            MacroUnits.Next();
            mu++;
        }
    }

    public override void Move(LinkedChain<MacroUnit> prevMacroUnits, float strength = 1)
    {
       //Log.Elements("moving with weights in ", Info);
        MacroUnits.Reset();
        int mu = 0;
        while (MacroUnits.Alive)
        {
           //Log.Elements("moving", mu, " th macrounit");
            MacroUnits.Get().Move(prevMacroUnits, strength);
            MacroUnits.Next();
            mu++;
        }
    }
}

public class PoolingLayerCNN : LayerCNN
{
    public override LayerTypeCNN LayerType { get => LayerTypeCNN.Pooling; }

    public override void InitializeLayer(int previousLayerDepth)
    {
        MacroUnits = new LinkedChain<MacroUnit>(new MacroUnit(this, previousLayerDepth, true));

        for (int i = 1; i < Size; i++)
        {
            MacroUnits.Append(new MacroUnit(this, previousLayerDepth, true));
        }
    }

    public override void Move(LinkedChain<MacroUnit> prevMacroUnits, float strength = 1)
    {

    }

    public override void Run(LinkedChain<MacroUnit> prevMacroUnits)
    {
        MacroUnits.Reset();
        prevMacroUnits.Reset();
        while (MacroUnits.Alive)
        {
            MacroUnits.Get().RunPooling(prevMacroUnits.Get());
            MacroUnits.Next();
            prevMacroUnits.Next();
        }
    }
}

public class PenultimateLayerCNN : ConvolutionalLayerCNN
{

    public override LayerTypeCNN LayerType { get => layerType; }

    LayerTypeCNN layerType;

    public PenultimateLayerCNN(LayerTypeCNN layerType) : base (layerType)
    {
    }

    public override void CalculateNodeValues(LinkedChain<MacroUnit> backMacroUnits)
    {
       //Log.Elements("penultimate layer valuing",Info);
       //Log.Finish();
        float nodeValue;
        float partialNodeValue;
        bool debug;

        // reseting just this weights selector
        backMacroUnits.Reset();
        while (backMacroUnits.Alive)
        {
            backMacroUnits.Get().ResetWeightsSelector();
            backMacroUnits.Next();
        }

        int mu = 0;
        MacroUnits.Reset();
        while (MacroUnits.Alive)
        {
           //Log.Elements("valuing", mu+ "th macrounit");
            for (int i = 0; i < MacroUnitHeight; i++)
            {
                for (int j = 0; j < MacroUnitWidth; j++)
                {
                    //debug = UnityEngine.Random.value < 0.01f;

                    nodeValue = 0f;
                    backMacroUnits.Reset();
                   //Log.Elements("penultimate node values at",i,"/",j);
                    while (backMacroUnits.Alive)
                    {
                         partialNodeValue = backMacroUnits.Get().CalculatePenultimateNodeValue();

                       //Log.Elements(partialNodeValue);
                        nodeValue += partialNodeValue;
                        backMacroUnits.Next();
                        
                    }
                   //Log.Elements("=", nodeValue);
                   //Log.Finish();
                    MacroUnits.Get().Neurons[i, j].NodeValue = nodeValue;
                }
            }
            mu++;
            MacroUnits.Next();
        }
    }
}

public class LastLayerCNN : ConvolutionalLayerCNN
{
    public const float ExpLimit = 69f;

    public override LayerTypeCNN LayerType { get => layerType; }

    LayerTypeCNN layerType;

    public LastLayerCNN(LayerTypeCNN layerType) : base (layerType)
    {

    }
    public override void Move(LinkedChain<MacroUnit> prevMacroUnits, float strength = 1)
    {
       //Log.Elements("moving with weights in ", Info);
        MacroUnits.Reset();
        int mu = 0;
        while (MacroUnits.Alive)
        {
           //Log.Elements("moving", mu, " th macrounit");
            MacroUnits.Get().Move(prevMacroUnits, strength, true);
            MacroUnits.Next();
            mu++;
        }
    }
    public override void Run(LinkedChain<MacroUnit> prevMacroUnits)
    {
        // calculating ResultBeforeFun for the last layer
        MacroUnits.Reset();
        while (MacroUnits.Alive)
        {
            MacroUnits.Get().Run(prevMacroUnits);
            Debug.Log("result:" + MacroUnits.Get().Single.ResultBeforeFun);

            MacroUnits.Next();
        }


        // calculating the result
        float resultBeforeFun;
        float divisor;
        float difference;
        bool zero;
        MacroUnits.Reset();
        while (MacroUnits.Alive)
        {
            // the result before it gets calculated
            resultBeforeFun = MacroUnits.Get().Single.ResultBeforeFun;

            // calculating the divisor
            divisor = 0f;
            zero = false;
            LinkedItem<MacroUnit> selectedMacroUnit = MacroUnits.Start;
            while (selectedMacroUnit != null && !zero)
            {
                difference = selectedMacroUnit.Contents.Single.ResultBeforeFun - resultBeforeFun;
                if (difference > ExpLimit)
                {
                    MacroUnits.Get().Single.Result = 0;
                    zero = true;
                }
                else if (difference > -ExpLimit)
                {
                    divisor += Mathf.Exp(difference);
                }


                selectedMacroUnit = selectedMacroUnit.Next;
            }
            if (!zero)
            {
                if (divisor < 1f)
                {
                   //Log.Elements("oh no, divisor is", divisor, "(",difference,")");
                   //Log.Finish();
                    divisor = 1f;
                }
                MacroUnits.Get().Single.Result = 1f / divisor;
            }
            
            MacroUnits.Next();
        }
    }
    
    public float CrossEntropyLoss(int correctIndex)
    {
        MacroUnits.Reset();
        float valueAtTheCorrectIndex = MacroUnits.Start.GetNext(correctIndex).Get().Single.Result;

        if(valueAtTheCorrectIndex < 0.001f)
        {
            return 10000000;
        }
        return Mathf.Log(valueAtTheCorrectIndex);
    }

    public void CalculateLastLayerNodeValues(int correctOutput)
    {
       //Log.Elements("valuing last layer", Info, "with output at", correctOutput);
       //Log.Finish();
        PixelNeuron pixelNeuron;
        MacroUnits.Reset();
        int i = 0;
        while (MacroUnits.Alive)
        {
            pixelNeuron = MacroUnits.Get().Single;
            pixelNeuron.NodeValue = pixelNeuron.Result - (correctOutput == i ? 1 : 0);
            MacroUnits.Next();
           //Log.Elements(i + "th node value", pixelNeuron.Result, "==>",pixelNeuron.NodeValue);
            i++;
        }
    }

}