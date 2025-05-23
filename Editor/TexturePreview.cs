using ImGuiNET;
using System.Collections.Generic;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;


namespace Editor {
    enum ToolType { Select, Drag }

    public class TexturePreview
    {
        private Texture2D? _texture = null;
        private string? _path = null;
        private ZoomControl _zoom = new ZoomControl();
        private ToolType _currentTool = ToolType.Select;
        private string[] _labels = Enum.GetNames<ToolType>();

        public void Load(string path)
        {
            if (_texture.HasValue)
            {
                Raylib.UnloadTexture(_texture.Value);
            }

            _path = path;
            _texture = Raylib.LoadTexture(path);
        }

        public void Render()
        {
            if (_texture == null)
                return;

            ImGui.Begin($"Asset Preview '{_path}'");

            var totalSize = ImGui.GetContentRegionAvail();
            float toolbarWidth = 220f;
            float imageRegionWidth = totalSize.X - toolbarWidth - 16f;

            ImGui.BeginChild("ImageRegion", new Vector2(imageRegionWidth, totalSize.Y), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

            float maxWidth = ImGui.GetContentRegionAvail().X;
            float scale = Math.Min(1.0f, maxWidth / _texture.Value.Width);

            var tex = _texture.Value;
            rlImGui.ImageSize(tex, (int)(tex.Width * _zoom.Zoom), (int)(tex.Height * _zoom.Zoom));

            ImGui.EndChild();
            ImGui.SameLine();

            ImGui.BeginChild("Toolbar", new Vector2(toolbarWidth + 20, totalSize.Y));

            _zoom.Render();

            if (ImGui.BeginCombo("Tool", _labels[(int)_currentTool]))
            {
                foreach (ToolType tool in Enum.GetValues<ToolType>())
                {
                    bool selected = tool == _currentTool;
                    if (ImGui.Selectable(_labels[(int)tool], selected))
                        _currentTool = tool;

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.EndChild();
            ImGui.End();
        }
    }
}
