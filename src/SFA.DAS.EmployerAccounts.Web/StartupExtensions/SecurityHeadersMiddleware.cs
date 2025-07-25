﻿using Microsoft.Extensions.Primitives;
using SFA.DAS.EmployerAccounts.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        const string dasCdn = "das-at-frnt-end.azureedge.net das-pp-frnt-end.azureedge.net das-mo-frnt-end.azureedge.net das-test-frnt-end.azureedge.net das-test2-frnt-end.azureedge.net das-prd-frnt-end.azureedge.net https://das-demo-frnt-end.azureedge.net";

        context.Response.Headers.AddIfNotPresent("x-frame-options", new StringValues("DENY"));
        context.Response.Headers.AddIfNotPresent("x-content-type-options", new StringValues("nosniff"));
        context.Response.Headers.AddIfNotPresent("X-Permitted-Cross-Domain-Policies", new StringValues("none"));
        context.Response.Headers.AddIfNotPresent("x-xss-protection", new StringValues("0"));
        context.Response.Headers.AddIfNotPresent("Content-Security-Policy", 
            new StringValues(
                $"default-src *; " +
                $"script-src 'self' 'unsafe-inline' 'unsafe-eval' {dasCdn} " +
                "*.googletagmanager.com *.postcodeanywhere.co.uk *.google-analytics.com *.googleapis.com https://*.zdassets.com https://*.zendesk.com wss://*.zendesk.com wss://*.zopim.com https://*.rcrsv.io;" +
                "connect-src *; " +
                "img-src *; " +
                $"style-src 'self' 'unsafe-inline' {dasCdn} https://tagmanager.google.com https://fonts.googleapis.com https://*.rcrsv.io ; " +
                "object-src *; " +
                "worker-src 'self' blob:;"));

        await next(context);
    }
}