using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace App.Confluence;

public class ConfluenceService : IConfluenceService
{
    private readonly HttpClient _httpClient;

    private readonly ConfluenceServiceConfig _serviceConfig;

    public ConfluenceService(HttpClient httpClient, ConfluenceServiceConfig serviceConfig)
    {
        _httpClient = httpClient;
        _serviceConfig = serviceConfig;
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_serviceConfig.Username}:{_serviceConfig.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            auth);
    }
    
    public async Task<ChildrenResponse> GetChildren(string pageId)
    {
        var uri = $"{_serviceConfig.BaseUrl}/wiki/api/v2/pages/{pageId}/children";

        try
        {
            var responseString = await _httpClient.GetAsync(uri);

            var json = await responseString.Content.ReadAsStringAsync();
            var catalog = JsonSerializer.Deserialize<ChildrenResponse>(json);
            return catalog ?? 
                   new ChildrenResponse(new List<Results>().ToArray());
        }
        catch
        {
            Console.WriteLine($"Failed to get children of {pageId}");
            throw;
        }
    }
    
    public async Task<string> CreatePage(string parentPageId, string title, string content = "")
    {
        Console.WriteLine("Create page: " + title);
        if(string.IsNullOrEmpty(content))
            content = TableOfContentsMacro;
        
        var uri = $"{_serviceConfig.BaseUrl}/wiki/rest/api/content";

        var body = new NewPageRequest(
            new[] { new Ancestors(parentPageId) },
            new NewBody(new Storage("storage", content)),
            new Space(_serviceConfig.SpaceKey),
            title,
            "page");

        var response = await _httpClient.PostAsJsonAsync(uri, body);

        var json = await response.Content.ReadAsStringAsync();
        var parsedResponse = JsonSerializer.Deserialize<NewPageResponse>(json);

        if (!response.IsSuccessStatusCode || parsedResponse?.id == null)
        {
            var badRequest = JsonSerializer.Deserialize<BadRequest>(json);
            if (badRequest != null)
            {
                throw new ConfluenceServiceException($"ERROR: Page with title '{title}' could not be created. " +
                                                     string.Join(null, badRequest.message.Split("BadRequestException: ").Skip(1)));
            }
            throw new ConfluenceServiceException(json);
        }
        return parsedResponse.id;
    }

    public async Task UpdatePage(string pageId, string title, string content, string reference)
    {
        var oldBody = await GetPage(pageId);
        if (content == oldBody?.body.storage.value.Replace("&quot;", "\""))
        {
            Console.WriteLine("No update: " + title);
            return;
        }
        
        Console.WriteLine("Update page: " + title);
        var currentVersion = oldBody!.version.number;
        
        var uri = $"{_serviceConfig.BaseUrl}/wiki/api/v2/pages/{pageId}";

        var body = new UpdatePageRequest(
            pageId,
            "current",
            title,
            new Body("storage", content),
            new Version(currentVersion+1, reference)
        );

        var responseString = await _httpClient.PutAsJsonAsync(uri, body);

        var json = await responseString.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<NewPageResponse>(json);
        return;
    }
    
    public async Task<int> GetPageVersion(string pageId)
    {
        var uri = $"{_serviceConfig.BaseUrl}/wiki/api/v2/pages/{pageId}?body-format=storage";
        var responseString = await _httpClient.GetAsync(uri);

        var json = await responseString.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<PageResponse>(json);
        return response!.version.number;
    }

    public async Task<PageResponse?> GetPage(string pageId)
    {
        var uri = $"{_serviceConfig.BaseUrl}/wiki/api/v2/pages/{pageId}?body-format=storage";
        var responseString = await _httpClient.GetAsync(uri);

        var json = await responseString.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<PageResponse>(json);
        return response;
    }

    public async Task DeletePage(string pageId, string name, bool recursive = false)
    {
        
        var uri1 = $"{_serviceConfig.BaseUrl}/wiki/api/v2/pages/{pageId}?body-format=storage";
        var responseString1 = await _httpClient.GetAsync(uri1);
        
        Console.WriteLine("Delete: " + name);
        var uri = $"{_serviceConfig.BaseUrl}/wiki/api/v2/pages/{pageId}";

        var response = await _httpClient.DeleteAsync(uri);
        if (!response.IsSuccessStatusCode)
        {
            // throw new ConfluenceServiceException(
            //     $"ERROR: Page {pageId} with title '{name}' could not be deleted. Does the confluence user have sufficient rights?");
        }
    }

    public string TableOfContentsMacro =>
        """
        <ac:structured-macro ac:name="children" ac:schema-version="2" data-layout="default" ac:local-id="8c8424f8-cd81-4a71-b5c9-58e87d7493fa" ac:macro-id="dfd91e927d85294353d873bf98912e5ddfc60871ec85741ee457556e18e0d1c5">
            <ac:parameter ac:name="all">true</ac:parameter>
            <ac:parameter ac:name="depth">0</ac:parameter>
            <ac:parameter ac:name="allChildren">true</ac:parameter>
            <ac:parameter ac:name="style" /><ac:parameter ac:name="sortAndReverse" />
            <ac:parameter ac:name="first">0</ac:parameter>
        </ac:structured-macro><p />
        """;
}