/*
 * Simplified BitfieldChangeDetector for testing the generated factory.
 * The real implementation lives in the consuming project
 * and may use more sophisticated change tracking.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modspec.Model;

public class BitfieldChangeDetector<TClient>
{
    private readonly List<ITracker> _trackers = [];

    public BitfieldChangeDetector<TClient> Track<T>(Func<TClient, T> getter, Func<T, Level> getLevel) where T : struct, Enum
    {
        _trackers.Add(new Tracker<T>(getter, getLevel));
        return this;
    }

    public async ValueTask CheckAsync(TClient client, Func<ulong, string, Level, ValueTask> onChanged)
    {
        bool anyChanged = false;
        ulong combinedCode = 0;
        Level highestLevel = Level.None;
        List<string>? descriptions = null;

        foreach (ITracker tracker in _trackers)
        {
            bool changed = tracker.TryCheck(client, out ulong code, out string desc, out Level level);
            anyChanged |= changed;
            combinedCode |= code;
            if (level > highestLevel) highestLevel = level;
            if (level != Level.None)
            {
                descriptions ??= [];
                descriptions.Add(desc);
            }
        }

        if (anyChanged)
        {
            string combined = descriptions is not null
                ? String.Join(", ", descriptions)
                : String.Empty;
            await onChanged(combinedCode, combined, highestLevel);
        }
    }

    private interface ITracker
    {
        bool TryCheck(TClient client, out ulong code, out string desc, out Level level);
    }

    private class Tracker<T> : ITracker where T : struct, Enum
    {
        private ulong _previous;
        private readonly Func<TClient, T> _getter;
        private readonly Func<T, Level> _getLevel;

        public Tracker(Func<TClient, T> getter, Func<T, Level> getLevel)
        {
            _getter = getter;
            _getLevel = getLevel;
        }

        public bool TryCheck(TClient client, out ulong code, out string desc, out Level level)
        {
            T current = _getter(client);
            code = Convert.ToUInt64(current);
            bool changed = code != _previous;
            _previous = code;
            level = _getLevel(current);
            desc = current.ToString();
            return changed;
        }
    }
}
