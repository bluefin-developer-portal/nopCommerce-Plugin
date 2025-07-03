using Nop.Core;

namespace Nop.Plugin.Payments.Bluefin.Domain;

public class BluefinTokenEntry : BaseEntity
{
    public int CustomerId { get; set; }
    public string BfTokenReference { get; set; } // TODO: Possibly, Refactor to "public List<string> bfTokenReferences;"

    public BluefinTokenEntry() { }
    public BluefinTokenEntry(int customerId, string bluefinTokenReference)
    {
        CustomerId = customerId;
        BfTokenReference = bluefinTokenReference;
    }
}