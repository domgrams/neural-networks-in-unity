using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeColorAI : MonoBehaviour
{
    public ColorAI colorAI;
    public GameObject Prefab;

    public List<ColorName> VisibleColors;


    int objectIndex = 0;

    public float size;

    public bool generate;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            generate = false;

            List<ColorName> visibleColorsWithoutDuplicates = new List<ColorName>();
            for (int i = 0; i < VisibleColors.Count; i++)
            {
                if (!visibleColorsWithoutDuplicates.Contains(VisibleColors[i]))
                    visibleColorsWithoutDuplicates.Add(VisibleColors[i]);
            }
            VisibleColors = visibleColorsWithoutDuplicates;

            Generate();
        }
    }

    void ShowObject(Color color)
    {
        GameObject currentObject;
        if (transform.childCount <= objectIndex)
        {
            currentObject = Instantiate(Prefab, transform);
        }
        else
        {
            currentObject = transform.GetChild(objectIndex).gameObject;
        }

        float objectScale = 1f / (int)size;
        currentObject.transform.localScale = new Vector3(objectScale, objectScale, objectScale);
        currentObject.transform.localPosition = new Vector3(color.r - 0.5f, color.g - 0.5f, color.b - 0.5f);
        currentObject.GetComponent<Renderer>().material.color = color;
        currentObject.SetActive(true);

        objectIndex++;
    }


    void ClearObjects()
    {
        objectIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void Generate()
    {
        ClearObjects();
        Color currentColor;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    currentColor = new Color(i / size, j / size, k / size);
                    ColorName cName = colorAI.PublicRun(currentColor);
                    if (VisibleColors.Contains(cName))
                    {
                        ShowObject(currentColor);
                    }
                }
            }
        }
    }
}
