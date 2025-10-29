using Nop.Data;

using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Services;

public class ReissueOrdersRepositoryService
{
    #region Fields

    private readonly IRepository<ReissueOrderEntry> _reissueRepository;

    #endregion

    #region Ctor

    public ReissueOrdersRepositoryService(IRepository<ReissueOrderEntry> reissueRepository)
    {
        _reissueRepository = reissueRepository;
    }

    #endregion

    #region Methods


    public async Task<ReissueOrderEntry> GetBfTokenByOrderGuid(string OrderGuid)
    {
        var entries = await _reissueRepository.GetAllAsync(query =>
        {
            query = query.Where(entry => entry.OrderGuid == OrderGuid);

            return query;
        }, null);

        return entries.FirstOrDefault();
    }



    public async Task InsertAsync(ReissueOrderEntry to_insert)
    {
        var entry = await GetBfTokenByOrderGuid(to_insert.OrderGuid);

        if (entry != null)
        {
             // await _tokenRepository.UpdateAsync(entry, false);
            return; // NO Nothing since bfTokenReference and OrderGuid is unique per order
        }
        else
        {
            await _reissueRepository.InsertAsync(to_insert, false);
        }

        return;

    }

    #endregion
    }