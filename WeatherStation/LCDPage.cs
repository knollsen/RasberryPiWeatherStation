using System;
using System.Collections.Generic;

namespace WeatherStation;

public abstract class LCDPage
{
    public string FirstLine { get; protected set; }
    public string SecondLine { get; protected set; }

    public abstract void Build(DataCache data);

    protected static string FormatStringForLcd(string beginning, string end)
    {
        var missing = 16 - (beginning.Length + end.Length);

        if (missing < 0)
        {
            throw new ArgumentException("Content is too long");
        }
        var fillerList = new List<char>();
        for (var i = 0; i < missing; i++)
        {
            fillerList.Add(' ');
        }

        var filler = string.Concat(fillerList);

        return beginning + filler + end;
    }
}