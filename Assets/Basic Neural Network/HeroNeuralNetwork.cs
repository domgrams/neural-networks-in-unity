using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrainingSample
{
    public float[] Inputs;
    public float[] Outputs;

    public TrainingSample(float[] inputs, float[] outputs)
    {
        Inputs = inputs;
        Outputs = outputs;
    }

    public override string ToString()
    {
        string text = "inputs ";
        foreach (float input in Inputs)
        {
            text += input + "; ";
        }
        text += "outputs ";
        foreach (float input in Outputs)
        {   
            text += input + "; ";
        }
        return text;
    }
}

public class TrainingSet
{

    public LinkedChain<TrainingSample> Samples;

}

public struct Slope
{
    public float right;
    public float left;

    public Slope(float right)
    {
        this.right = right;
        left = float.NaN;
    }

    public Slope(float right, float left)
    {
        this.right = right;
        this.left = left;
    }

    public float GetGradient()
    {
        if (left is float.NaN) 
            return right;

        if (right < 0 && right >= -left)
            return right;
        else if (left > 0 && left >= -right)
            return left;
        else return 0;
    }

    public override string ToString()
    {
        if(left is float.NaN)
        {
            return right.ToString();
        }
        else
        {
            return "(" + left + ", " + right + ")";
        }
    }

    

    public static Slope operator +(Slope a, Slope b)
    {
        if (a.left is float.NaN) return b + a.right;
        if (b.left is float.NaN) return a + b.right;
        return new Slope(a.right + b.right, a.left + b.left);
    }
    public static Slope operator *(Slope a, Slope b)
    {
        if (a.left is float.NaN) return b * a.right;
        if (b.left is float.NaN) return a * b.right;
        return new Slope(a.right * b.right, a.left * b.left);
    }
    public static Slope operator +(Slope a, float f)
    {
        return new Slope(a.right + f, a.left + f);
    }
    public static Slope operator *(Slope a, float f)
    {
        return new Slope(a.right * f, a.left * f);
    }
    public static Slope operator +(float f, Slope a)
    {
        return new Slope(a.right + f, a.left + f);
    }
    public static Slope operator *(float f, Slope a)
    {
        return new Slope(a.right * f, a.left * f);
    }

    public static implicit operator Slope(int v)
    {
        return new Slope(v);
    }
}


public class HeroNeuralNetwork
{
    public static BiasNeuron BiasNeuron;

    public InputHeroNeuron[] Inputs;
    public AbstractNeuron[] Outputs;
    public NeuronFunction Function;

    public float LearningStrength = 0.01f;

    public float Error;
    public int Time;

    /// <summary>
    /// Initialize the Hero Neural Network using fully connected layered structure - according to the layerSizes
    /// </summary>
    /// <param name="layerSizes">the number of neurons in each layer, including the input layer as the first parameter</param>
    public HeroNeuralNetwork(params int[] layerSizes)
    {
        BiasNeuron = new BiasNeuron(1);

        // initialize input neurons
        Inputs = new InputHeroNeuron[layerSizes[0]];
        AbstractNeuron[] lastLayer = new AbstractNeuron[layerSizes[0] + 1];

        for (int i = 0; i < layerSizes[0]; i++)
        {
            Inputs[i] = new InputHeroNeuron(0);
            lastLayer[i] = Inputs[i];
        }
        lastLayer[lastLayer.Length - 1] = BiasNeuron;
        // initialize every other neuron
        AbstractNeuron[] thisLayer;
        HeroNeuron heroNeuron;
        for (int i = 1; i < layerSizes.Length; i++)
        {

            thisLayer = new AbstractNeuron[layerSizes[i] 
                + (i == layerSizes.Length - 1 ? 0 : 1)];    

            for (int j = 0; j < layerSizes[i]; j++)
            {
                //Debug.Log("init " + i + ", " + j + "/" + thisLayer.Length);
                heroNeuron = new HeroNeuron(this);
                heroNeuron.SetInputs(lastLayer);
                thisLayer[j] = heroNeuron;
            }

            lastLayer = thisLayer;

            if (i != layerSizes.Length - 1)
            {
                lastLayer[lastLayer.Length - 1] = BiasNeuron;
            }
            
        }
        Outputs = lastLayer;

        foreach(AbstractNeuron neuron in Outputs)
        {
            neuron.ConnectToOutput();
        }

        Function = new SReLU(1);
        Time = 0;
    }

