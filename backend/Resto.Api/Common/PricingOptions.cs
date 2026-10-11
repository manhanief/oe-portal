namespace Resto.Api.Common;

/// <summary>Bound from the "Pricing" section of appsettings.json.</summary>
public class PricingOptions
{
    public const string SectionName = "Pricing";

    /// <summary>0.06 = 6% SST</summary>
    public decimal TaxRate { get; set; }
}
