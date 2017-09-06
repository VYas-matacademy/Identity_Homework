using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer4;
using AspNet.Security.OAuth.Instagram;
using AspNet.Security.OAuth.GitHub;

namespace Server
{
    public class Startup
    {
        public Startup(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                DisplayName = "Google",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com",
                ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo"
            });

            app.UseFacebookAuthentication(new FacebookOptions
            {
                AuthenticationScheme = "Facebook",
                DisplayName = "Facebook",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "1047727442029326",
                ClientSecret = "d02027f82161a6e8e2b849c09674ca89"
            });

            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
            {
                AuthenticationScheme = "Microsoft",
                DisplayName = "Microsoft",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "6faa30e3-6a89-4635-8627-40055b8d2c5c",
                ClientSecret = "qLPKNCayzqEwyetjCn5fj3T"
            });

            app.UseInstagramAuthentication(new InstagramAuthenticationOptions
            {
                AuthenticationScheme = "Instagram",
                DisplayName = "Instagram",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "a3450fd73c1b4ff5ae60616b458d400e",
                ClientSecret = "40a12da77a044e6ca471f585c8f8806e"
            });


            app.UseGitHubAuthentication(new GitHubAuthenticationOptions
            {
                AuthenticationScheme = "Github",
                DisplayName = "Github",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "97bd26016811974105e2",
                ClientSecret = "0a48e80cb2aee58043159985cf0c3db971150eb7"
            });

            app.UseLinkedInAuthentication(new LinkedInOptions
            {
                AuthenticationScheme = "LinkedIn",
                DisplayName = "LinkedIn",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "7892ba7xazkfb1",
                ClientSecret = "1VFgoVVE5njN0oBH"
            });



            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
