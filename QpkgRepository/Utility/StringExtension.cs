namespace ArxOne.Qnap.Utility;

public static class StringExtension
{
    public static string ToCData(this string value)
    {
        return "<![CData[" + value + "]]>";
    }
}