    public void Randomize()
    {
        UnityEngine.Random.InitState(3);
        deupdateAll();
        foreach(AbstractNeuron neuron in Outputs)
        {
            neuron.Randomize();
        }
    }

    public void Run(float[] inputs)
    {
        deupdateAll();
        for (int i = 0; i < inputs.Length; i++)
        {
            Inputs[i].SetOutputAndUpdate(inputs[i]);
        }


        foreach (AbstractNeuron neuron in Outputs)
        {
            neuron.CalculateOutput();
        }
    }

    public float[] Test(float[] inputs, float[] outputs)
    {
        Run(inputs);

        float[] errorDerivatives = new float[outputs.Length];
        for (int i = 0; i < outputs.Length; i++)
        {
            //Debug.Log(outputs[i] + " to " + Outputs[i].Output);

            errorDerivatives[i] = calculateErrorDerivative(Outputs[i].Output, outputs[i]);
        }
        return errorDerivatives;
    }


    public float TrainBySet(TrainingSet tSet)
    {
        Error = 0;


        resetAllWeightGradients();

        tSet.Samples.Reset();
        bool notDifferentiable = false;

        //trainBySample(
        //    tSet.Samples.Start.GetNext(UnityEngine.Random.Range(0,tSet.Samples.Start.Depth)).Contents
        //    );
        while (tSet.Samples.Alive)
        {
            //if(UnityEngine.Random.value > 0.1)
            {
                notDifferentiable |= trainBySample(tSet.Samples.Get());
            }
            //Debug.Log("train inp: "+tSet.Samples.Get().ToString());
            tSet.Samples.Next();
        }

        if (notDifferentiable)
        {
            Debug.Log($"Non-differentiable at time ({Time}).");
            heroAdaptWeights();
        }
        else
        {
            adaptWeights();
        }
        Time++;
        return Error;
    }



    private bool trainBySample(TrainingSample tSample)
    {
        //Debug.Log("Function: "+tSample.ToString());
        float[] errorDerivatives = Test(tSample.Inputs, tSample.Outputs);
        
        Error += CalculatePrimitiveError(errorDerivatives);

        //{
        //    string x = "Network: ";
        //    foreach (AbstractNeuron neuron in Inputs)
        //    {
        //        x += neuron.Output + "; ";
        //    }
        //    x +=     "outputs: ";
        //    foreach (AbstractNeuron neuron in Outputs)
        //    {
        //        x += neuron.Output + "; ";
        //    }

        //    Debug.Log(x + Error);
        //}

        resetAllNodeValues();

        //Debug.Log("errorDer:" + errorDerivative);
        return backpropagation(errorDerivatives);
    }

    private bool backpropagation(float[] errors)
    {
        deupdateAll();

        bool notDifferentiable = false;
        for (int i = 0; i < Outputs.Length; i++)
        {
            notDifferentiable |= Outputs[i].BackpropStep(new Slope(errors[i]));
        }
        return notDifferentiable;
    }

    private void adaptWeights()
    {
        deupdateAll();



        foreach (AbstractNeuron neuron in Outputs)
        {
            neuron.AdaptWeights();
        }
    }
    private void heroAdaptWeights()
    {
        deupdateAll();


        bool heroFound = false;
        foreach (AbstractNeuron neuron in Outputs)
        {
            if(!heroFound)
                heroFound |= neuron.HeroAdaptWeights();
        }
        if (!heroFound)
        {
            Debug.Log("Hero not found, it may be a local minimum");
        }
    }
    private void deupdateAll(bool all = false)
    {
        foreach (AbstractNeuron neuron in Outputs)
        {
            neuron.DeupdateAll(all);
        }
    }

    private void resetNeuralNetwork()
    {

    }

    private void resetAllWeightGradients()
    {
        deupdateAll();
        foreach (AbstractNeuron neuron in Outputs)
        {
            neuron.ResetAllWeightGradients();
        }
    }
    private void resetAllNodeValues()
    {
        deupdateAll();
        foreach (AbstractNeuron neuron in Outputs)
        {
            neuron.ResetAllNodeValues();
        }
    }
    public float CalculatePrimitiveError(float[] errorDerivativeList)
    {
        float error = 0;
        foreach(float der in errorDerivativeList)
        {
            error += 0.5f * der * der;
        }
        return error;
    }
    private float calculateErrorDerivative(float output, float expectedOutput)
    {
        return (output - expectedOutput) ;
    }

