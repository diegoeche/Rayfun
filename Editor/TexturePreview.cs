using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;

namespace Editor
{
    enum ToolType { Select, Drag }
    public class TexturePreview
    {
        private Texture2D? _texture = null;
        private string? _path = null;
        private ZoomControl _zoom = new ZoomControl();
        private ToolType _currentTool = ToolType.Select;
        private string[] _labels = Enum.GetNames<ToolType>();

        private Vector2 _panOffset = Vector2.Zero;
        private bool _isDragging = false;
        private Vector2 _lastMousePos = Vector2.Zero;
        private bool _isOverImageRegion = false;

        public void Load(string path)
        {
            if (_texture.HasValue)
            {
                Raylib.UnloadTexture(_texture.Value);
            }

            _path = path;
            _texture = Raylib.LoadTexture(path);
            _panOffset = Vector2.Zero;
        }

        public void HandleInput()
        {
            if (_currentTool != ToolType.Drag)
                return;

            if (!ImGui.IsItemActive())
                return;

            var io = ImGui.GetIO();
            Vector2 currentMousePos = io.MousePos;

            if (!_isDragging && ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                _isDragging = true;
                _lastMousePos = currentMousePos;
                return;
            }

            if (_isDragging)
            {
                Vector2 delta = currentMousePos - _lastMousePos;
                _panOffset += delta;
                _lastMousePos = currentMousePos;

                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    _isDragging = false;
                    _lastMousePos = Vector2.Zero;
                }
            }
        }

        public void Render()
        {
            if (_texture == null) return;

            ImGui.Begin($"Texture Preview '{_path}'");

            var totalSize = ImGui.GetContentRegionAvail();
            float toolbarWidth = 220f;
            float imageRegionWidth = totalSize.X - toolbarWidth - 16f;

            ImGui.BeginChild("ImageRegion", new Vector2(imageRegionWidth, totalSize.Y), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

            _isOverImageRegion = ImGui.IsWindowHovered();

            float maxWidth = ImGui.GetContentRegionAvail().X;

            var tex = _texture.Value;
            int zoomedWidth = (int)(tex.Width * _zoom.Zoom);
            int zoomedHeight = (int)(tex.Height * _zoom.Zoom);

            // Draw invisible input-absorbing region
            ImGui.InvisibleButton("ImageDragRegion", new Vector2(zoomedWidth, zoomedHeight));

            // Apply panning offset
            ImGui.SetCursorPos(_panOffset);

            HandleInput();

            rlImGui.ImageSize(tex, zoomedWidth, zoomedHeight);

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
