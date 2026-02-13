namespace Agent.Core.WebSearch.SearXNG;

public class SearXNGSearch
{
    private readonly string _searXNGInstanceUrl;

    public SearXNGSearch(string searXNGInstanceUrl)
    {
        _searXNGInstanceUrl = searXNGInstanceUrl;
    }

    public async Task<string> SearchAsync(string query)
    {
        // Example implementation for SearXNG search
        // This would typically involve making an HTTP request to the SearXNG instance
        // and parsing the results.
        var result = await new Url(_searXNGInstanceUrl)
            .AppendPathSegment("search")
            .SetQueryParams(new { q = query, format = "json" })
            .GetJsonAsync<dynamic>();

        // For simplicity, returning a serialized version of the dynamic result
        return result.ToString();
    }
}

