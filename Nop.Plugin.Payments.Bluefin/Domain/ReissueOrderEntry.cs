using Nop.Core;

namespace Nop.Plugin.Payments.Bluefin.Domain;

public class ReissueOrderEntry : BaseEntity
{
    public string OrderGuid { get; set; }
    public string BfTokenReference { get; set; }
}