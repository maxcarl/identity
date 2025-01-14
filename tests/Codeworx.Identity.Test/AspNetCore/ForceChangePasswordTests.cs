﻿using System;
using System.Collections.Generic;

namespace Codeworx.Identity.Test.AspNetCore
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Codeworx.Identity.Configuration;
    using Codeworx.Identity.OAuth;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using NUnit.Framework;

    public class ForceChangePasswordTests : IntegrationTestBase
    {
        [Test]
        public async Task LoginWithForcePasswordChangeShouldRedirectToPasswordChange()
        {
            var options = this.TestServer.Host.Services.GetRequiredService<IOptions<IdentityOptions>>();

            var loginRequestBuilder = new UriBuilder(this.TestClient.BaseAddress.ToString());
            loginRequestBuilder.AppendPath(options.Value.AccountEndpoint);
            loginRequestBuilder.AppendPath("login");
            loginRequestBuilder.AppendQueryParameter(Constants.ReturnUrlParameter, "http://example.com");

            var response = await this.TestClient.PostAsync(loginRequestBuilder.ToString(),
                               new FormUrlEncodedContent(new Dictionary<string, string>
                               {
                                   {"provider-id", Constants.FormsLoginProviderId},
                                   {"username", Constants.ForcePasswordUserName},
                                   {"password", Constants.ForcePasswordUserName}
                               }));

            response.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies);

            var authenticationCookie = cookies?.FirstOrDefault(p => p.StartsWith("identity"));

            Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);

            var redirectLocation = response.Headers.Location;
            var expectedRedirectTarget = options.Value.AccountEndpoint + "/change-password";
            Assert.True(redirectLocation.LocalPath.Contains(expectedRedirectTarget));

            var changePasswordContent = new FormUrlEncodedContent(new Dictionary<string, string>
                                             {
                                                 {"username", Constants.ForcePasswordUserName},
                                                 {"current-password", Constants.ForcePasswordUserName},
                                                 {"password", "aaAAbb11!!"},
                                                 {"confirm-password", "aaAAbb11!!"},
                                             });

            changePasswordContent.Headers.Add(HeaderNames.Cookie, new[] { authenticationCookie });

            var changePasswordResponse = await this.TestClient.PostAsync(redirectLocation, changePasswordContent);

            response = await this.TestClient.PostAsync(loginRequestBuilder.ToString(),
                               new FormUrlEncodedContent(new Dictionary<string, string>
                               {
                                   {"provider-id", Constants.FormsLoginProviderId},
                                   {"username", Constants.ForcePasswordUserName},
                                   {"password", "aaAAbb11!!"}
                               }));

            Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
            Assert.AreEqual(new Uri("http://example.com"), response.Headers.Location);
        }

        [Test]
        public async Task LoginWithForcePasswordChangeShouldNotReturnTokens()
        {
            var options = this.TestServer.Host.Services.GetRequiredService<IOptions<IdentityOptions>>();

            var loginRequestBuilder = new UriBuilder(this.TestClient.BaseAddress.ToString());
            loginRequestBuilder.AppendPath(options.Value.AccountEndpoint);
            loginRequestBuilder.AppendPath("login");
            loginRequestBuilder.AppendQueryParameter(Constants.ReturnUrlParameter, "http://example.com");

            var response = await this.TestClient.PostAsync(loginRequestBuilder.ToString(),
                               new FormUrlEncodedContent(new Dictionary<string, string>
                               {
                                   {"provider-id", Constants.FormsLoginProviderId},
                                   {"username", Constants.ForcePasswordUserName},
                                   {"password", Constants.ForcePasswordUserName}
                               }));

            response.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies);

            var authenticationCookie = cookies?.FirstOrDefault(p => p.StartsWith("identity"));
            this.TestClient.DefaultRequestHeaders.Add(HeaderNames.Cookie, new[] { authenticationCookie });

            var request = new OAuthAuthorizationRequestBuilder().WithClientId(Constants.DefaultCodeFlowClientId)
                                                                .Build();

            var requestString = this.ToRequestString(request);

            var authorizationResponse = await this.TestClient.GetAsync(options.Value.OauthAuthorizationEndpoint + requestString);

            Assert.AreEqual(HttpStatusCode.Redirect, authorizationResponse.StatusCode);
            Assert.AreEqual($"{options.Value.AccountEndpoint}/change-password", authorizationResponse.Headers.Location.AbsolutePath);
        }

        private string ToRequestString(AuthorizationRequest request)
        {
            return $"?{Constants.OAuth.ClientIdName}={request.ClientId}" +
                   $"&{Constants.OAuth.RedirectUriName}={request.RedirectUri}" +
                   $"&{Constants.OAuth.ResponseTypeName}={request.ResponseType}" +
                   $"&{Constants.OAuth.ScopeName}={request.Scope}" +
                   $"&{Constants.OAuth.StateName}={request.State}";
        }
    }
}
