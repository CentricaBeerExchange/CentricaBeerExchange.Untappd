using CentricaBeerExchange.Untappd;

UntappdSearchScraper searchScraper = new();

try
{
    string query = "Calm In Paradise: Orange, Mango, Apricot And Vanilla";
    //string query = "brewdog";

    Console.WriteLine($"Scraping Search Results using query '{query}'..");
    Console.WriteLine();

    (int total, Beer[] results) = await searchScraper.SearchAsync(query);

    if (results.Length == 0)
    {
        Console.WriteLine($"No results for query '{query}'!");
        return;
    }

    string extraInfo = total > results.Length ? $" (scraped {results.Length})" : string.Empty;

    Console.WriteLine($"Found {total} results{extraInfo}:");
    Console.WriteLine();

    int maxBreweryNameLength = results.Max(r => r.BreweryName.Length);

    foreach (Beer beer in results)
    {
        Console.WriteLine($"- {beer.BreweryName.PadRight(maxBreweryNameLength)} :: {beer.Name}");
        Console.WriteLine($"    ABV:     {beer.ABV:0.0}%");
        Console.WriteLine($"    IBU:     {beer.IBU}");
        Console.WriteLine($"    Rating:  {beer.Rating:0.00}");
        Console.WriteLine();
    }
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine($"ERROR: {ex}");
}
finally
{
    searchScraper?.Dispose();

    Console.WriteLine();
    Console.WriteLine("Done. Press ANY key to exit");
    Console.ReadKey(true);
}
