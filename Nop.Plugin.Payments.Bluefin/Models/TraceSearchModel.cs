using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Bluefin.Models;


public partial record TraceSearchModel : BaseSearchModel
{
    #region Properties
    #nullable enable
    public int? Id { get; set; }
    public string? TraceId { get; set; }

    public string? ErrorMessage { get; set; }

    public string? Json { get; set; }
    
    public DateTime? Created { get; set; }

    #endregion
}