using Nop.Core;

namespace Nop.Plugin.Payments.Bluefin.Domain;

public class TraceIdEntry : BaseEntity
{
    public string TraceId { get; set; }

    public string ErrorMessage { get; set; }

    public string Json { get; set; }

    public DateTime Created { get; set; }
}