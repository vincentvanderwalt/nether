﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace IdentityServerTestClient
{
    /// <summary>
    /// A command to test/demonstrate the custom guest authentication flow (using IdentityModel.Client)
    /// </summary>
    class GuestAuthCommand : CommandBase
    {
        private CommandOption _clientIdOption;
        private CommandOption _clientSecretOption;
        private CommandOption _guestIdentifierOption;

        public GuestAuthCommand(IdentityClientApplication application)
            : base(application)
        {

        }

        public override void Register(CommandLineApplication config)
        {
            base.Register(config);

            _clientIdOption = config.Option("--client-id", "clientid", CommandOptionType.SingleValue);
            _clientSecretOption = config.Option("--client-secret", "clientsecret", CommandOptionType.SingleValue);
            _guestIdentifierOption = config.Option("--guest-id", "Guest identifier", CommandOptionType.SingleValue);

            config.StandardHelpOption();
        }


        protected override async Task<int> ExecuteAsync()
        {
            var clientId = _clientIdOption.GetValue("client-id", requireNotNull: true, promptIfNull: true);
            var clientSecret = _clientSecretOption.GetValue("client-secret", requireNotNull: true, promptIfNull: true, sensitive: true);
            var guestIdentifier = _guestIdentifierOption.GetValue("guest identifier", requireNotNull: true, promptIfNull: true);


            string rootUrl = Application.IdentityRootUrl;
            var disco = await DiscoveryClient.GetAsync(rootUrl);

            if (string.IsNullOrEmpty(disco.TokenEndpoint))
            {
                Console.WriteLine($"Unable to discover token endpoint from '{rootUrl}' - is the server online?");
                return -1;
            }

            if (string.IsNullOrEmpty(disco.TokenEndpoint))
            {
                Console.WriteLine($"Unable to discover token endpoint from '{rootUrl}' - is the server online?");
                return -1;
            }

            var tokenClient = new TokenClient(disco.TokenEndpoint, clientId, clientSecret);
            var tokenResponse = await tokenClient.RequestCustomGrantAsync("guest-access", "nether-all", new { guest_identifier = guestIdentifier });


            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return -1;
            }

            Console.WriteLine("Token response:");
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            Console.WriteLine("Calling echo API:");
            await EchoClaimsAsync(tokenResponse.AccessToken);
            Console.WriteLine("\n\n");

            Console.WriteLine("Checking role:");
            await ShowPlayerInfoAsync(tokenResponse.AccessToken);
            Console.WriteLine("\n\n");

            return 0;

        }



    }
}
