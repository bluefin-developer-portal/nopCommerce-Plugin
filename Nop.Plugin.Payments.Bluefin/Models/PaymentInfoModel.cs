﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Bluefin.Models;

/// <summary>
/// Represents a payment info model
/// </summary>
public record PaymentInfoModel : BaseNopModel
{
    public string IFrameCompleted { get; set; }
    
    public bool IframeResponsive { get; set; }
    public string IframeWidth { get; set; }
    public string IframeHeight { get; set; }
}