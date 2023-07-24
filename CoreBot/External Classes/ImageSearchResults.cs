using GScraper;
using GScraper.Google;

namespace CoreBot.External_Classes;

public class ImageSearchResults
{
    private readonly List<IImageResult> images;

    public ImageSearchResults(string query)
    {
        var scraper = new GoogleScraper();
        try
        {
            var imageResults = scraper.GetImagesAsync(query).Result.Take(100);
            images = imageResults.Cast<IImageResult>().ToList();
        }
        catch (Exception e) when (e is HttpRequestException or GScraperException)
        {
            throw new Exception($"Failed to fetch images for query '{query}': {e.Message}");
        }
    }
        
    public IImageResult this[long index]
    {
        get
        {
            if (index < 0 || index >= images.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return images[(int)index];
        }
    }
    public IReadOnlyList<IImageResult> Images => images.AsReadOnly();

    public int Count => images.Count;
}