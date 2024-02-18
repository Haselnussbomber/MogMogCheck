using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;

namespace MogMogCheck.Services;

public class TripleTriadNumberFontManager : IDisposable
{
    private IFontHandle? TripleTriadNumberFont;

    public IFontHandle GetFont()
    {
        return TripleTriadNumberFont ??= Service.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 208f / 10f));
    }

    public void Dispose()
    {
        TripleTriadNumberFont?.Dispose();
    }
}
