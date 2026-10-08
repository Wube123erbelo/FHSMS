namespace FHSMS.TelegramBot.Configuration;

public class FhsmsSettings
{
    public const string SectionName = "Fhsms";

    public string ApiBaseUrl { get; set; } = "http://localhost:5080/api";
    public string ServiceAccountEmail { get; set; } = default!;
    public string ServiceAccountPassword { get; set; } = default!;
}
