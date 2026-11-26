namespace AppForSEII.MAUI.Services;

public static class ApiConfig
{
    // The Android emulator reaches the host PC through 10.0.2.2, not localhost
    // AppForSEII.API launchSettings.json: https://localhost:7206
#if ANDROID
    public const string BaseAddress = "https://10.0.2.2:7206/";
#else
    public const string BaseAddress = "https://localhost:7206/";
#endif

    // The MAUI app has no login yet, so the captain is a fixed seed user (SeedData.cs)
    public const string CaptainUserId = "1";
    public const string CaptainUserName = "petra@alu.uclm.es";
    public const string CaptainDisplayName = "Petra Peric";
}