﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.EmployerAccounts.Web.Extensions;

public static class HtmlExtensions
{
    public static HtmlString CommaSeperatedAddressToHtml(this IHtmlHelper htmlHelper, string commaSeperatedAddress)
    {
        if (string.IsNullOrWhiteSpace(commaSeperatedAddress))
        {
            return new HtmlString(string.Empty);
        }

        var htmlAddress = commaSeperatedAddress
            .Split((char[])[','], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => $"{line.Trim()}<br/>")
            .Aggregate(string.Empty, (x, y) => x + y);
        return new HtmlString(htmlAddress);
    }
}