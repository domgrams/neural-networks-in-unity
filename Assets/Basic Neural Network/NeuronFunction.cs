using Unity;
using UnityEngine;

public abstract class NeuronFunction
{
    public abstract float Get(float x);
    public abstract float GetDerivative(float x);
    public abstract Slope GetDerivativeSlope(float x);
    public abstract string GetName();


    public NeuronFunction()
    {

    }
}

public class Atangens : NeuronFunction
{
    private float multiplier;
    public Atangens(float multX)
    {
        multiplier = multX;
    }

    public override float Get(float x)
    {
        return Mathf.Atan(x)  * multiplier;
    }

    public override float GetDerivative(float x)
    {
        return 1 / (x*x+1) * multiplier;
    }
    public override Slope GetDerivativeSlope(float x)
    {
        return new Slope(multiplier);
    }

    public override string GetName()
    {
        return "Arctangens";
    }
}

public class Sigmoid : NeuronFunction
{

    private float multiplier;
    public Sigmoid(float multX)
    {
        multiplier = multX;
    }

    public override float Get(float x)
    {
        if(Mathf.Abs(x) > 200f)
        {
            return x >= 0 ? 1 : 0;
        }
        return 1 / (1 + Mathf.Exp(-x*multiplier));
    }

    public override float GetDerivative(float x)
    {
        float exp = Mathf.Exp(-x * multiplier);
        return (exp) / (1 + 2 * exp + exp * exp);
    }
    public override Slope GetDerivativeSlope(float x)
    {
        float exp = Mathf.Exp(-x * multiplier);
        return new Slope((exp) / (1 + 2 * exp + exp * exp));
    }

    public override string GetName()
    {
        return "Sigmoid";
    }
}

public class PReLU : NeuronFunction
{
    public bool NanIn0 = true;
    private float multiplier;
    public PReLU(float multX)
    {
        multiplier = multX;
    }

    public override float Get(float x)
    {
        return x <= 0 ? x / 20 * multiplier : x * multiplier;
    }

    public override float GetDerivative(float x)
    {
        if (x == 0) 
        {
            Logger.Print("Non-differentiable PReLU found.");
            SilkNeuralNetwork.wasNonDiff = true;
            return NanIn0 ? float.NaN : 0.525f * multiplier; 
        }
        return x < 0 ? multiplier / 20 : multiplier;
    }
    public override Slope GetDerivativeSlope(float x)
    {
        if (x == 0) { return new Slope(0, multiplier); }
        return new Slope(x < 0 ? 0 : multiplier);
    }

    public override string GetName()
    {
        return "PReLU";
    }
}

public class Abs : NeuronFunction
{
    public bool NanIn0 = true;
    private float multiplier;
    public Abs(float multX)
    {
        multiplier = multX;
    }

    public override float Get(float x)
    {
        return Mathf.Abs(x);
    }

    public override float GetDerivative(float x)
    {
        if (x == 0) {
            Logger.Print("Non-differentiable Abs found.");
            SilkNeuralNetwork.wasNonDiff = true; 
            return NanIn0 ? float.NaN : 0f; 
        }
        return x < 0 ? -multiplier : multiplier;
    }
    public override Slope GetDerivativeSlope(float x)
    {
        if (x == 0) { return new Slope(0, multiplier); }
        return new Slope(x < 0 ? 0 : multiplier);
    }

    public override string GetName()
    {
        return "Abs";
    }
}

public class SReLU : NeuronFunction
{

    private float multiplier;
    public SReLU(float multX)
    {
        multiplier = multX;
    }

    public override float Get(float x)
    {
        return x <= 0 ? x * multiplier / 10 : x * multiplier;
    }

    public override float GetDerivative(float x)
    {
        if (x == 0) { Debug.LogWarning("derivative of ReLU at 0"); return float.NaN; }
        return x < 0 ? multiplier / 10 : multiplier;
    }
    public override Slope GetDerivativeSlope(float x)
    {
        if (x == 0) { return new Slope(0, multiplier); }
        return new Slope(x < 0 ? multiplier / 10 : multiplier);
    }

    public override string GetName()
    {
        return "SReLU";
    }
}

public class OhOne : NeuronFunction
{
    private float multiplier;
    public OhOne(float multX)
    {
        multiplier = multX;
    }

    public override float Get(float x)
    {
        return x <= 0 ? 0 : x >= 1 ? multiplier : x * multiplier;
    }

    public override float GetDerivative(float x)
    {
        if (x == 0 || x == 1) { Debug.LogWarning("derivative of ReLU at 0"); return float.NaN; }

        return x <= 0 ? 0 : x >= 1 ? 0 : multiplier;
    }
    public override Slope GetDerivativeSlope(float x)
    {
        if (x == 0) { return new Slope(0, multiplier); }
        if (x == 1) { return new Slope(multiplier, 0); }
        return new Slope(x < 0 ? 0 : multiplier);
    }

    public override string GetName()
    {
        return "OhOne";
    }
}