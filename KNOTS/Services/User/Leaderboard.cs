using System;
using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

public class Leaderboard<TPlayer>
    where TPlayer : class, IComparable<TPlayer>, IEquatable<TPlayer>
{
    private readonly IReadOnlyCollection<TPlayer> _players;

    public Leaderboard(IEnumerable<TPlayer> players)
    {
        if (players is null)
            throw new ArgumentNullException(nameof(players));

        _players = players.ToList();
    }

    public IReadOnlyList<TResult> SelectTop<TResult>(int count, Func<TPlayer, TResult> projector)
        where TResult : notnull
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (projector is null)
            throw new ArgumentNullException(nameof(projector));

        return _players
            .OrderBy(p => p)
            .Take(count)
            .Select(projector)
            .ToList();
    }
}
