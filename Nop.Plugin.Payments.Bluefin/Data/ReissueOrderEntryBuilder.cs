using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Data;

public class ReissueOrderEntryBuilder : NopEntityBuilder<ReissueOrderEntry>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ReissueOrderEntry.OrderGuid)).AsString(100).Nullable()
            .WithColumn(nameof(ReissueOrderEntry.BfTokenReference)).AsString(100).Nullable();
    }

    #endregion
}