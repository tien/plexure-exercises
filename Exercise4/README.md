# Exercise 4

I have made adjustments to the provided snippet as below. All original comments have been removed for brevity.

```csharp
public class ItineraryManager
{
    private readonly IDataStore _dataStore;
    private readonly IDistanceCalculator _distanceCalculator;

    // Should use DI to resolve IDataStore & IDistanceCalculator instead of initialize them here
    public ItineraryManager(IDataStore dataStore, IDistanceCalculator distanceCalculator)
    {
        _dataStore = dataStore;
        _distanceCalculator = distanceCalculator;
    }

    // Always return a task when method is doing async operations. Never eat the performance hit when we don't have to
    public async Task<IEnumerable<Quote>> CalculateAirlinePricesAsync(int itineraryId, IEnumerable<IAirlinePriceProvider> priceProviders)
    {
        var itinerary = await _dataStore.GetItinaryAsync(itineraryId);
        if (itinerary == null)
            throw new InvalidOperationException();

        // Use declarative Task.WhenAll instead of Parallel which block the calling thread
        var pricesSets = await Task.WhenAll(priceProviders
                .Select(provider => Task.Run(() => provider.GetQuotes(itinerary.TicketClass, itinerary.Waypoints))));

        return pricesSets
            .SelectMany(x => x);
    }

    public async Task<double> CalculateTotalTravelDistanceAsync(int itineraryId)
    {
        var itinerary = await _dataStore.GetItinaryAsync(itineraryId);
        if (itinerary == null)
            throw new InvalidOperationException();


        // Always prefer declarative over imperitive code
        // Here the code itself describe it intention
        var waypoints = (IEnumerable<int>) itinerary.Waypoints;
        var distancesTask = waypoints
            .SkipLast(1)
            .Zip(waypoints.Skip(1), (orig, dest) => (orig, dest))
            .Select(pair => _distanceCalculator.GetDistanceAsync(pair.orig, pair.dest));

        return await Task.WhenAll(distancesTask);
    }


    // This method signature is misleading in that it doesn't imply any side effect
    // Should be called FindAgentAndUpdatePhoneNumber
    // Was originally named FindAgent
    // Also C# 8 have syntax for explicit nullable reference so we should use that if possible
    public TravelAgent? FindAgentAndUpdatePhoneNumber(int id, string updatedPhoneNumber)
    {
        var agentDao = _dataStore.GetAgent(id);
        if (agentDao == null)
            return null;s

        if (!string.IsNullOrWhiteSpace(updatedPhoneNumber))
        {
            agentDao.PhoneNumber = updatedPhoneNumber;
            _dataStore.UpdateAgent(id, agentDao);
        }

        return Mapper.Map<TravelAgent>(agentDao);
    }
}
```