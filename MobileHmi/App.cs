// Copyright (c) Converter Systems LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Workstation.ServiceModel.Ua;
using Xamarin.Forms;

namespace Workstation.MobileHmi
{
    public class App : Application
    {
        private ILoggerFactory loggerFactory;
        private UaApplication application;

        protected override void OnStart()
        {
            // Setup a logger.
            this.loggerFactory = new LoggerFactory();
            this.loggerFactory.AddDebug(LogLevel.Trace);

            // Build and run an OPC UA application instance.
            this.application = new UaApplicationBuilder()
                .UseApplicationUri($"urn:{Dns.GetHostName()}:Workstation.MobileHmi")
                .UseDirectoryStore(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "pki"))
                //.UseIdentity(this.ShowSignInDialog)
                .UseLoggerFactory(this.loggerFactory)
                .Map("opc.tcp://localhost:26543", "opc.tcp://10.0.2.2:26543")
                .Build();

            this.application.Run();

            // Show the MainPage
            this.MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnSleep()
        {
            this.application?.SuspendAsync().Wait();
        }

        protected override void OnResume()
        {
            this.application?.Run();
        }

        /// <summary>
        /// Shows a Sign In dialog if the remote endpoint demands a UserNameIdentity token.
        /// </summary>
        /// <param name="endpoint">The remote endpoint.</param>
        /// <returns>A UserIdentity</returns>
        private async Task<IUserIdentity> ShowSignInDialog(EndpointDescription endpoint)
        {
            if (endpoint.UserIdentityTokens.Any(p => p.TokenType == UserTokenType.Anonymous))
            {
                return new AnonymousIdentity();
            }

            if (endpoint.UserIdentityTokens.Any(p => p.TokenType == UserTokenType.UserName))
            {
                return new UserNameIdentity("root", "secret");
            }

            return new AnonymousIdentity();
        }
    }
}
