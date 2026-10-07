using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelOpener : MonoBehaviour
{
    public Text HeaderTextObject;
    public string HeaderText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        int count = transform.parent.childCount;
        for (int i = 0; i < count; i++)
        {
            transform.parent.GetChild(i).gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
        HeaderTextObject.gameObject.SetActive(true);
        HeaderTextObject.text = HeaderText;
    }

    public void Close()
    {
        Application.Quit();
    }
}
