using System.Linq.Expressions;

namespace ExtensionsTesting;

// https://stackoverflow.com/questions/5489987/linq-full-outer-join
public static class EnumerableExtension
{
    public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(
        this IEnumerable<TLeft> leftItems,
        IEnumerable<TRight> rightItems,
        Func<TLeft, TKey> leftKeySelector,
        Func<TRight, TKey> rightKeySelector,
        Func<TLeft, TRight, TResult> resultSelector)
        where TKey : EqualityComparer<TKey>

    {
        var intersecting = from left in leftItems
            from right in rightItems
            where leftKeySelector(left) == rightKeySelector(right)
            select (left, right);
        
        var x = leftItems
            .SelectMany(left => rightItems, (left, right) => new { left, right })
            .Select(@t => (@t.left, @t.right));
        
        // var leftOnly = leftItems.Ex

        return from left in leftItems

            join right in rightItems on leftKeySelector(left) equals rightKeySelector(right) into temp
            from right in temp.DefaultIfEmpty()
            select resultSelector(left, right);
    }


}