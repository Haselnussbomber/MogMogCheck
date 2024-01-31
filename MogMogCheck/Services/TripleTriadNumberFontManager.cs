using Dalamud.Interface.GameFonts;

namespace MogMogCheck.Services;

public class TripleTriadNumberFontManager : IDisposable
{
    private GameFontHandle? TripleTriadNumberFont;
    private float TripleTriadNumberFontSize;

    public GameFontHandle GetFont(float size)
    {
        if (TripleTriadNumberFont == null || TripleTriadNumberFontSize != size)
        {
            TripleTriadNumberFont?.Dispose();
            TripleTriadNumberFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, size));
            TripleTriadNumberFontSize = size;
        }

        return TripleTriadNumberFont;
    }

    public void Dispose()
    {
        TripleTriadNumberFont?.Dispose();
    }
}
