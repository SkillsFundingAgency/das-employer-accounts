namespace SFA.DAS.EmployerAccounts.Web.Extensions
{
    public static class UriExtensions
    {
        public static string WithoutQuery(this Uri uri)
        {
            string port = uri.IsDefaultPort ? "" : $":{uri.Port}";
            return $"{uri.Scheme}://{uri.Host}{port}{uri.AbsolutePath}";
        }
    }
}
