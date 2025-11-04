using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;

namespace Nop.Plugin.Payments.Bluefin.Models;

/// <summary>
/// Represents a payment info model
/// </summary>

// See: https://webiant.com/docs/nopcommerce/Libraries/Nop.Core/Domain/Orders/Order for the complete Order Entry Schema
public record ReissueOrderModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalPaymentId")] 
    public int Id { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalOrderStatus")] 
    public OrderStatus OrderStatus { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalOrderGuid")]
    public Guid OrderGuid { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalPaymentStatus")]
    public PaymentStatus PaymentStatus { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReissueOrder.OriginalOrderTotal")]
    public decimal OrderTotal { get; set; }


    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReissueOrder.ReissueTotal")] 
    public decimal ReissueTotal { get; set; }
}