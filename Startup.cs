// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using EchoBot1.Bots;
using EchoBot1.Dialogs;
using EchoSkillBot;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;

namespace EchoBot1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
           services.AddSingleton<ConversationState>();

            // The Dialog that will be run by the bot.
            //services.AddSingleton<UserProfileDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot>();

            // Create QnA Maker endpoint as a singleton
            services.AddSingleton(new QnAMakerEndpoint
            {
                KnowledgeBaseId = Configuration.GetValue<string>("QnAKnowledgebaseId"),
                EndpointKey = Configuration.GetValue<string>("QnAAuthKey"),
                Host = Configuration.GetValue<string>("QnAEndpointHostName")
            });

            // Register AuthConfiguration to enable custom claim validation.
            services.AddSingleton(sp =>
            {
                var allowedCallers = new List<string>(sp.GetService<IConfiguration>().GetSection("AllowedCallers").Get<string[]>());

                var claimsValidator = new AllowedCallersClaimsValidator(allowedCallers);

                // If TenantId is specified in config, add the tenant as a valid JWT token issuer for Bot to Skill conversation.
                // The token issuer for MSI and single tenant scenarios will be the tenant where the bot is registered.
                var validTokenIssuers = new List<string>();
                var tenantId = sp.GetService<IConfiguration>().GetSection(MicrosoftAppCredentials.MicrosoftAppTenantIdKey)?.Value;

                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    // For SingleTenant/MSI auth, the JWT tokens will be issued from the bot's home tenant.
                    // Therefore, these issuers need to be added to the list of valid token issuers for authenticating activity requests.
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidTokenIssuerUrlTemplateV1, tenantId));
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidTokenIssuerUrlTemplateV2, tenantId));
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV1, tenantId));
                    validTokenIssuers.Add(string.Format(CultureInfo.InvariantCulture, AuthenticationConstants.ValidGovernmentTokenIssuerUrlTemplateV2, tenantId));
                }

                return new AuthenticationConfiguration
                {
                    ClaimsValidator = claimsValidator,
                    ValidTokenIssuers = validTokenIssuers
                };
            });

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, SkillAdapterWithErrorHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
