using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorAIPanel : MonoBehaviour
{
    public ColorAI colorAI;
    public Transform actionPanels;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setAIActive()
    {
        if(!colorAI.gameObject.activeSelf)
            colorAI.gameObject.SetActive(true);
    }

    public void OnActionChange(UnityEngine.UI.Dropdown change)
    {
        int selectedIndex = change.value;
        setAIActive();

        colorAI.Training = selectedIndex == 2;
        colorAI.Testing = selectedIndex == 1;
        colorAI.InAction = selectedIndex != 0;

        colorAI.visualization.gameObject.SetActive(colorAI.Training);

        for (int i = 0; i < actionPanels.childCount; i++)
        {
            actionPanels.GetChild(i).gameObject.SetActive(false);
        }

        actionPanels.GetChild(selectedIndex).gameObject.SetActive(true);
    }

    public void OnFunctionChange(UnityEngine.UI.Dropdown change)
    {
        int i = change.value;
        setAIActive();

        ColorAI.function = i;
    }
}