    private float damp(float x)
    {
        return x > 1 ? 1 : x;
    }
    private float dampDerivative(float x)
    {
        return x > 1 ? 0 : 1;
    }

    public float GetRandomWeight()
    {
        return (UnityEngine.Random.value) - 0.4f;
    }
}

public abstract class AbstractNeuron
{
    protected HeroNeuralNetwork neuralNetwork;
    public float Output = 0;
    public bool Updated = false;

    public virtual float CalculateOutput()
    {
        if (Updated)
        {
            //Debug.Log(Output);
            return Output;
        }
        else
        {
            Output = 0;
            Updated = true;
            return Output;
        }
    }

    public virtual void DeupdateAll(bool all = false)
    {
        Updated = false;
    }

    public virtual void ResetAllWeightGradients()
    {
        Updated = true;
        return;
    }
    public virtual void ResetAllNodeValues()
    {
        Updated = true;
        return;
    }
    public virtual void ResetAll()
    {
        Updated = true;
        Output = 0;
        return;
    }

    public virtual void Randomize()
    {
        Updated = true;
        return;

    }


    public virtual bool BackpropStep(Slope derivative)
    {
        //if (Output == 0)
        //{
        //    Debug.Log(GetInfo() + ", "+ true + ", "+Output+", " + derivative);
        //}
        Updated = true;
        return false;
    }

    public virtual void AdaptWeights()
    {

        Updated = true;
        return;
    }

    public virtual bool HeroAdaptWeights()
    {
        return false;
    }

    public virtual void ConnectToOutput()
    {
    }

    public virtual string GetInfo()
    {
        return "Abstract Neuron";
    }
}

public class InputHeroNeuron : AbstractNeuron
{
    public InputHeroNeuron(float output)
    {
        Output = output;
    }

    public void SetOutputAndUpdate(float y)
    {
        Updated = true;
        Output = y;
    }

    public override string GetInfo()
    {
        return "Input Hero Neuron";
    }
}

public class BiasNeuron : AbstractNeuron
{
    public override void ResetAll()
    {

        Updated = true;
        Output = 1;
        return;
    }
    public override float CalculateOutput()
    {
        if (Updated)
        {
            return Output;
        }
        else
        {
            Output = 1;
            Updated = true;
            return Output;
        }
    }
    public BiasNeuron(float output)
    {
        Output = output;
    }

    public override string GetInfo()
    {
        return "Bias Neuron";
    }
}

public class HeroNeuron : AbstractNeuron
{
    int outputCountTotal = 0;
    public int OutputCount;

    public LinkedChain<float> Weights;
    public LinkedChain<Slope> WeightGradients;
    public AbstractNeuron[] Inputs;
    public float Sum;

    public Slope NodeValue;

    public HeroNeuron(HeroNeuralNetwork heroNeuralNetwork)
    {
        neuralNetwork = heroNeuralNetwork;
    }

    public override void DeupdateAll(bool all = false)
    {
        if (!Updated && !all) return;

        foreach (AbstractNeuron neuron in Inputs)
        {
            neuron.DeupdateAll(all);
        }
        base.DeupdateAll();
    }

    public override float CalculateOutput()
    {
        if (Updated) return Output;

        Sum = 0;
        Weights.Reset();
        foreach (AbstractNeuron neuron in Inputs)
        {
            Sum += Weights.Get() * neuron.CalculateOutput();
            Weights.Next();
        }
        Output = neuralNetwork.Function.Get(Sum);
        Updated = true;
        return Output;
    }

    public override void ResetAllWeightGradients()
    {
        if (Updated) return;

        WeightGradients.Reset();
        while (WeightGradients.Alive)
        {
            WeightGradients.Selector.Contents = 0;
            WeightGradients.Next();
        }

        foreach (AbstractNeuron neuron in Inputs)
        {
            neuron.ResetAllWeightGradients();
        }

        Updated = true;
    }

    public override void ResetAllNodeValues()
    {
        if (Updated) return;

        NodeValue = 0;

        Updated = true;
    }

