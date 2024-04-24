using System.Text.RegularExpressions;

namespace SFA.DAS.EmployerAccounts.Web.Extensions
{
    public static class UriExtensions
    {
        public static string RemoveQuery(this Uri uri)
        {
            string port = uri.OriginalString.Contains($":{uri.Port}") ? $":{uri.Port}" : string.Empty;
            return Uri.UnescapeDataString($"{uri.Scheme}://{uri.Host}{port}{uri.AbsolutePath}");
        }

        public static string ReplaceHashedAccountId(this Uri uri, string hashedAccountId)
        {
            return Regex.Replace(uri.OriginalString, Regex.Escape("{{hashedAccountId}}"), hashedAccountId);
        }
    }
}
