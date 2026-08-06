using System.Runtime.CompilerServices;

namespace CardService.src.CardActions.Api.Utils
{
    public static class EnumExtenstions
    {
        public static HashSet<T> ToHashSet<T>(this T source) where T : struct, Enum
            => new HashSet<T> { source };
    }
}
