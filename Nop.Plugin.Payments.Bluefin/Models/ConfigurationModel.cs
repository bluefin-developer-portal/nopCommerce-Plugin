﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Payments.Bluefin.Models;

public record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
        ThreeDSecureTransactionTypes = new List<SelectListItem>
        {
            new SelectListItem {
                Text = "GOODS_SERVICE_PURCHASE",
                Value = "GOODS_SERVICE_PURCHASE"
            },
            new SelectListItem {
                Text = "CHECK_ACCEPTANCE",
                Value = "CHECK_ACCEPTANCE"
            },
            new SelectListItem {
                Text = "ACCOUNT_FUNDING",
                Value = "ACCOUNT_FUNDING"
            },
            new SelectListItem {
                Text = "QUSAI_CASH_TRANSACTION",
                Value = "QUSAI_CASH_TRANSACTION"
            },
            new SelectListItem {
                Text = "PREPAID_ACTIVATION",
                Value = "PREPAID_ACTIVATION"
            }
        };

        DeliveryTimeFrames = new List<SelectListItem>
        {
            new SelectListItem {
                Text = "ELECTRONIC_DELIVERY",
                Value = "ELECTRONIC_DELIVERY"
            },
            new SelectListItem {
                Text = "SAME_DAY_SHIPPING",
                Value = "SAME_DAY_SHIPPING"
            },
            new SelectListItem {
                Text = "OVERNIGHT_SHIPPING",
                Value = "OVERNIGHT_SHIPPING"
            },
            new SelectListItem {
                Text = "TWO_DAYS_OR_MOSRE_SHIPPING",
                Value = "TWO_DAYS_OR_MOSRE_SHIPPING"
            }
        };

        ThreeDSecureChallengeIndicators = new List<SelectListItem>
        {
            new SelectListItem {
                Text = "NO_PREFERENCE",
                Value = "NO_PREFERENCE"
            },
            new SelectListItem {
                Text = "PREFER_NO_CHALLENGE",
                Value = "PREFER_NO_CHALLENGE"
            },
            new SelectListItem {
                Text = "PREFER_A_CHALLENGE",
                Value = "PREFER_A_CHALLENGE"
            },
            new SelectListItem {
                Text = "OVERWRITE_NO_CHALLENGE",
                Value = "OVERWRITE_NO_CHALLENGE"
            },
            new SelectListItem {
                Text = "REQUIRES_MANDATE_CHALLENGE",
                Value = "REQUIRES_MANDATE_CHALLENGE"
            }
        };

        ReorderIndicators = new List<SelectListItem>
        {
            new SelectListItem {
                Text = "FIRST_TIME_ORDERED",
                Value = "FIRST_TIME_ORDERED"
            },
            new SelectListItem {
                Text = "REORDER",
                Value = "REORDER"
            }
        };

        ShippingIndicators = new List<SelectListItem>
        {
            new SelectListItem {
                Text = "BILLING_ADDRESS",
                Value = "BILLING_ADDRESS"
            },
            new SelectListItem {
                Text = "MERCHANT_VERIFIED_ADDRESS",
                Value = "MERCHANT_VERIFIED_ADDRESS"
            },
            new SelectListItem {
                Text = "NOT_BILLING_ADDRESS",
                Value = "NOT_BILLING_ADDRESS"
            },
            new SelectListItem {
                Text = "SHIP_TO_STORE",
                Value = "SHIP_TO_STORE"
            },
            new SelectListItem {
                Text = "DIGITAL_GOODS",
                Value = "DIGITAL_GOODS"
            },
            new SelectListItem {
                Text = "TRAVEL_AND_EVENT_TICKETS",
                Value = "TRAVEL_AND_EVENT_TICKETS"
            },
            new SelectListItem {
                Text = "PICK_UP_AND_GO_DELIVERY",
                Value = "PICK_UP_AND_GO_DELIVERY"
            },
            new SelectListItem {
                Text = "LOCKER_DELIVERY",
                Value = "LOCKER_DELIVERY"
            },
            new SelectListItem {
                Text = "OTHER",
                Value = "OTHER"
            }
        };

    }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.UseSandbox")]
    public bool UseSandbox { get; set; }
    public bool UseSandbox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ApiKeyId")]
    public string ApiKeyId { get; set; }
    public string ApiKeyId_OverrideForStore { get; set; } // TODO: Override other settings per store

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ApiKeySecret")]
    [DataType(DataType.Password)]
    public string ApiKeySecret { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.AccountId")]
    public string AccountId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.IFrameConfigId")]
    public string IFrameConfigId { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.Use3DS")]
    public bool Use3DS { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.UseAuthorizeOnly")]
    public bool UseAuthorizeOnly { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.EnableLogging")]
    public bool EnableLogging { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ThreeDTransType")]
    public string ThreeDTransType { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.DeliveryTimeFrame")]
    public string DeliveryTimeFrame { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ThreeDSecureChallengeIndicator")]
    public string ThreeDSecureChallengeIndicator { get; set; }
    
    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ReorderIndicator")]
    public string ReorderIndicator { get; set; }

    [NopResourceDisplayName("Plugins.Payments.Bluefin.Fields.ShippingIndicator")]
    public string ShippingIndicator { get; set; }


    public IList<SelectListItem> ThreeDSecureTransactionTypes { get; set; }
    public IList<SelectListItem> DeliveryTimeFrames { get; set; }
    public IList<SelectListItem> ThreeDSecureChallengeIndicators { get; set; }
    public IList<SelectListItem> ReorderIndicators { get; set; }

    public IList<SelectListItem> ShippingIndicators { get; set; }


}