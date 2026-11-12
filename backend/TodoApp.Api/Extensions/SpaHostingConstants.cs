namespace TodoApp.Api.Extensions;

/// <summary>
/// Routes used to host the Angular build from the API's wwwroot (AD-6, NFR-29).
/// </summary>
public static class SpaHostingConstants
{
    /// <summary>Unmatched paths under /api return 404 instead of the Angular page.</summary>
    public const string ApiFallbackPattern = "api/{**path}";

    /// <summary>Angular entry page served for client-side routes such as /tasks.</summary>
    public const string IndexFile = "index.html";
}
