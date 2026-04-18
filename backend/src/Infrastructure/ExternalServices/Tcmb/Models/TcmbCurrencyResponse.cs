using System.Xml.Serialization;

namespace PortfolioTracker.Infrastructure.ExternalServices.Tcmb.Models;

[XmlRoot("Tarih_Date")]
public class TcmbCurrencyResponse
{
    [XmlAttribute("Date")]
    public string Date { get; set; } = string.Empty;

    [XmlElement("Currency")]
    public List<TcmbCurrency> Currencies { get; set; } = new();
}

public class TcmbCurrency
{
    [XmlAttribute("CurrencyCode")]
    public string Code { get; set; } = string.Empty;

    [XmlElement("Unit")]
    public int Unit { get; set; }

    [XmlElement("ForexBuying")]
    public string ForexBuyingString { get; set; } = string.Empty;

    [XmlElement("ForexSelling")]
    public string ForexSellingString { get; set; } = string.Empty;

    [XmlElement("CurrencyName")]
    public string Name { get; set; } = string.Empty;

    [XmlIgnore]
    public decimal ForexBuying => ParseDecimal(ForexBuyingString);

    [XmlIgnore]
    public decimal ForexSelling => ParseDecimal(ForexSellingString);

    private static decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        // TCMB uses Turkish decimal separator (comma instead of dot)
        // Replace comma with dot for parsing
        var normalizedValue = value.Replace(',', '.');

        if (decimal.TryParse(normalizedValue, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return 0m;
    }
}
