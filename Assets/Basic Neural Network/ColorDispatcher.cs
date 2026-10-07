using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileEncryption;

public class ColorDispatcher : MonoBehaviour
{
    public List<ColorDataPoint> tempColorDataPoints;
    ColorAI colorAI;
    public Color color;
    public ColorName colorName;
    public bool add;
    public string Path;
    public bool load;
    public bool save;

    public bool put;


    // Start is called before the first frame update
    void Start()
    {
        colorAI = GetComponent<ColorAI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tempColorDataPoints == null)
        {
            tempColorDataPoints = new List<ColorDataPoint>();
        }
        if (load)
        {
            load = false;
            Load(Application.persistentDataPath + "/" + Path);
        }
        if (save)
        {
            save = false;
            Save(Application.persistentDataPath + "/" + Path);
        }
        if (put)
        {
            put = false;
            colorAI = GetComponent<ColorAI>();
            colorAI.trainingSet = new TrainingSet();
            colorAI.colorDataPoints = tempColorDataPoints;
            foreach (ColorDataPoint cdp in tempColorDataPoints)
            {
                if(colorAI.trainingSet.Samples == null)
                {
                    colorAI.trainingSet.Samples = new LinkedChain<TrainingSample>(new TrainingSample(cdp.inputs, cdp.outputs));

                }
                else
                {
                    colorAI.trainingSet.Samples.Append(new TrainingSample(cdp.inputs, cdp.outputs));

                }
            }
        }
        if (add)
        {
            add = false;
            tempColorDataPoints.Add(new ColorDataPoint(color, colorName));

            color = new Color(Random.value, Random.value, Random.value);
        }
    }



    private void OnValidate()
    {
        Debug.Log(Application.persistentDataPath);
        
    }

    public void Load(string path)
    {
        tempColorDataPoints.Clear();
        FEcomplex FECmain = FileEncryptor.Decrypt(System.IO.File.ReadAllText(path));
        for (int i = 0; i < FECmain.Count / 2; i++)
        {
            FEobject FELi = FECmain["I" + i];
            FEenum FEEo = FECmain.Get("O" + i, FEenum.Empty);

            Color clr = FEobject.LoadColor(FELi);
            ColorName clrName = ConvertPlus.ToEnum<ColorName>(FEEo.Representation);
            tempColorDataPoints.Add(new ColorDataPoint(clr, clrName));
            Debug.Log(tempColorDataPoints[i].ToString());
        }
    }

    public void Save(string path)
    {
        FEobject[] FEO = new FEobject[tempColorDataPoints.Count * 2];
        for (int i = 0; i < tempColorDataPoints.Count; i++)
        {
            FEO[i * 2] = FEobject.SaveColor(tempColorDataPoints[i].color, "I" + i);
            FEO[i * 2 + 1] = new FEenum(tempColorDataPoints[i].colorName.ToString(), "O" + i);
        }
        FEcomplex FECmain = new FEcomplex(FEO);
        System.IO.File.WriteAllText(path, FileEncryptor.Encrypt(FECmain));
    }

    public static class ConvertPlus
    {
        /// <summary>
        /// Converts any string value to corresponding value in specified enum type
        /// </summary>
        /// <typeparam name="T">enum type to convert to</typeparam>
        /// <param name="value">string value to convert to</param>
        /// <returns>[T] enum representation of the [string] value</returns>
        public static T ToEnum<T>(string value)
        {
            return (T)System.Enum.Parse(typeof(T), value, true);
        }
    }
}
