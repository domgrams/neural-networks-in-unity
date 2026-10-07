
using System;
using System.Collections.Generic;
using UnityEngine;

public class BinaryToConvolutionalDataPointLoader
{
    public BinaryToConvolutionalDataPointLoader(int channelSize, int macroUnitWidth, int macroUnitHeight)
    {
        this.channelSize = channelSize;
        this.macroUnitWidth = macroUnitWidth;
        this.macroUnitHeight = macroUnitHeight;
        this.channelByteSize = macroUnitHeight * macroUnitWidth;
        this.sampleByteSize = channelSize * channelByteSize + 1 /* 1 byte for the ouput */;
    }

    private int channelSize;
    private int macroUnitWidth;
    private int macroUnitHeight;

    private int channelByteSize;
    private int sampleByteSize;


    private byte[] Dataset = null;
    private int datasetSize = 0;

    public void LoadData(byte[] data)
    {
        Dataset = data;
        datasetSize = Dataset.Length / sampleByteSize;
    }

    public void ClearData()
    {
        Dataset = null;
        datasetSize = 0;
    }

    public ConvolutionalDataPoint DecryptSample(int sampleIndex)
    {
        //if (sampleIndex >= datasetSize) return null;

        MacroUnit mu = new MacroUnit(macroUnitWidth, macroUnitHeight);
        LinkedChain<MacroUnit> macroUnits = new LinkedChain<MacroUnit>(mu);


         int startByteIndex = sampleByteSize * sampleIndex;
        int endByteIndex = startByteIndex + sampleByteSize;

        // output is stored at the start of the sample, as a number 0 to 9 referencing the images
        int outputNumber = Dataset[startByteIndex];

        int currentValue;
        int byteIndex = startByteIndex + 1;
        int channelIndex = 0;
        while (byteIndex < endByteIndex)
        {
            currentValue = Dataset[byteIndex];

            Debug.Log(channelIndex + ", "+ macroUnitWidth + ", "+channelIndex / macroUnitWidth + ", "+channelIndex % macroUnitWidth);
            
            if (channelIndex >= channelByteSize)
            {
                channelIndex = 0;
                macroUnits.Append(new MacroUnit(macroUnitWidth, macroUnitHeight));
            }

            macroUnits.Get().Neurons[channelIndex / macroUnitWidth, channelIndex % macroUnitWidth].Result = currentValue / 255f; // RGB(int) -> RGB(float01)

            channelIndex++;
            byteIndex++;

        }

        ConvolutionalDataPoint cdp = new ConvolutionalDataPoint(macroUnits, outputNumber);
        return cdp;
    }

    public int GetDatasetSize()
    {
        return datasetSize;
    }
}