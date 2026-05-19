namespace web.Services;

public class FunctionKeyHandler : DelegatingHandler
{
    private readonly string _functionKey;

    public FunctionKeyHandler(string functionKey)
    {
        _functionKey = functionKey;
        InnerHandler = new HttpClientHandler();
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_functionKey))
        {
            request.Headers.Add("x-functions-key", _functionKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
