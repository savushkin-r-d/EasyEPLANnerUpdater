namespace Updater;

/// <summary>
/// Источкник запуска приложения (аргумент при запуске приложения)
/// </summary>
public record class RunSourceArg
{
    /// <summary> При запуске EPLAN (onInit EasyEPLANner) </summary>
    public static readonly RunSourceArg AtStartUpEplan = new("es");

    /// <summary> Из меню EasyEPLAnner </summary>
    public static readonly RunSourceArg FromMenu = new("em");

    /// <summary> Без аргументов: прямой заупуск .exe </summary>
    public static readonly RunSourceArg Default = new("");


    public static implicit operator string(RunSourceArg runSourceArg) => runSourceArg.sourceArg;

    public static implicit operator RunSourceArg(string sourceArg) => sourceArg switch
    {
        "es" => AtStartUpEplan,
        "em" => FromMenu,
        "" => Default,
        _ => Default,
    };

    private readonly string sourceArg;

    private RunSourceArg(string sourceArg)
    {
        this.sourceArg = sourceArg;
    }
}