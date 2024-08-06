using Microsoft.International.Converters.PinYinConverter;

namespace StartifyBackend.Helpers
{
    internal interface AppAlphMatcher
    {
        string Match(string AppNameAlph, byte[] EncodedChar);


        static string NO_MATCH = "NO_MATCH";
    }

    internal class ChineseAppAlphMatcher : AppAlphMatcher
    {

        public string Match(string AppNameAlph, byte[] EncodedChar)
        {
            var chineseChar = new ChineseChar(AppNameAlph[0]);
            if (chineseChar.PinyinCount <= 0) return AppAlphMatcher.NO_MATCH;
            return chineseChar.Pinyins[0][0..1];
        }
    }
}
