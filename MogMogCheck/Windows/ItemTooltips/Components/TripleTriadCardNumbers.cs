using System.Numerics;
using Dalamud.Interface.Utility;
using HaselCommon.Graphics;
using HaselCommon.Gui.Yoga;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MogMogCheck.Services;

namespace MogMogCheck.Windows.ItemTooltips.Components;

public class TripleTriadCardNumbers(TripleTriadNumberFontManager tripleTriadNumberFontManager) : Node()
{
    private TripleTriadCardResident? _card;
    private string _textTop = string.Empty;
    private string _textBottom = string.Empty;
    private string _textRight = string.Empty;
    private string _textLeft = string.Empty;

    public void SetCard(TripleTriadCardResident card)
    {
        if (_card != card)
        {
            _card = card;

            _textTop = $"{_card.Top:X}";
            _textBottom = $"{_card.Bottom:X}";
            _textRight = $"{_card.Right:X}";
            _textLeft = $"{_card.Left:X}";
        }
    }

    public override void DrawContent()
    {
        if (_card == null)
            return;

        using var font = tripleTriadNumberFontManager.GetFont().Push();

        var letterSize = ImGui.CalcTextSize("A");
        Width = letterSize.X * 2;
        Height = letterSize.Y * 2;

        var pos = AbsolutePosition;
        var scale = 2.25f;
        var scaledLetterSize = letterSize / scale;

        var positionTop = pos + new Vector2(scaledLetterSize.X, -scaledLetterSize.Y);
        var positionBottom = pos + new Vector2(scaledLetterSize.X, scaledLetterSize.Y);
        var positionRight = pos + new Vector2(letterSize.X * 1.1f + scaledLetterSize.X, 0);
        var positionLeft = pos + new Vector2(-(letterSize.X * 0.1f + scaledLetterSize.X), 0);

        DrawTextShadow(positionTop, _textTop);
        DrawTextShadow(positionBottom, _textBottom);
        DrawTextShadow(positionRight, _textRight);
        DrawTextShadow(positionLeft, _textLeft);

        DrawText(positionTop, _textTop);
        DrawText(positionBottom, _textBottom);
        DrawText(positionRight, _textRight);
        DrawText(positionLeft, _textLeft);
    }

    private static void DrawTextShadow(Vector2 position, string text)
    {
        DrawShadow(ImGui.GetWindowPos() + position, ImGui.CalcTextSize(text), 8, Color.Black with { A = 0.1f });
    }

    private static void DrawText(Vector2 position, string text)
    {
        var outlineColor = Color.Black with { A = 0.5f };

        // outline
        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(-1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.TextUnformatted(text);

        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.TextUnformatted(text);

        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(1, -1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.TextUnformatted(text);

        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(-1, 1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.TextUnformatted(text);

        // text
        ImGui.SetCursorPos(position);
        ImGui.TextUnformatted(text);
    }

    private static void DrawShadow(Vector2 pos, Vector2 size, int layers, Vector4 shadowColor)
    {
        var drawList = ImGui.GetWindowDrawList();

        for (var i = 0; i < layers; i++)
        {
            var shadowOffset = i * 2.0f;
            var transparency = shadowColor.W * (1.0f - (float)i / layers);
            var currentShadowColor = new Vector4(shadowColor.X, shadowColor.Y, shadowColor.Z, transparency);

            drawList.AddRectFilled(
                pos - new Vector2(shadowOffset, shadowOffset),
                pos + size + new Vector2(shadowOffset, shadowOffset),
                ImGui.ColorConvertFloat4ToU32(currentShadowColor),
                50
            );
        }
    }
}