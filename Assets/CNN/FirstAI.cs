using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConvolutionalDataPoint
{
    public LinkedChain<MacroUnit> Inputs;
    public int Output;

    public ConvolutionalDataPoint(ConvolutionalNeuralNetwork CNN, Texture2D texture, int output)
    {
        Inputs = new LinkedChain<MacroUnit>(new MacroUnit(CNN.InputMacroUnitWidth, CNN.InputMacroUnitHeight));
        for (int i = 1; i < CNN.InputChannels; i++)
        {
            Inputs.Append(new MacroUnit(CNN.InputMacroUnitWidth, CNN.InputMacroUnitHeight));
        }
        for (int i = 0; i < CNN.InputMacroUnitHeight; i++)
        {
            for (int j = 0; j < CNN.InputMacroUnitWidth; j++)
            {
                Color pixel = texture.GetPixel(j, i);
                Inputs.Reset();
                //Debug.Log(pixel.r);
                Inputs.Get().Neurons[i, j].Result = pixel.r;
                Inputs.Next().Get().Neurons[i, j].Result = pixel.g;
                Inputs.Next().Get().Neurons[i, j].Result = pixel.b;

            }
        }
        Output = output;
    }
    public ConvolutionalDataPoint(LinkedChain<MacroUnit> macroUnits, int output)
    {
        Inputs = macroUnits;
        Output = output;
    }
}

[System.Serializable]
public class TrainImage
{
    public Texture2D Image;
    public int Result;

    public TrainImage(Texture2D image, int result)
    {
        Image = image;
        Result = result;
    }
}

public enum DatasetOptions
{
    Manual,
    LoadFromFile
}

public class FirstAI : MonoBehaviour
{
    ConvolutionalNeuralNetwork CNN;


    public Texture2D TestImage;


    public int InputImageWidth;
    public int InputImageHeight;
    public DatasetOptions LoadDatasetAs;

    public List<TrainImage> TrainImages;
    private List<ConvolutionalDataPoint> ReadyDataPoints;

    public string DatasetFilePath;
    private BinaryToConvolutionalDataPointLoader binaryToConvolutaionalLoader;


    public int FPS;
    public int TPF;
    public int NT;
    public float Strength;
    public float TrainError;


    public bool InitializeNetwork = false;
    public bool Test = false;
    public bool Train = false;
    public bool UpdateTrainDataSet = false;



    public List<float> results;



    // Start is called before the first frame update
    void Start()
    {
        initializeNetwork();
        ReadyDataPoints = new List<ConvolutionalDataPoint>();
    }

    void initializeNetwork()
    {
        NT = 0;

        CNN = new ConvolutionalNeuralNetwork(6);
        CNN.SetInputParameters(3, InputImageWidth, InputImageHeight);
        CNN.AddLayer(LayerTypeCNN.Convolutional, 3, 3, 2);
        CNN.AddLayer(LayerTypeCNN.Convolutional, 3, 3, 2);
        CNN.AddLayer(LayerTypeCNN.Convolutional, 3, 3, 2);
        CNN.AddLayer(LayerTypeCNN.Classic, size: 1200);
        CNN.AddLayer(LayerTypeCNN.Classic, size: 600);
        CNN.AddLayer(LayerTypeCNN.Classic, size: 10);

        //CNN.Print();
    }

    
    void Update()
    {
        FPS = (int)(1 / Time.deltaTime);
        if (Train)
        {
            if (UpdateTrainDataSet)
            {
                UpdateTrainDataSet = false;


                if (LoadDatasetAs == DatasetOptions.Manual)
                {
                    ReadyDataPoints.Clear();
                    foreach (TrainImage trainImage in TrainImages)
                    {
                        ReadyDataPoints.Add(new ConvolutionalDataPoint(CNN, trainImage.Image, trainImage.Result));
                    }
                }
                else if(LoadDatasetAs == DatasetOptions.LoadFromFile)
                {
                    binaryToConvolutaionalLoader = new BinaryToConvolutionalDataPointLoader(3, 32, 32);
                    binaryToConvolutaionalLoader.LoadData(System.IO.File.ReadAllBytes(DatasetFilePath));
                    Debug.Log(binaryToConvolutaionalLoader.GetDatasetSize());
                }
            }
            float error = 0f;

            Debug.Log("training");
            Debug.Log("to turn it on, enable random.range");
            if (LoadDatasetAs == DatasetOptions.Manual)
            {
                for (int i = 0; i < TPF; i++)
                {
                    int randomState = 0;//Random.Range(0, ReadyDataPoints.Count - 1);
                    error += CNN.Train(ReadyDataPoints[randomState].Inputs, ReadyDataPoints[randomState].Output, Strength * 0.01f);
                }
            }
            else if(LoadDatasetAs == DatasetOptions.LoadFromFile)
            {
                Debug.Log("from file training");
                for (int i = 0; i < TPF; i++)
                {
                    int randomState = 0;//Random.Range(0, binaryToConvolutaionalLoader.GetDatasetSize());
                    
                    ConvolutionalDataPoint cdp = binaryToConvolutaionalLoader.DecryptSample(randomState);
                    
                    cdp.Inputs.Reset();
                    while (cdp.Inputs.Alive)
                    {
                        Debug.Log(cdp.Inputs.Get().ToString());
                        cdp.Inputs.Next();
                    }

                    error += CNN.Train(cdp.Inputs, cdp.Output, Strength * 0.01f);
                }
            }
            NT += TPF;
            TrainError = error;
        }
        if (!Train && Test && TestImage != null)
        {
            Test = false;

            LinkedChain<MacroUnit> macroUnits = new LinkedChain<MacroUnit>(new MacroUnit(CNN.InputMacroUnitWidth, CNN.InputMacroUnitHeight));
            for (int i = 1; i < CNN.InputChannels; i++)
            {
                macroUnits.Append(new MacroUnit(CNN.InputMacroUnitWidth, CNN.InputMacroUnitHeight));
            }
            for (int i = 0; i < CNN.InputMacroUnitHeight; i++)
            {
                for (int j = 0; j < CNN.InputMacroUnitWidth; j++)
                {
                    Color pixel = TestImage.GetPixel(j, i);
                    macroUnits.Reset();
                    //Debug.Log(pixel.r);
                    macroUnits.Get().Neurons[i, j].Result = pixel.r;
                    macroUnits.Next().Get().Neurons[i, j].Result = pixel.g;
                    macroUnits.Next().Get().Neurons[i, j].Result = pixel.b;
                    
                }
            }
            

            
            macroUnits.Reset();
            while(macroUnits.Alive)
            {
                Debug.Log(macroUnits.Get().ToString());
                macroUnits.Next();
            }

            LinkedChain<MacroUnit> finalLayer = CNN.Run(macroUnits);

            finalLayer.Reset();
            results = new List<float>();
            while (finalLayer.Alive)
            {
                results.Add(finalLayer.Get().Single.Result);
                finalLayer.Next();
            }
            //results.Sort();
        }
        if (!Train && InitializeNetwork)
        {
            InitializeNetwork = false;
            initializeNetwork();
        }
    }

}
