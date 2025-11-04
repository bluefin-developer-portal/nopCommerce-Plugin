using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;

using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Data;

// See https://webiant.com/docs/nopcommerce/Libraries/Nop.Data/Migrations/MigrationProcessType
// NOTE: This migration type applied migration on Installation with the database tables wiped out after uninstalling
[NopMigration("2026-06-30 00:00:00", "Payments.Bluefin base schema", MigrationProcessType.Installation)]
public class SchemaMigration : AutoReversingMigration
{
    #region Methods

    /// <summary>
    /// Collect the UP migration expressions
    /// </summary>
    public override void Up()
    {
        Create.TableFor<BluefinTokenEntry>();
        Create.TableFor<TraceIdEntry>();
        Create.TableFor<ReissueOrderEntry>();
    }

    #endregion
}
