using System;
using System.IO;

public static  class SafeLoops
{
    // find first char in a loop, returning -1 if not found
    public static int FindChar(string s, char ch, int startIndex = 0)
    {
        s += ch;
        while(s[startIndex] != ch)
        {
            startIndex++;
        }
        if(startIndex == s.Length - 1)
        {
            // not found
            return -1;
        }
        return startIndex;
    }

    public static string DeleteChars(string s, char remove)
    {
        string newString = "";
        foreach(char ch in s)
        {
            if (ch != remove) newString += ch;
        }
        return newString;
    }

    public static string DeleteInBetweenChars(string s, char remove)
    {
        bool inBetween = false;
        string newString = "";
        foreach(char ch in s)
        {
            if (ch == remove) inBetween = !inBetween;
            else if (!inBetween) newString += ch;
        }
        return newString;
    }

    public static int GetIndex<T>(T[] array, T item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Equals(item))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// <para>initializes a new array with an item added at the end, thereby lenghtening the array by 1.
    /// </para><para>if array is <c>null</c>, it will just return the old array</para>
    /// </summary>
    /// <typeparam name="T">any array parameter</typeparam>
    /// <param name="array">old array</param>
    /// <param name="item">item to be added</param>
    /// <returns></returns>
    public static T[] AddItemToArray<T>(T[] array, T item)
    {
        if (array == null) return null;
        if (item == null) return array;

        T[] newArray = new T[array.Length + 1];
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }
        newArray[newArray.Length - 1] = item;
        return newArray;
    }

    /// <summary>
    /// <para>initializes a new array with items added at the end, thereby lenghtening the array by items.Length. </para>
    /// <para>if array is <c>null</c>, it will just return the old array</para>
    /// </summary>
    /// <typeparam name="T">any array parameter</typeparam>
    /// <param name="array">old array</param>
    /// <param name="items">item to be added</param>
    /// <returns></returns>
    public static T[] AddItemToArray<T>(T[] array, T[] items)
    {
        if (array == null) return null;
        if (items == null) return array;

        T[] newArray = new T[array.Length + items.Length];
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i];
        }
        for (int i = 0; i < items.Length; i++)
        {
            newArray[i+array.Length] = items[i];
        }
        return newArray;
    }

    public static T[] DeleteItemInArray<T>(T[] array, int itemIndex)
    {
        if (array == null) return null;
        if (itemIndex < 0 || itemIndex >= array.Length) return array;

        T[] newArray = new T[array.Length - 1];
        int j = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if(i != itemIndex)
            {
                newArray[j] = array[i];
                j += 1;
            }
        }

        return newArray;
    }
}
public static class SafeStrings
{
    public static string FirstCapital(string s)
    {
        if (s == null || s == "") return s;
        if (s.Length == 1) return s.ToUpper();
        return s[0].ToString().ToUpper() + s.Substring(1);
    }
    public static T ToEnum<T>(string value) where T : Enum
    {
        try
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        catch
        {
            return default(T);
        }
    }

    public static string Connect(string[] array, char connector = ' ', bool afterLast = false, int start = 0, int end = -1)
    {
        if (array == null) return "";

        if (end == -1 || end > array.Length || end == 0) end = array.Length;

        string text = "";
        for (int i = start; i < end - 1; i++)
        {
            text += array[i] + connector;
        }
        text += array[end - 1] + (afterLast ? connector : ' ');
        return text;
    }

    public static char GetLast(string s, string notInclude = " ")
    {
        for (int i = s.Length - 1; i >= 0; i--)
        {
            if (!notInclude.Contains(s[i].ToString()))
            {
                return s[i];
            }
        }
        return ' ';
    }

    public static string Insert(int index, string insertion, string text, int insertLength = 0)
    {
        return text.Substring(0, index) + insertion + text.Substring(index + insertLength); 
    }

    public static int GetIndex(string expression, string text, int which = 0)
    {
        int i = 0;
        int j = 0;
        while(i < text.Length)
        {
            if (text[i] == expression[j])
            {
                j++;
            }
            else if (text[i] == expression[0])
            {
                j = 1;
            }
            else
            {
                j = 0;
            }

            if(j == expression.Length)
            {
                if(which == 0)
                {
                    return i - expression.Length;
                }
                else
                {
                    i -= expression.Length + 1;
                    which--;
                    j = 0;
                }
            }
            i++;
        }
        return -1;
    }
}
public static class SafeIntegers
{
    // converting to int, returning 'wrong' if cannot
    public static int Convert(string s, int wrong = int.MaxValue)
    {
        try
        {
            return System.Convert.ToInt32(s.Trim(' '));
        }
        catch
        {
            return wrong;
        }
    }
    public static long Convert(string s, bool isLong, int wrong = int.MaxValue)
    {
        if (!isLong) return Convert(s, wrong);

        try
        {
            return System.Convert.ToInt64(s.Trim(' '));
        }
        catch
        {
            return wrong;
        }
    }

    public static int Clamp(int min, int divider, int max)
    {
        return divider < min ? min : (divider > max ? max : divider);
    }
}
public static class SafeBooleans
{
    // converting to int, returning 'wrong' if cannot
    public static bool Convert(object obj, bool wrong = false)
    {
        try
        {
            return System.Convert.ToBoolean(obj);
        }
        catch
        {
            return wrong;
        }
    }
}

public static class SafeFloats
{
    // converting to int, returning 'wrong' if cannot
    public static float Convert(string s, float wrong = float.MaxValue)
    {
        try
        {
            return System.Convert.ToSingle(s.Trim(' '));
        }
        catch
        {
            return wrong;
        }
    }
}

public static class SafeChars
{
    public static char ToUpper(char ch)
    {
        return ch.ToString().ToUpper()[0];
    }
}


public static class SafeIO
{
    //loading a file text, returning wrong if does not exist
    public static string LoadFileText(string path, string wrong = "")
    {

        try
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path, System.Text.Encoding.UTF8);
            }
            else
            {
                return wrong;
            }
        }
        catch
        {
            return wrong;
        }
    }

    public static string[] LoadFileLines(string path, string[] wrong = null)
    {

        try
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path, System.Text.Encoding.UTF8);
            }
            else
            {
                return wrong;
            }
        }
        catch
        {
            return wrong;
        }
    }

    public static bool SaveFileText(string path, string text)
    {
        try
        {
                File.WriteAllText(path, text, System.Text.Encoding.UTF8);
        }
        catch
        {
            return false;
        }
        return true;
    }

    public static bool AppendFileText(string path, string text)
    {
        try
        {
            File.AppendAllText(path, text, System.Text.Encoding.UTF8);
        }
        catch
        {
            return false;
        }
        return true;
    }
}