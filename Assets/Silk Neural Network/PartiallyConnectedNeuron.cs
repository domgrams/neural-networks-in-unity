using System.Collections.Generic;
using UnityEngine;

public class PartiallyConnectedNeuron : FloatNeuronWithNodeValue, IBiasedNeuron, IWeightedNeuron1
{
    public override float NodeValue { get { return nodeValue; } set { nodeValue = value; } }
    public override float Result { get { return result; } set { result = value; } }
    public override float ResultBeforeFun { get { return resultBeforeFun; } set { resultBeforeFun = value; } }


    private float result;
    private float resultBeforeFun;
    private float nodeValue;

    public int[] WeightKeys;
    private float[] WeightValues;
    private float Bias;

    public PartiallyConnectedNeuron(int[] weightKeys)
    {
        WeightKeys = weightKeys;
        WeightValues = new float[weightKeys.Length];

    }

    //private int GetWeightIndex(int key)
    //{
    //    int high = WeightValues.Length;
    //    int low = 0;
    //    int i;

    //    while (true)
    //    {
    //        i = (high + low) / 2;

    //        if (WeightKeys[i] == key)
    //        {
    //            return i;
    //        }
    //        else if (WeightKeys[i] > key)
    //        {
    //            high = i;
    //        }
    //        else
    //        {
    //            low = i;
    //        }

    //        if(high == low)
    //        {

    //            if(key == WeightKeys[i])
    //            {
    //                return i;
    //            }
    //            else
    //            {
    //                return -1;
    //            }
    //        }
    //    }

    //}
    
    public override float Run(FloatNeuron[] prevLayer, NeuronFunction fun)
    {
        float final = 0;
        for (int i = 0; i < WeightKeys.Length; i++)
        {
            final += prevLayer[WeightKeys[i]].Result * WeightValues[i];
        }
        final += Bias;

        ResultBeforeFun = final;
        Result = fun.Get(final);
        return Result;
    }

    public override Neuron Randomize()
    {
        for (int i = 0; i < WeightValues.Length; i++)
        {
            WeightValues[i] = (Random.value - 0.5f) * 0.1f;
        }
        Bias = Random.value * 0.2f;
        return this;
    }


    public float GetBias()
    {
        return Bias;
    }

    public float GetWeight(int i)
    {
        if (i == -1) return 0f;
        return WeightValues[i];
    }

    public void SetBias(float b)
    {
        Bias = b;
    }

    public void SetWeight(int i, float w)
    {

        if (i != -1)
            WeightValues[i] = w;
    }
}