using Microsoft.Extensions.Logging;
using AppForSEII.MAUI.Services;

namespace AppForSEII.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton(sp =>
		{
			var handler = new HttpClientHandler();
#if DEBUG
			// Dev certificate is not trusted on the emulator: accept it only in DEBUG and only for local API hosts
			handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
				errors == System.Net.Security.SslPolicyErrors.None
				|| message.RequestUri?.Host is "10.0.2.2" or "localhost";
#endif
			return new HttpClient(handler)
			{
				BaseAddress = new Uri(ApiConfig.BaseAddress),
				Timeout = TimeSpan.FromSeconds(10)
			};
		});
		builder.Services.AddSingleton<TeamApiService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
