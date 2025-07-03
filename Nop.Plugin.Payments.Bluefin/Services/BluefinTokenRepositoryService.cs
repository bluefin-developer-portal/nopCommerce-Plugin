using Nop.Data;

using Nop.Plugin.Payments.Bluefin.Domain;

namespace Nop.Plugin.Payments.Bluefin.Services;

public class BluefinTokenRepositoryService
{
    #region Fields

    private readonly IRepository<BluefinTokenEntry> _tokenRepository;

    #endregion

    #region Ctor

    public BluefinTokenRepositoryService(IRepository<BluefinTokenEntry> tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    #endregion

    #region Methods

    public async Task<IList<BluefinTokenEntry>> GetBluefinTokensByCustomerIdAsync(int CustomerId)
    {
        var entries = await _tokenRepository.GetAllAsync(query =>
        {
            query = query.Where(entry => entry.CustomerId == CustomerId);

            return query;
        }, null);

        return entries;
    }

    public async Task<BluefinTokenEntry> GetBluefinTokenEntry(string bfTokenReference)
    {
        var entries = await _tokenRepository.GetAllAsync(query =>
        {
            query = query.Where(entry => entry.BfTokenReference == bfTokenReference);

            return query;
        }, null);

        return entries.FirstOrDefault();
    }



    public async Task InsertAsync(BluefinTokenEntry to_insert)
    {
        var entry = await GetBluefinTokenEntry(to_insert.BfTokenReference);

        if (entry != null)
        {
             // await _tokenRepository.UpdateAsync(entry, false);
            return; // NO Nothing since bfTokenReference is unique per customer
        }
        else
        {
            await _tokenRepository.InsertAsync(to_insert, false);
        }

        return;

    }

    #endregion
    }