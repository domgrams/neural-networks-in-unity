
public static class Logger
{

    public static string Text = "";

    public static string Print(string help)
    {
        Text += help + "\r\n";
        return Text;
    }
    public static string Print(float[] help)
    {

        foreach (float value in help)
        {
            Text += value + "; ";
        }
        return Text + "\r\n";
    }
    public static string Print(int[] help)
    {

        foreach (float value in help)
        {
            Text += value + "; ";
        }
        return Text + "\r\n";
    }
    public static string Clear()
    {
        Text = "";
        return Text;
    }
}