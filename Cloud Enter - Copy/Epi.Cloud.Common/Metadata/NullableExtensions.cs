namespace Epi.Cloud.Common.Metadata
{
    public static class NullableExtensions
    {
        public static int ValueOrDefault(this int? nullableValue) { return nullableValue.HasValue ? nullableValue.Value : 0; }
        public static double ValueOrDefault(this double? nullableValue) { return nullableValue.HasValue ? nullableValue.Value : 0d; }
        public static bool ValueOrDefault(this bool? nullableValue) { return nullableValue.HasValue ? nullableValue.Value : false; }
        public static decimal ValueOrDefault(this decimal? nullableValue) { return nullableValue.HasValue ? nullableValue.Value : 0m; }
    }
}