﻿namespace Codeworx.Identity
{
    public interface IAuthorizationParametersBuilder
    {
        IAuthorizationParameters Parameters { get; }

        IAuthorizationParametersBuilder WithClientId(string clientId);

        IAuthorizationParametersBuilder WithNonce(string nonce);

        IAuthorizationParametersBuilder WithRedirectUri(string redirectUri);

        IAuthorizationParametersBuilder WithResponseMode(string responseMode);

        IAuthorizationParametersBuilder WithResponseTypes(params string[] responseTypes);

        IAuthorizationParametersBuilder WithScopes(params string[] scopes);

        IAuthorizationParametersBuilder WithState(string state);
    }
}