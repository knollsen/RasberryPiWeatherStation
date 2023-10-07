namespace WeatherStation;

public class BlankPage : LCDPage
{
    public override void Build(DataCache data)
    {
        this.FirstLine = FormatStringForLcd("        ", "        ");
        this.SecondLine = FormatStringForLcd("        ", "        ");
    }
}