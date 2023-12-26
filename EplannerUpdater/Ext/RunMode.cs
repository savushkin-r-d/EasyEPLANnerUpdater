namespace Updater;

/// <summary>
/// Режим запуска приложения при запуске EPLAN
/// </summary>
public record class RunMode
{
    /// <summary> Запускать всегда </summary>
    public static readonly RunMode Always = new(0);

    /// <summary> Default: Запускать при наличии обновлений </summary>
    public static readonly RunMode ThereAreUpdates = new(1);
    
    /// <summary> Никогда не запускать </summary>
    public static readonly RunMode Never = new(2);

    public static implicit operator int(RunMode runMode) => runMode.mode;

    private readonly int mode;
    
    private RunMode(int mode)
    {
        this.mode = mode;
    }
}