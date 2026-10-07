using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorAITrainingPanel : MonoBehaviour
{

    public ColorAI colorAI;
    private ColorDispatcher colorDispatcher;
    public Toggle toggle;
    public Text samplesTrainedText;
    public Text errorValueText;
    public InputField trainingStrengthText;
    public InputField trainingSamplesPerFrameText;
    public InputField log;


    // Start is called before the first frame update
    void Start()
    {
        colorDispatcher = colorAI.GetComponent<ColorDispatcher>();
        colorDispatcher.load = true;
        colorDispatcher.put = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        colorAI.TPF = SafeIntegers.Convert(trainingSamplesPerFrameText.text, 0);
        //colorAI.strength = SafeFloats.Convert(trainingStrengthText.text, 0f);
        samplesTrainedText.text = colorAI.NumberOfTrainSamples.ToString() + " Samples Trained";
        errorValueText.text = (colorAI.ErrorValue / 100f).ToString() + "% error";
        log.text = Logger.Text;
        colorAI.InAction = toggle.isOn;
    }
    public void RestartAIClicked()
    {
        colorAI.RestartAI = true;
    }

    public void OnShowColorChange(Dropdown dropdown)
    {
        colorAI.visualization.VisibleColors.Add((ColorName)dropdown.value);
        colorAI.visualization.Generate();
    }

    public void OnClearColors()
    {
        colorAI.visualization.VisibleColors.Clear();
        colorAI.visualization.Generate();

    }

}
