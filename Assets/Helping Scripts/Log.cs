using System;
using UnityEngine;


public class Log
{

    static string Message;
    public static void Elements(params object[] elements)
    {

        for (int i = 0; i < elements.Length - 1; i++)
        {
            Message += elements[i].ToString() + " ";
        }
        Message += elements[elements.Length - 1] + "; ";
    }

    public static void Finish()
    {

        Debug.Log(Message);
        Message = "";
    }
}
