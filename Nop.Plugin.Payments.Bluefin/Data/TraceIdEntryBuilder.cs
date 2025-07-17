using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Data;

public class TraceIdEntryBuilder : NopEntityBuilder<TraceIdEntry>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TraceIdEntry.TraceId)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(TraceIdEntry.ErrorMessage)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(TraceIdEntry.Json)).AsString(int.MaxValue).Nullable();
    }

    #endregion
}