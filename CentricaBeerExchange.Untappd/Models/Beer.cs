namespace CentricaBeerExchange.Untappd.Models;

public  class Beer
{
    public Beer(long id, string name, string url, string thumbnailUrl, string style, decimal abv, string ibu, decimal rating, string breweryName, string breweryUrl)
    {
        Id = id;
        Name = name;
        Url = url;
        ThumbnailUrl = thumbnailUrl;
        Style = style;
        ABV = abv;
        IBU = ibu;
        Rating = rating;
        BreweryName = breweryName;
        BreweryUrl = breweryUrl;
    }

    public long Id { get; }
    public string Name { get; }
    public string Url { get; }
    public string ThumbnailUrl { get; }
    public string Style { get; }
    public decimal ABV { get; }
    public string IBU { get; }
    public decimal Rating { get; }
    public string BreweryName { get; }
    public string BreweryUrl { get; }
}
