namespace FSH.BlazorWebAssembly.Client.Infrastructure.Common;

public record LanguageCode(string Code, string DisplayName, bool IsRTL = false);

public static class LocalizationConstants
{
    public static readonly LanguageCode[] SupportedLanguages =
    {
                new("kg-KG", "Kyrgyz"),
                new("ru-RU", "Russian"),
        new("en-US", "English")
        

    };
}