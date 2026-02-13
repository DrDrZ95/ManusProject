namespace Agent.Application.Services.Hdfs;

public class HdfsService : IHdfsService
{
    private readonly HttpClient _httpClient;
    private readonly string _hdfsNamenodeUrl;

    public HdfsService(IConfiguration configuration)
    {
        _hdfsNamenodeUrl = configuration["Hdfs:NamenodeUrl"] ?? throw new ArgumentNullException("Hdfs:NamenodeUrl configuration is missing.");
        _httpClient = new HttpClient();
        // You might need to configure authentication for HDFS here
        // For example, Kerberos authentication would require more complex setup
    }

    private string GetWebHdfsUrl(string path, string op, bool redirect = true)
    {
        var url = new StringBuilder();
        url.Append($"{_hdfsNamenodeUrl}/webhdfs/v1{path}?op={op}");
        if (!redirect)
        {
            url.Append("&noredirect=true");
        }
        return url.ToString();
    }

    public async Task<bool> UploadFileAsync(string remotePath, Stream fileStream, string contentType)
    {
        // Step 1: Get the redirection URL for file creation
        var createUrl = GetWebHdfsUrl(remotePath, "CREATE", false);
        var response = await _httpClient.PutAsync(createUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to get HDFS upload URL: {response.ReasonPhrase}");
        }

        var redirectUrl = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(redirectUrl))
        {
            throw new InvalidOperationException("HDFS did not provide a redirection URL for upload.");
        }

        // Step 2: Upload the file to the redirected URL
        using (var content = new StreamContent(fileStream))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            var uploadResponse = await _httpClient.PutAsync(redirectUrl, content);
            uploadResponse.EnsureSuccessStatusCode();
            return uploadResponse.IsSuccessStatusCode;
        }
    }

    public async Task<Stream> DownloadFileAsync(string remotePath)
    {
        var openUrl = GetWebHdfsUrl(remotePath, "OPEN", false);
        var response = await _httpClient.GetAsync(openUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to get HDFS download URL: {response.ReasonPhrase}");
        }

        var redirectUrl = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(redirectUrl))
        {
            throw new InvalidOperationException("HDFS did not provide a redirection URL for download.");
        }

        var downloadResponse = await _httpClient.GetAsync(redirectUrl);
        downloadResponse.EnsureSuccessStatusCode();
        return await downloadResponse.Content.ReadAsStreamAsync();
    }

    public async Task<string> ReadTextFileAsync(string remotePath)
    {
        using (var stream = await DownloadFileAsync(remotePath))
        using (var reader = new StreamReader(stream))
        {
            return await reader.ReadToEndAsync();
        }
    }

    public async Task<bool> CreateDirectoryAsync(string remotePath)
    {
        var mkdirUrl = GetWebHdfsUrl(remotePath, "MKDIRS");
        var response = await _httpClient.PutAsync(mkdirUrl, null);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> FileExistsAsync(string remotePath)
    {
        var getStatusUrl = GetWebHdfsUrl(remotePath, "GETFILESTATUS");
        var response = await _httpClient.GetAsync(getStatusUrl);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteFileAsync(string remotePath)
    {
        var deleteUrl = GetWebHdfsUrl(remotePath, "DELETE");
        var response = await _httpClient.DeleteAsync(deleteUrl);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }
}
