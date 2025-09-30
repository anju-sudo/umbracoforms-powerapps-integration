using Microsoft.Extensions.Configuration;

namespace FormDemo.ExternalUserLogin.GoogleAuthentication
{
    public static class GoogleAuthenticationExtensions
    {
        public static IUmbracoBuilder AddGoogleAuthentication(this IUmbracoBuilder builder)
        {
            // Register provider options
            builder.Services.ConfigureOptions<GoogleBackOfficeExternalLoginProviderOptions>();

            // Get configuration from DI
            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            builder.AddBackOfficeExternalLogins(logins =>
            {
                logins.AddBackOfficeLogin(backOfficeAuthenticationBuilder =>
                {
                    var schemeName = backOfficeAuthenticationBuilder.SchemeForBackOffice(
                        GoogleBackOfficeExternalLoginProviderOptions.SchemeName);

                    ArgumentNullException.ThrowIfNull(schemeName);

                    backOfficeAuthenticationBuilder.AddGoogle(schemeName, options =>
                    {
                        options.CallbackPath = "/umbraco-google-signin";

                        // Read client id/secret from configuration (safe, not hardcoded)
                        options.ClientId = config["Authentication:Google:ClientId"];
                        options.ClientSecret = config["Authentication:Google:ClientSecret"];
                    });
                });
            });

            return builder;
        }
    }
}
