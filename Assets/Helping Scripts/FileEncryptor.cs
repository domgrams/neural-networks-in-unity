using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FileEncryption is specific text-based freeware for encrypting and decrypting programming data into files,
/// easily enough to make it understandable and even human-writable in text file and with scrict rules such that
/// the encryption and decryption is a fast process.
/// </summary>
namespace FileEncryption
{
    /// <summary>
    /// Main class for handling files encrypted by FileEncryption - .fe/.tmap/.rfe
    /// FileEncryption is specific text-based freeware for encrypting and decrypting programming data into files,
    /// easily enough to make it understandable and even human-writable in text file and with scrict rules such that
    /// the encryption and decryption is a fast process.
    /// Full tutorial and FE file examples are on domgrams website - www.domgrams.webnode.cz
    /// </summary>
    public static class FileEncryptor
    {

        public static bool Debugging = false;


        /// <summary>
        /// Character for separating items in simple list
        /// </summary>
        public const char ParSeparator = ',';
        /// <summary>
        /// Character for separating single value parameters
        /// </summary>
        public const char EndSeparator = ';';
        /// <summary>
        /// Character for assigning values to single value parameters
        /// </summary>
        public const char AssignOperator = '=';
        /// <summary>
        /// Character that signifies a new complex object or hard list
        /// </summary>
        public const char OpenObject = '{';
        /// <summary>
        /// Character that signifies closing of a complex object or hard list
        /// </summary>
        public const char CloseObject = '}';
        /// <summary>
        /// Character that signifies any type of list
        /// </summary>
        public const char OpenArray = '[';
        //static char CloseArray = ']';
        /// <summary>
        /// Character for string values
        /// </summary>
        public const char StringIndicator = '"';
        /// <summary>
        /// Character for enum values
        /// </summary>
        public const char EnumIndicator = '.';
        /// <summary>
        /// Character for float values
        /// </summary>
        public const char FloatIndicator = 'f';
        /// <summary>
        /// Character for pointer values
        /// </summary>
        public const char PointerIndicator = '@';

        /// <summary>
        /// Method for compressing FE file text - getting rid of spaces, new lines, comments
        /// </summary>
        /// <param name="text">text to be compressed</param>
        /// <returns>compressed text</returns>
        static string Compress(string text)
        {
            bool inString = false;
            bool inComment = false;
            string newText = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (!inString && text[i] == '$')
                {
                    inComment = !inComment;
                }
                if (!inComment)
                {
                    if (inString || (text[i] != 65279 && text[i] != '\r' && text[i] != ' ' && text[i] != '\n' && text[i] != '\t'))
                    {
                        newText += text[i];
                    }
                    else if (text[i] == ' ' && i != 0 && i != text.Length - 1)
                    {
                        if (text[i - 1] != ' ' && text[i + 1] != ' ' && text[i - 1] != AssignOperator && text[i + 1] != AssignOperator)
                            newText += text[i];
                    }
                    if (text[i] == StringIndicator)
                    {
                        inString = !inString;
                    }
                }
            }
            return newText;
        }
        /// <summary>
        /// Decrypts simple list
        /// </summary>
        /// <param name="text">FE text to be decrypted</param>
        /// <returns>FE list</returns>
        static FElist DecryptSimpleList(string text)
        {
            List<string> values = new List<string>();
            bool inString = false;
            string currentItem = "";
            for (int j = 0; j < text.Length; j++)
            {
                if (text[j] == StringIndicator)
                {
                    inString = !inString;
                }
                if (text[j] == ParSeparator && !inString)
                {
                    values.Add(currentItem);
                    currentItem = "";
                }
                else
                    currentItem += text[j];
            }
            if (currentItem != "")
                values.Add(currentItem);

            FEobject[] fEobjects = new FEobject[values.Count];
            int i = 0;
            foreach (string value in values)
            {
                fEobjects[i] = DecryptValue(value);
                i++;
            }
            return new FElist(fEobjects);

        }

        static string RefactorString(string s)
        {
            s = s.Substring(1, s.Length - 2);
            s = s.Replace("\\&dqm", "\"");
            return s;
        }


