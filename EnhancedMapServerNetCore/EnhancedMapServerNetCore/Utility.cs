using System.Linq;
using System.Xml;

namespace EnhancedMapServerNetCore
{
    public static class Utility
    {
        private static readonly char[] _ForbiddenChars = {'<', '>', ':', '"', '/', '\\', '|', '?', '*'};

        public static string GetText(in XmlElement node, in string defaultValue)
        {
            return node?.InnerText ?? defaultValue;
        }

        public static bool IsStringAllowed(in string s)
        {
            return !string.IsNullOrEmpty(s) && !s.Any(x => char.IsWhiteSpace(x) || _ForbiddenChars.Contains(x));
        }
    }
}