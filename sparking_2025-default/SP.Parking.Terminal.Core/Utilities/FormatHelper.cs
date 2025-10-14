using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class FormatHelper
{
	public static string ToEngineeringNotation(this int num)
	{
		double DIV = 1000;

		double f = num;
		if (f < DIV) return num.ToString();
		f /= DIV;
		if (f < DIV) return f.ToString("0.0k");
		f /= DIV;
		if (f < DIV) return f.ToString("0.0m");
		return (f / DIV).ToString("0.0g");
	}

    public static String BytesToString(long byteCount)
    {
        string[] suf = { " B", " KB", " MB", " GB", " TB", " PB", " EB" }; //Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
} 