        /// <summary>
        /// Decrypts single value
        /// </summary>
        /// <param name="text">FE value to be decrypted - string, int, bool, enum,...</param>
        /// <returns>FE object of the corresponding value type</returns>
        static FEobject DecryptValue(string text)
        {

            if (text[0] == StringIndicator)
            {
                return new FEstring(RefactorString(text));
            }
            else if (text[0] == EnumIndicator)
            {
                return new FEenum(text.Substring(1));
            }
            else if (text == "true" || text == "false")
            {
                return new FEbool(text);
            }
            else
            {
                bool isDirectFloat = text[text.Length - 1] == FloatIndicator;
                if (isDirectFloat || text.Contains("."))
                {
                    return new FEfloat(isDirectFloat ? text.Substring(0, text.Length - 1) : text);
                }
                else
                {
                    return new FEint(text);
                }
            }

        }

        /// <summary>
        /// Encrypts FEcomplex to FE format
        /// </summary>
        /// <param name="FEC">FEcomplex to be encrypted</param>
        /// <returns>encrypted text</returns>
        public static string Encrypt(FEcomplex FEC)
        {
            return FEC.WithoutBrackets();
        }

        /// <summary>
        /// Recognizes and decrypts all values of FE text
        /// </summary>
        /// <param name="text">FE text to be decrypted</param>
        /// <returns>decrypted text</returns>
        static FEcomplex LocalDecrypt(string text)
        {

            List<FEobject> list = new List<FEobject>();

            int i = 0;
            while (i < text.Length)
            {
                int nameStartI = i;
                bool breaking = false;
                int state = 0;
                while (!breaking)
                {
                    if (text[i] == AssignOperator)
                    {
                        state = 1;
                        breaking = true;
                    }
                    else if (text[i] == OpenObject)
                    {
                        state = 2;
                        breaking = true;
                    }
                    else if (text[i] == OpenArray)
                    {
                        state = 3;
                        breaking = true;
                    }
                    i++;
                }
                int startI = i;

                //if the specified FEobject is FEvalue
                if (state == 1)
                {
                    bool isString = false;
                    for (; i < text.Length; i++)
                    {
                        if (text[i] == StringIndicator)
                        {
                            isString = !isString;
                        }
                        if (!isString && text[i] == EndSeparator)
                        {
                            FEobject fEobject = DecryptValue(text.Substring(startI, i - startI));
                            fEobject.Name = text.Substring(nameStartI, startI - nameStartI - 1);
                            list.Add(fEobject);
                            i++;
                            break;
                        }
                    }
                }
                //if the specified FEobject is FEcomplex
                else if (state == 2)
                {
                    int brackets = 1;
                    bool isString = false;
                    for (; brackets > 0; i++)
                    {
                        if (text[i] == StringIndicator)
                        {
                            isString = !isString;
                        }
                        if (!isString)
                        {
                            if (text[i] == OpenObject)
                            {
                                brackets++;
                            }
                            else if (text[i] == CloseObject)
                            {
                                brackets--;
                            }
                        }
                    }
                    FEobject fEobject = LocalDecrypt(text.Substring(startI, i - startI - 1));
                    fEobject.Name = text.Substring(nameStartI, startI - nameStartI - 1).Trim(' ');
                    list.Add(fEobject);
                }
                //if the specified FEobject is FElist
                else if (state == 3)
                {
                    // simple list
                    i += 2;
                    if (text[i - 1] == AssignOperator)
                    {
                        bool isString = false;
                        for (; i < text.Length; i++)
                        {
                            if (text[i] == StringIndicator)
                            {
                                isString = !isString;
                            }
                            if (!isString && text[i] == EndSeparator)
                            {
                                FEobject fEobject = DecryptSimpleList(text.Substring(startI + 2, i - startI - 2));
                                fEobject.Name = text.Substring(nameStartI, startI - nameStartI - 1).Trim(' ');
                                list.Add(fEobject);
                                i++;
                                break;
                            }
                        }
                    }
                    // hard list
                    else if (text[i - 1] == OpenObject)
                    {
                        bool isString = false;
                        int brackets = 1;
                        for (; brackets > 0; i++)
                        {
                            if (text[i] == StringIndicator)
                            {
                                isString = !isString;
                            }
                            if (!isString)
                            {
                                if (text[i] == OpenObject)
                                {
                                    brackets++;
                                }
                                else if (text[i] == CloseObject)
                                {
                                    brackets--;
                                }
                            }
                        }
                        FEobject fEobject = DecryptHardList(text.Substring(startI + 2, i - startI - 3));
                        fEobject.Name = text.Substring(nameStartI, startI - nameStartI - 1).Trim(' ');
                        list.Add(fEobject);
                    }
                }
            }
            FEcomplex fEcomplex = new FEcomplex(list.ToArray());
            return fEcomplex;
        }
        /// <summary>
        /// Decrypts hard list
        /// </summary>
        /// <param name="text">FE text to be decrypted</param>
        /// <returns>FE list</returns>
        static FElist DecryptHardList(string text)
        {
            bool isString = false;
            int subObj = 0;
            int starting = 0;
            List<FEobject> objects = new List<FEobject>();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == StringIndicator)
                {
                    isString |= !isString;
                }
                else if (text[i] == OpenObject)
                {
                    if (subObj == 0)
                        starting = i;
                    subObj++;
                }
                else if (text[i] == CloseObject)
                {
                    subObj--;
                    if (subObj == 0)
                    {
                        objects.Add(LocalDecrypt(text.Substring(starting + 1, i - starting - 1)));
                    }
                }
            }
            return new FElist(objects.ToArray());
        }

        /// <summary>
        /// Only public method to decrypt yet uncompressed text in FE file
        /// </summary>
        /// <param name="text">text to be decrypted</param>
        /// <returns>decrypted FEobject (always FEcomplex)</returns>
        public static FEcomplex Decrypt(string text)
        {
            if (text == null || text == "") return new FEcomplex(new FEcomplex[0]);
            return LocalDecrypt(Compress(text));
        }
    }




    /// <summary>
    /// Abstract parent class for fundamental building unit of FileEncryption. As a type, it can store lists, strings, bools ...
    /// </summary>
    public abstract class FEobject
    {
        public string Name = "";

        public FEobject Parent { get; set; }
        public string Path
        {
            get
            {
                string fullPath = Name;
                FEobject currentObject = Parent;
                while (currentObject != null)
                {
                    fullPath = currentObject.Name + "\\" + fullPath;
                    currentObject = currentObject.Parent;
                }
                return fullPath;
            }
        }

        /// <summary>
        /// works same as 'as' keyword
        /// </summary>
        /// <typeparam name="T">class type within FEobject parent class</typeparam>
        /// <returns>this as type T</returns>
        public T As<T>() where T : FEobject
        {
            return this as T;
        }

        /// <summary>
        /// similar to 'as' keyword, returning <paramref name="def"/> if null
        /// </summary>
        /// <typeparam name="T">the FEobject type that you want to change it to</typeparam>
        /// <param name="def">the default value if it would return null</param>
        /// <returns>the same FEobject but as a <typeparamref name="T"/>, being <paramref name="def"/> if it cannot be <typeparamref name="T"/></returns>
        public T As<T>(T def) where T : FEobject
        {
            if (!(this is T)) return def;
            return this as T;
        }

        //public static Font LoadFont(FElist FELfont)
        //{
        //    string fontName = FELfont.Get(0, new FEstring("Segoe UI")).Value;
        //    float fontSize = FELfont.Get(1, new FEfloat("16")).Value;
        //    FontStyle fontStyle = SafeStrings.ToEnum<FontStyle>(FELfont.Get(2, new FEenum("Regular")).Representation);
        //    return new Font(fontName, fontSize, fontStyle);
        //}

        public static FElist SaveColor(Color color, string name = "")
        {
            return new FElist(new FEobject[] { new FEfloat(color.r), new FEfloat(color.g), new FEfloat(color.b), new FEfloat(color.a), }, name);
        }

        /// <summary>
        /// Loads color using dynamic expressions passing FEobject
        /// Full list of color names using enums is in example temporarilly on www.domgrams.webnode.cz
        /// </summary>
        /// <param name="colorObject">FEenum as color name, simple list as rgb(a) or complex as RGB(A), using float (0-1) or int (0-255) values</param>
        /// <returns>UnityEngine.Color obtaining corresponding color</returns>
        /// <exception cref="ArgumentNullException">if color object is either null or another FE type</exception>
        public static Color LoadColor(FEobject colorObject)
        {
            if (colorObject == null) throw new ArgumentNullException();
            FEenum FEEcolor = colorObject.As<FEenum>();
            if (FEEcolor != null)
            {
                return FEEcolor.Representation switch
                {
                    "White" => Color.white,
                    "Black" => Color.black,
                    "Pink" => new Color(1, 0.53f, 0.81f),
                    "Blue" => Color.blue,
                    "Green" => Color.green,
                    "Red" => Color.red,
                    "Yellow" => Color.yellow,
                    "Gray" => Color.gray,
                    "Magenta" => Color.magenta,
                    "Orange" => new Color(1, 0.5f, 0f),
                    "Brown" => new Color(0.51f, 0.19f, 0.075f),
                    "LightSkyBlue" => new Color(0f, 1f, 1f),
                    "SkyBlue" => new Color(0f, 0.58f, 1f),
                    "DarkGreen" => new Color(0, 0.5f, 0),
                    "LightGray" => new Color(0.75f, 0.75f, 0.75f),
                    "DarkGray" => new Color(0.25f, 0.25f, 0.25f),
                    "DarkViolet" => new Color(0.07f, 0.07f, 0.3f),
                    "DarkRed" => new Color(0.55f, 0.1f, 0.1f),
                    "Waste" => new Color(0.5f, 0.5f, 0f),
                    "RedWine" => new Color(0.5f, 0f, 0.5f),
                    "Turquoise" => new Color(0f, 0.5f, 0.5f),
                    "LightGreen" => new Color(0f, 1f,0.5f),
                    "LightYellow" => new Color(1, 1, 0),
                    "LightRed" => new Color(1, 0.4f, 0.4f),
                    _ => Color.white
                };
            }
            FElist FELrgb = colorObject.As<FElist>();
            if (FELrgb != null)
            {
                float r = FELrgb.Get<FEfloat>(0) == null ? FELrgb.Get<FEint>(0).Value / 255f : FELrgb.Get<FEfloat>(0).Value;
                float g = FELrgb.Get<FEfloat>(1) == null ? FELrgb.Get<FEint>(1).Value / 255f : FELrgb.Get<FEfloat>(1).Value;
                float b = FELrgb.Get<FEfloat>(2) == null ? FELrgb.Get<FEint>(2).Value / 255f : FELrgb.Get<FEfloat>(2).Value;
                float a;
                if (FELrgb.Count == 4)
                {
                    a = FELrgb.Get<FEfloat>(3) == null ? FELrgb.Get<FEint>(3).Value / 255f : FELrgb.Get<FEfloat>(3).Value;
                }
                else
                {
                    a = 1;
                }
                return new Color(r, g, b, a);
            }
            FEcomplex FECrgb = colorObject.As<FEcomplex>();
            if (FECrgb != null)
            {
                float r = FECrgb.Get<FEfloat>("R") == null ? FECrgb.Get<FEint>("R").Value / 255f : FECrgb.Get<FEfloat>("R").Value;
                float g = FECrgb.Get<FEfloat>("G") == null ? FECrgb.Get<FEint>("G").Value / 255f : FECrgb.Get<FEfloat>("G").Value;
                float b = FECrgb.Get<FEfloat>("B") == null ? FECrgb.Get<FEint>("B").Value / 255f : FECrgb.Get<FEfloat>("B").Value;
                float a = FECrgb.Get<FEfloat>("A") == null ? (FECrgb.Get<FEint>("A") == null ? 255 : FECrgb.Get<FEint>("A").Value) / 255f : FECrgb.Get<FEfloat>("A").Value;

                return new Color(r, g, b, a);
            }
            throw new ArgumentNullException();
        }
        
        public virtual string ValueToString()
        {
            return "";
        }

        /// <summary>
        /// copies the entire FEobject and returns a reference to the new FEobject
        /// </summary>
        /// <returns>the reference to the newly initialized FEobject</returns>
        public abstract FEobject Copy();

        /// <summary>
        /// returns FE code for the FEobject
        /// </summary>
        /// <returns>FE code of the FEobject</returns>
        public virtual string ToFileText()
        {
            return Name + FileEncryptor.AssignOperator + ValueToString() + FileEncryptor.EndSeparator;
        }
    }
    /// <summary>
    /// FEobject that can store other FE objects, adressing them by their name
    /// </summary>
    public class FEcomplex : FEobject
    {
        private FEobject[] objects;
        public static FEcomplex Empty { get { return empty; } }
        static FEcomplex empty = new FEcomplex(new FEobject[0], "");
        public FEcomplex(FEobject[] fEobjects, string name = "")
        {
            foreach (FEobject obj in fEobjects)
            {
                if (obj != null)
                    obj.Parent = this;
            }
            if (name != "") Name = name;
            objects = fEobjects;
        }

        public FEobject this[string name]
        {
            get
            {
                foreach (FEobject item in objects)
                {
                    if (item.Name == name)
                    {
                        return item;
                    }
                }
                return null;
            }
        }
        public T Get<T>(string name, T def = null) where T : FEobject
        {
            foreach (FEobject item in objects)
            {
                if (item.Name == name)
                {
                    if (item is T)
                        return item as T;
                }
            }
            return def;
        }

        public void Delete(int i)
        {
            objects = SafeLoops.DeleteItemInArray(objects, i);
        }
        public void Delete(string name)
        {
            int i = 0;
            foreach (FEobject item in objects)
            {
                if (item.Name == name)
                {
                    Delete(i);
                }
                i++;
            }
        }
        public FEobject this[int i]
        {
            get
            {
                return objects[i];
            }
            set
            {
                objects[i] = value;
            }
        }
        public T Get<T>(int i) where T : FEobject
        {
            return objects[i] as T;
        }

        public void Set(FEobject[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                this.objects[i] = objects[i];
                this.objects[i].Parent = this;
            }
        }
        public void Set(FEcomplex complex)
        {
            objects = complex.objects;
        }

        public int Count
        {
            get { return objects.Length; }

        }

        /// <summary>
        /// switches two FEobjects in the FEcomplex between each other
        /// </summary>
        /// <param name="i">the index of the first FEobject</param>
        /// <param name="j">the index of the second FEobject</param>
        /// <returns><c>true</c> if the operation was succesful, otherwise <c>false</c></returns>
        public bool Switch(int i, int j)
        {
            if (i < 0 || j < 0) return false;
            if (i >= objects.Length || j >= objects.Length) return false;
            FEobject temp = objects[i];
            objects[i] = objects[j];
            objects[j] = temp;
            return true;
        }

        /// <summary>
        /// switches two FEobjects in the FEcomplex between each other
        /// </summary>
        /// <param name="a">the name of the first FEobject</param>
        /// <param name="b">the name of the second FEobject</param>
        /// <returns><c>true</c> if the operation was succesful, otherwise <c>false</c></returns>
        public bool Switch(string a, string b)
        {
            int i = -1;
            int j = -1;
            for (int x = 0; x < objects.Length; x++)
            {
                if (objects[x] != null)
                {
                    if (objects[x].Name == a)
                    {
                        i = x;
                    }
                    if (objects[x].Name == b)
                    {
                        j = x;
                    }
                }
            }

            return Switch(i, j);
        }

        public string WithoutBrackets()
        {
            string fileText = "";
            for (int i = 0; i < objects.Length; i++)
            {
                FEobject FEO = objects[i];
                if (FEO != null)
                    fileText += FEO.ToFileText();
            }
            return fileText;
        }
        public override string ValueToString()
        {
            string fileText = "";
            fileText += FileEncryptor.OpenObject;
            fileText += WithoutBrackets();
            fileText += FileEncryptor.CloseObject;
            return fileText;
        }

        public override string ToFileText()
        {
            return Name + ValueToString();
        }

        public void Add(FEobject fEobject)
        {
            objects = SafeLoops.AddItemToArray(objects, fEobject);
            fEobject.Parent = this;
        }

        public override FEobject Copy()
        {
            FEobject[] copyObjects =new FEobject[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    copyObjects[i] = objects[i].Copy();
                }
            }

            return new FEcomplex(copyObjects, Name);
        }
    }
    /// <summary>
    /// FEobject that can store a single integer
    /// </summary>
    public class FEint : FEobject
    {
        public int Value { get; private set; }
        public static FEint Empty { get { return empty; } }
        public static FEint Five { get { return five; } }
        public static FEint One { get { return one; } }

        static FEint empty = new FEint("0");
        static FEint one = new FEint("1");
        static FEint five = new FEint("5");

        public FEint(string val, string name = "")
        {
            if (name != "") Name = name;
            Value = Convert.ToInt32(val);
        }

        public void Set(int text)
        {
            Value = text;
        }
        public override string ValueToString()
        {
            return Value.ToString();
        }

        public override FEobject Copy()
        {
            return new FEint(Value.ToString(), Name);
        }
    }
    /// <summary>
    /// FEobject that can store a single text string
    /// </summary>
    public class FEstring : FEobject
    {
        public string Value { get; private set; }
        public static FEstring Empty { get { return empty; } }

        static FEstring empty = new FEstring("");

        public FEstring(string val, string name = "")
        {
            if (name != "") Name = name;
            Value = val;
        }

        public void Set(string text)
        {
            Value = text;
        }
        public override string ValueToString()
        {
            if (Value == null) return FileEncryptor.StringIndicator + "" + FileEncryptor.StringIndicator;
            return FileEncryptor.StringIndicator + Value.Replace("\"", "\\&dqm") + FileEncryptor.StringIndicator;
        }

        public override FEobject Copy()
        {
            return new FEstring(Value, Name);
        }
    }
    /// <summary>
    /// FEobject that can store a single floating point number
    /// </summary>
    public class FEfloat : FEobject
    {
        public static System.Globalization.CultureInfo ConversionPoint = new System.Globalization.CultureInfo("en-US");
        public float Value { get; private set; }


        public FEfloat(string val, string name = "")
        {
            if (name != "") Name = name;
            Value = Convert.ToSingle(val, ConversionPoint);
        }
        public FEfloat(float val, string name = "")
        {
            if (name != "") Name = name;
            Value = val;
        }

        public void Set(float value)
        {
            Value = value;
        }
        public override string ValueToString()
        {
            string valueInString = Value.ToString(ConversionPoint);
            if (valueInString.Contains("."))
            {
                return valueInString;
            }
            else
            {
                return valueInString + FileEncryptor.FloatIndicator;
            }
        }

        public override FEobject Copy()
        {
            return new FEfloat(Value.ToString(), Name);
        }
    }
    /// <summary>
    /// FEobject that can store a single character
    /// </summary>
    public class FEchar : FEobject
    {
        public char Value { get; private set; }

        public FEchar(char val)
        {
            Value = val;
        }
        public void Set(char value)
        {
            Value = value;
        }
        public override string ValueToString()
        {
            return Value.ToString();
        }
        public override FEobject Copy()
        {
            FEchar FEH = new FEchar(Value);
            FEH.Name = Name;
            return FEH;
        }
    }
    /// <summary>
    /// FEobject that can store a single enum representation as a string value
    /// </summary>
    public class FEenum : FEobject
    {
        public string Representation { get; private set; }
        public static FEenum Empty { get { return empty; } }

        static FEenum empty = new FEenum("None");

        public FEenum(string val, string name = "")
        {
            if (name != "") Name = name;
            Representation = val;
        }
        public void Set<T>(T repr)
        {
            Representation = Convert.ToString(repr);
        }
        public override string ValueToString()
        {
            return FileEncryptor.EnumIndicator + Representation;
        }
        public override FEobject Copy()
        {
            return  new FEenum(Representation, Name);
        }
    }
    /// <summary>
    /// FEobject that can store a single boolean expression
    /// </summary>
    public class FEbool : FEobject
    {
        public bool Value { get; private set; }
        public static FEbool True { get { return _true; } }
        public static FEbool False { get { return _false; } }



        private static FEbool _true = new FEbool(true);
        private static FEbool _false = new FEbool(false);

        public FEbool(string val, string name = "")
        {
            if (name != "")
            {
                Name = name;
            }
            Value = val == "true";
        }
        public FEbool(bool condition, string name = "")
        {
            Value = condition;
            Name = name;
        }

        public void Set(bool value)
        {
            Value = value;
        }
        public override string ValueToString()
        {
            return Value ? "true" : "false";
        }

        public override FEobject Copy()
        {
            return new FEbool(Value, Name);
        }
    }

    public class FEpointer : FEobject
    {
        public FEobject Target { get; private set; }
        public FEpointer(FEobject target, string name = "")
        {
            Target = target;
            Name = name;
        }

        public T Get<T>(T def = null) where T : FEobject
        {
            if (Target == null) return def;
            return Target.As<T>();
        }

        public void Set(FEobject target)
        {
            Target = target;
        }
        public override string ValueToString()
        {
            return FileEncryptor.PointerIndicator + Target.Path;
        }

        public override FEobject Copy()
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// FEobject that can store a list of either FEcomplexs (a.k.a. hard list) or single value FE objects (a.k.a. simple list)
    /// </summary>
    public class FElist : FEobject, IEnumerable
    {
        private FEobject[] objects;

        public int Count
        {
            get
            {
                return objects.Length;
            }
        }

        public static FElist Empty { get { return empty; } }
        static FElist empty = new FElist(new FEobject[0]);

        public FEobject this[int i]
        {
            get
            {

                return objects[i];
            }
        }


        public FElist(FEobject[] fEobjects, string name = "")
        {
            foreach (FEobject obj in fEobjects)
            {
                if (obj != null)
                    obj.Parent = this;
            }
            objects = fEobjects;
            Name = name;
        }

        public T Get<T>(int i, T def = null) where T : FEobject
        {
            if (i >= objects.Length) return def;
            if (!(objects[i] is T)) return def;
            return objects[i] as T;
        }

        /// <summary>
        /// not used, use Set(int index) or SetArray(string[] array) instead
        /// </summary>
        /// <param name="text">text splitted by '|'</param>
        public void Set(FEobject[] values)
        {
            if (objects.Length != values.Length)
            {
                objects = values;
                for (int i = 0; i < objects.Length; i++)
                {
                    if (values[i] != null)
                    {
                        objects[i].Parent = this;
                    }
                }
                return;
            }
            for (int i = 0; i < objects.Length; i++)
            {
                if (values[i] != null)
                {
                    objects[i].Parent = this;
                    objects[i] = values[i];
                }
                else
                {
                    objects[i] = null;
                }
            }
        }

        public bool IsHard()
        {
            if (objects.Length == 0) return true;
            if (objects[0] is FEcomplex) return true;
            return false;
        }
        public bool IsSimple()
        {
            if (objects.Length == 0) return true;
            if (objects[0] is FEcomplex) return false;
            return true;
        }

        public void Delete(int i)
        {
            objects = SafeLoops.DeleteItemInArray(objects, i);

        }
        public bool Switch(int i, int j)
        {
            if (i < 0 || j < 0) return false;
            if (i >= objects.Length || j >= objects.Length) return false;
            FEobject temp = objects[i];
            objects[i] = objects[j];
            objects[j] = temp;
            return true;
        }

        public override string ToFileText()
        {
            string fileText = Name + FileEncryptor.OpenArray + "]" + FileEncryptor.AssignOperator;
            if (objects.Length == 0)
            {
                return fileText + FileEncryptor.OpenObject + FileEncryptor.CloseObject;
            }
            if (objects[0].As<FEcomplex>() != null)
            {
                fileText += FileEncryptor.OpenObject;
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] != null)
                        fileText += objects[i].ValueToString();
                }
                return fileText + FileEncryptor.CloseObject;
            }
            else
            {
                for (int i = 0; i < objects.Length - 1; i++)
                {
                    if (objects[i] != null)
                        fileText += objects[i].ValueToString() + FileEncryptor.ParSeparator;
                }
                fileText += objects[objects.Length - 1].ValueToString() + FileEncryptor.EndSeparator;
                return fileText;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new FElistEnum(objects);
        }

        /// <summary>
        /// add
        /// method that shouldn't be used if more than once in parallel
        /// </summary>
        /// <param name="fEobject"></param>
        public void Add(FEobject fEobject)
        {
            objects = SafeLoops.AddItemToArray(objects, fEobject);
            fEobject.Parent = this;
        }

        public void Add(FEobject[] fEobjects)
        {
            for (int i = 0; i < fEobjects.Length; i++)
            {
                if (fEobjects[i] != null)
                    fEobjects[i].Parent = this;
            }
            objects = SafeLoops.AddItemToArray(objects, fEobjects);
        }

        public override FEobject Copy()
        {
            FEobject[] copyObjects = new FEobject[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    copyObjects[i] = objects[i].Copy();
                }
            }

            return new FElist(copyObjects, Name);
        }
    }

    /// <summary>
    /// added the functionality for foreach with FElists
    /// </summary>
    public class FElistEnum : IEnumerator
    {
        public FEobject Current
        {
            get
            {

                try
                {
                    return objects[position];
                }
                catch
                {
                    throw new Exception();
                }
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            position++;
            return (position < objects.Length);
        }

        public void Reset()
        {
            position = -1;
        }
        int position = -1;
        FEobject[] objects;
        public FElistEnum(FEobject[] fEobjects)
        {
            objects = fEobjects;
        }
    }
}