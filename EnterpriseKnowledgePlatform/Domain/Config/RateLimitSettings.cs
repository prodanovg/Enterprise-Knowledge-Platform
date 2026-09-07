namespace Domain.Config;

public class RateLimitSettings
{
    public int PermitLimit { get; set; }
    public int WindowInSeconds { get; set; }

}