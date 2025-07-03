using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Data;

public class BluefinTokenEntryBuilder : NopEntityBuilder<BluefinTokenEntry>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(BluefinTokenEntry.CustomerId)).AsString(100).Nullable()
            .WithColumn(nameof(BluefinTokenEntry.BfTokenReference)).AsString(100).Nullable();
    }

    #endregion
}