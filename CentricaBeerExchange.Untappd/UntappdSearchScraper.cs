using Microsoft.Playwright;
using System.Globalization;
using System.Web;

namespace CentricaBeerExchange.Untappd;

public class UntappdSearchScraper : IDisposable
{
    private const string BASE_URL = "https://untappd.com";
    private const string SEARCH_URL = $"{BASE_URL}/search?q=";

    private static readonly NumberFormatInfo _numberFormat = new() { NumberDecimalSeparator = "." };

    private bool _isInitialized;
    private bool _isDisposed;

    private IPlaywright _playwright;
    private IBrowser _browser;

    public async Task<(int totalMatches, Beer[] results)> SearchAsync(string query)
    {
        await InitializeAsync();

        IBrowserContext context = await _browser.NewContextAsync();
        IPage page = await context.NewPageAsync();

        string searchUrl = $"{SEARCH_URL}{HttpUtility.UrlEncode(query)}";

        await page.GotoAsync(searchUrl);

        await TryDismisConsentAsync(page);

        int totalMatches = await GetTotalMatchesAsync(page);
        Beer[] results = await GetBeerItemsAsync(page);

        return (totalMatches, results);
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();

        _isInitialized = true;
    }

    private static async Task TryDismisConsentAsync(IPage page)
    {
        ILocator element = page.Locator("css=button.fc-cta-consent");

        if (element is not null)
            await element.ClickAsync();
    }

    private static async Task<int> GetTotalMatchesAsync(IPage page)
    {
        ILocator locatorTotal = page.Locator("div.results-list-top > p.total");
        string text = await locatorTotal.InnerTextAsync();
        string strTotal = text[..text.IndexOf(' ')];

        int total = int.Parse(strTotal);

        return total;
    }

    private static async Task<Beer[]> GetBeerItemsAsync(IPage page)
    {
        await Task.CompletedTask;

        ILocator locator = page.Locator("css=div.beer-item");

        IReadOnlyList<ILocator> items = await locator.AllAsync();

        List<Beer> foundBeers = new();

        foreach (ILocator item in items)
        {
            Beer beer = await GetBeerItemAsync(item);

            if (beer is not null)
                foundBeers.Add(beer);
        }

        return foundBeers.ToArray();
    }

    private static async Task<Beer> GetBeerItemAsync(ILocator item)
    {
        long id = await GetIdAsync(item);

        (string name, string url) = await GetNameAndUrlAsync(item);

        string thumbUrl = await GetThumbUrl(item);

        string style = await GetStyleAsync(item);

        (string breweryName, string breweryUrl) = await GetBreweryNameAndUrlAsync(item);

        decimal abv = await GetAbvAsync(item);
        string ibu = await GetIbuAsync(item);

        decimal rating = await GetRatingAsync(item);

        return new Beer(id, name, url, thumbUrl, style, abv, ibu, rating, breweryName, breweryUrl);
    }

    private static async Task<long> GetIdAsync(ILocator item)
    {
        ILocator locatorLabel = item.Locator("a.label");
        string urlId = await locatorLabel.GetAttributeAsync("href");

        int lastIndexOf = urlId.LastIndexOf('/');
        int startIndex = lastIndexOf + 1;

        string strId = urlId[startIndex..];

        return long.Parse(strId);
    }

    private static async Task<(string name, string url)> GetNameAndUrlAsync(ILocator item)
    {
        ILocator locatorName = item.Locator("div.beer-details > p.name > a");
        string name = await locatorName.InnerTextAsync();
        string urlPart = await locatorName.GetAttributeAsync("href");

        return (name, $"{BASE_URL}{urlPart}");
    }

    private static async Task<string> GetThumbUrl(ILocator item)
    {
        ILocator locatorThumb = item.Locator("a.label > img");
        string thumbUrl = await locatorThumb.GetAttributeAsync("src");

        return thumbUrl;
    }

    private static async Task<string> GetStyleAsync(ILocator item)
    {
        ILocator locatorStyle = item.Locator("div.beer-details > p.style");
        string style = await locatorStyle.InnerTextAsync();

        return style;
    }

    private static async Task<(string breweryName, string breweryUrl)> GetBreweryNameAndUrlAsync(ILocator item)
    {
        ILocator locatorBrewery = item.Locator("div.beer-details > p.brewery > a");
        string breweryName = await locatorBrewery.InnerTextAsync();
        string breweryUrlPart = await locatorBrewery.GetAttributeAsync("href");

        return (breweryName, $"{BASE_URL}{breweryUrlPart}");
    }

    private static async Task<decimal> GetAbvAsync(ILocator item)
    {
        ILocator locatorAbv = item.Locator("div.details.beer > p.abv");
        string text = await locatorAbv.InnerTextAsync();

        string trimmedText = text.Replace("%", "").Replace("ABV", "").Trim();

        decimal abv = decimal.Parse(trimmedText, _numberFormat);

        return abv;
    }

    private static async Task<string> GetIbuAsync(ILocator item)
    {
        ILocator locatorIbu = item.Locator("div.details.beer > p.ibu");
        string text = await locatorIbu.InnerTextAsync();

        string trimmedText = text.Replace("IBU", "").Trim();

        return trimmedText;
    }

    private static async Task<decimal> GetRatingAsync(ILocator item)
    {
        ILocator locatorRating = item.Locator("div.details.beer > div.rating > span.num");
        string text = await locatorRating.InnerTextAsync();

        string trimmedText = text.Replace("(", "").Replace(")", "").Trim();

        decimal rating = decimal.Parse(trimmedText, _numberFormat);

        return rating;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
            DisposeAsync().GetAwaiter().GetResult();

        _isDisposed = true;
    }

    private async Task DisposeAsync()
    {
        if (_browser is not null)
            await _browser.DisposeAsync();

        _playwright?.Dispose();

        _browser = null;
        _playwright = null;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
