/*
 * Stub so the generated change detection factory compiles in the test project,
 * which does not reference the real BitfieldChangeDetector implementation.
 */
namespace Modspec.Model;

public class BitfieldChangeDetector<TClient>
{
    public BitfieldChangeDetector<TClient> Track<T>(System.Func<TClient, T> getter, System.Func<T, Level> getLevel) where T : struct, System.Enum
    {
        return this;
    }
}
