using System.Collections.Generic;

namespace StartifyBackend.Helpers
{
    internal class AppAlphHelper
    {
        private static readonly List<AppAlphMatcher> AppAlphaMatchers = new List<AppAlphMatcher>() { new ChineseAppAlphMatcher() };

        public static string GetAppAlpha(string AppNameAlph)
        {
            bool isDigit = char.IsDigit(AppNameAlph[0]);

            if (isDigit) { return "#"; }

            byte[] encodedChar = System.Text.Encoding.Default.GetBytes(AppNameAlph); // Get encoded character

            if (encodedChar.Length == 1) { return AppNameAlph.ToUpper(); } // If char is A-Z, return uppercase A-Z,

            foreach (var appAlphMatcher in AppAlphaMatchers)
            {
                string matchResult = appAlphMatcher.Match(AppNameAlph, encodedChar);
                if (matchResult != null && matchResult != AppAlphMatcher.NO_MATCH)
                {
                    return matchResult;
                }
            }
            return AppNameAlph; // fallback to old logic.
        }
    }
}