    public override void ResetAll()
    {
        if (Updated) return;

        Sum = 0;
        Weights.Reset();
        while (Weights.Alive)
        {
            Weights.Selector.Contents = 0;
            Weights.Next();
        }


        foreach (AbstractNeuron neuron in Inputs)
        {
            neuron.ResetAll();
        }

        Updated = true;
    }

    public override bool BackpropStep(Slope derivative)
    {

        OutputCount++;
        NodeValue += derivative;

        if (OutputCount != outputCountTotal)
        {
            return false;
        }
        //Debug.Log(OutputCount);
        OutputCount = 0;
        

        Slope derivativeSlope = neuralNetwork.Function.GetDerivativeSlope(Sum);
        NodeValue *= derivativeSlope;

        //Debug.Log("aks"+Sum);
        bool alreadyNonDifferentiable = false;

        WeightGradients.Reset();
        Weights.Reset();
        foreach (AbstractNeuron neuron in Inputs)
        {
            WeightGradients.Selector.Contents +=
                NodeValue           // dE/dx
                * neuron.Output;    // dx/dw


            alreadyNonDifferentiable |= neuron.BackpropStep(
                NodeValue           // dE/dx
                * Weights.Get()     // dx/dy
                );
            

            WeightGradients.Next();
            Weights.Next();
        }

        Updated = true;

        //if(derivativeSlope.left != float.NaN || alreadyNonDifferentiable)
        //{
        //    Debug.Log("aka"+derivativeSlope.right + " " + alreadyNonDifferentiable);
        //}

        return !(derivativeSlope.left is float.NaN) || alreadyNonDifferentiable;
    }

    public override void AdaptWeights()
    {
        if (Updated) return;

        

        Weights.Reset();
        WeightGradients.Reset();
        while (Weights.Alive)
        {
            Weights.Selector.Contents -= neuralNetwork.LearningStrength * WeightGradients.Get().GetGradient();
            WeightGradients.Next();
            Weights.Next();
        }


        foreach (AbstractNeuron neuron in Inputs)
        {
            neuron.AdaptWeights();
        }

        Updated = true;
    }

    public override bool HeroAdaptWeights()
    {
        if (Updated) return false;



        Weights.Reset();
        WeightGradients.Reset();
        bool alreadyNonDifferentiable = false;
        foreach (AbstractNeuron neuron in Inputs)
        {
            if (!(WeightGradients.Get().left is float.NaN))
            {
                if (neuron.Output != 0 && WeightGradients.Get().GetGradient() != 0)
                {
                    Debug.Log("non "+neuron.GetInfo() + " is the hero and also this is interesting: "+neuron.Output);
                    Debug.Log("non " + Weights.Selector.Contents + ", " + WeightGradients.Get().GetGradient());
                    Weights.Selector.Contents -= Mathf.Clamp(WeightGradients.Get().GetGradient(),-1f,1f);   // E/w
                    Debug.Log("non " + Weights.Selector.Contents);
                    return true;
                }

                alreadyNonDifferentiable = true;
            }
            WeightGradients.Next();
            Weights.Next();
        }

        if (!alreadyNonDifferentiable)
        {
            foreach (AbstractNeuron neuron in Inputs)
            {
                if (neuron.HeroAdaptWeights())
                {
                    return true;
                }
            }
        }

        Updated = true;
        return false;
    }

    public void SetInputs(AbstractNeuron[] inputs)
    {
        Inputs = inputs;

        Weights = new LinkedChain<float>(0);
        WeightGradients = new LinkedChain<Slope>(0);
        
        if (inputs[0] is HeroNeuron) (inputs[0] as HeroNeuron).outputCountTotal++;

        for (int i = 1; i < inputs.Length; i++)
        {
            Weights.Append(0);
            WeightGradients.Append(0);

            if (inputs[i] is HeroNeuron) (inputs[i] as HeroNeuron).outputCountTotal++;
        } 

    }

    public override void Randomize()
    {
        if (Updated) return;

        Weights.Reset();
        while (Weights.Alive)
        {
            Weights.Selector.Contents = neuralNetwork.GetRandomWeight();
            Weights.Next();
        }


        foreach (AbstractNeuron neuron in Inputs)
        {
            neuron.Randomize();
        }

        Updated = true;
    }

    public override void ConnectToOutput()
    {
        outputCountTotal++;
    }


    public override string GetInfo()
    {
        return "Hero Neuron";
    }
}