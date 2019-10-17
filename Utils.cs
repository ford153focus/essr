using System;

namespace essr
{
    public static class Utils
    {
        public static class Dt
        {
            public const int SecsInWeek = 7 * 24 * 60 * 60;

            public static int UnixNow()
            {
                return (int) (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        public static class Validators
        {
            public static bool IsValidUrl(string url)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
        }
    }
}