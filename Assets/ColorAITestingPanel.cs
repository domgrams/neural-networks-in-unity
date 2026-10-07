using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorAITestingPanel : MonoBehaviour
{
    Color[] colors = new Color[] { Color.black, Color.white, Color.red, Color.green, Color.blue, Color.yellow, new Color(1,0.4f,0f), new Color(0.5f, 0.2f, 0f), new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 0f, 0.4f), new Color(1f,0f,0.8f),  new Color(0,0.5f,0.5f) };
    public ColorPicker colorPicker;
    public ColorAI colorAI;
    public Text errorText;
    public RectTransform ColorNameGuessList;
    public GameObject ColorNameGuessSliderPrefab;
    public int ListSpace;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            Slider newSlider = Instantiate(ColorNameGuessSliderPrefab, ColorNameGuessList).GetComponent<Slider>();
            ColorBlock colorBlock = newSlider.colors;
            colorBlock.disabledColor = colors[i];
            newSlider.colors = colorBlock;
        }
    }

    // Update is called once per frame
    void Update()
    {
        colorAI.color = colorPicker.color;
        colorAI.colorName = colorPicker.colorName;
        errorText.text = "Error: "+colorAI.ErrorValue.ToString();


        for (int i = 0; i < ColorNameGuessList.childCount; i++)
        {
            RectTransform transform = (RectTransform)ColorNameGuessList.GetChild(i);

            int j = 0;
            while ((int)colorAI.chances[j].colorName != i) j++;



            transform.gameObject.SetActive(true);
            transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, -20 - (ListSpace) * j);
            transform.GetComponent<Slider>().value = colorAI.chances[j].chance;
        }
    }
}
