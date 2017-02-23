namespace Epi.Common.Utilities
{
    public static class WildcardExtension
    {
        public static bool WildCardCompare(this string value, string pattern)
        {
            value = value.ToLowerInvariant();
            pattern = pattern.ToLowerInvariant();

            int pos = 0;
            while (pattern.Length != pos)
            {
                switch (pattern[pos])
                {
                    case '?':
                        break;

                    case '*':
                        for (int i = value.Length; i >= pos; i--)
                        {
                            if (WildCardCompare(value.Substring(i), pattern.Substring(pos + 1)))
                            {
                                return true;
                            }
                        }
                        return false;

                    default:
                        if (value.Length == pos || (pattern.Length > pos && value.Length > pos && pattern[pos] != value[pos]))
                        {
                            return false;
                        }
                        break;
                }

                pos++;
            }

            return value.Length == pos;
        }
    }
}
