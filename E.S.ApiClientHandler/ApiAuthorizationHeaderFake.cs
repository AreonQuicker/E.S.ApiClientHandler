using System;
using E.S.ApiClientHandler.Interfaces;

namespace E.S.ApiClientHandler;

public abstract class ApiAuthorizationHeaderFake : IApiAuthorizationHeader
{
    public string GetAuthorizationHeader()
    {
        return null;
    }
}