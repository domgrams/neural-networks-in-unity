using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;

    public RawImage colorImage;
    public Dropdown dropdown;

    public Color color;
    public ColorName colorName;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
          
    }

    public void SliderChanged()
    {
        color = new Color(sliderR.value, sliderG.value, sliderB.value);

        colorImage.color = color;
    }

    public void SelectedDropdownChanged()
    {
        colorName = (ColorName)dropdown.value;
    }

    
}
