using Nop.Data;

using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Services;

public class TraceLogsRepositoryService
{
    #region Fields

    private readonly IRepository<TraceIdEntry> _logsRepository;

    #endregion

    #region Ctor

    public TraceLogsRepositoryService(IRepository<TraceIdEntry> logsRepository)
    {
        _logsRepository = logsRepository;
    }

    #endregion

    #region Methods

    public async Task<IList<TraceIdEntry>> GetAllLogs()
    {
        var entries = await _logsRepository.GetAllAsync(query => query, null);

        return entries;
    }

    public async Task<TraceIdEntry> GetById(int Id)
    {
        var entry = await _logsRepository.GetByIdAsync(Id);

        return entry;
    }

    public async Task InsertAsync(TraceIdEntry to_insert)
    {
        // NOTE: No need to be idempotent for now
        await _logsRepository.InsertAsync(to_insert, false);

        return;

    }

    #endregion
    }