
namespace Haus.HassClient;

public interface ITemplateClient
{
    Task<string> RenderAsync(string template, object? variables = null, CancellationToken cancellationToken = default);
}

internal sealed class TemplateClient(IHassApiClient api) : ITemplateClient
{
    public Task<string> RenderAsync(string template, object? variables = null, CancellationToken cancellationToken = default) =>
        api.RenderTemplateAsync(template, variables, cancellationToken);
}
