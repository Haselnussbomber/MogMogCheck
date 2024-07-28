using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Plugin;

namespace MogMogCheck.Services;

public class TripleTriadNumberFontManager(IDalamudPluginInterface PluginInterface) : IDisposable
{
    private IFontHandle? TripleTriadNumberFont;

    public IFontHandle GetFont()
    {
        return TripleTriadNumberFont ??= PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 208f / 10f));
    }

    public void Dispose()
    {
        TripleTriadNumberFont?.Dispose();
        GC.SuppressFinalize(this);
    }
}
