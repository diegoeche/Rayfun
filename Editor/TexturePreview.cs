using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace Editor
{
    enum ToolType { Select, Drag, SpriteMap }
    public class TexturePreview : IFilePreview
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

	private int _gridSize = 16;
	private Vector2 _initialOffset = Vector2.Zero;
	private Vector2 _cellSpacing = Vector2.Zero;

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
                Log.Write("Mouse Down");

                _isDragging = true;
                _lastMousePos = currentMousePos;
                return;
            }

            if (_isDragging)
            {
		Log.Write("Dragging");

                Vector2 delta = currentMousePos - _lastMousePos;
                _panOffset += delta;
                _lastMousePos = currentMousePos;

                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    Log.Write("Mouse Released");
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
            float toolbarWidth = 300f;
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

	    if (_currentTool == ToolType.SpriteMap)
	    {
		DrawSpriteMapGrid(tex.Width, tex.Height);
	    }

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

	    if (_currentTool == ToolType.SpriteMap)
	    {
		ImGui.Separator();
		ImGui.Text("SpriteMap Grid");

		ImGui.InputInt("Grid Size", ref _gridSize);
		if (_gridSize <= 0) _gridSize = 1;

		ImGui.InputFloat2("Initial Offset", ref _initialOffset);
		ImGui.InputFloat2("Cell Spacing", ref _cellSpacing);

		if (ImGui.Button("Create Empty Atlas"))
		{
		    CreateEmptyAtlas();
		}
	    }

	    ImGui.EndChild();
            ImGui.End();
        }


	private void DrawSpriteMapGrid(int texWidth, int texHeight)
	{
	    var drawList = ImGui.GetWindowDrawList();
	    float zoom = _zoom.Zoom;

	    Vector2 origin = ImGui.GetItemRectMin() + _panOffset;
	    int cols = (int)Math.Floor((texWidth - _initialOffset.X) / (_gridSize + _cellSpacing.X));
	    int rows = (int)Math.Floor((texHeight - _initialOffset.Y) / (_gridSize + _cellSpacing.Y));

	    uint color = ImGui.GetColorU32(ImGuiCol.Border); // Change to whatever color you want

	    for (int y = 0; y <= rows; y++)
	    {
		float yPos = origin.Y + zoom * (_initialOffset.Y + y * (_gridSize + _cellSpacing.Y));
		drawList.AddLine(
		    new Vector2(origin.X + zoom * _initialOffset.X, yPos),
		    new Vector2(origin.X + zoom * (_initialOffset.X + cols * (_gridSize + _cellSpacing.X)), yPos),
		    color
		);
	    }

	    for (int x = 0; x <= cols; x++)
	    {
		float xPos = origin.X + zoom * (_initialOffset.X + x * (_gridSize + _cellSpacing.X));
		drawList.AddLine(
		    new Vector2(xPos, origin.Y + zoom * _initialOffset.Y),
		    new Vector2(xPos, origin.Y + zoom * (_initialOffset.Y + rows * (_gridSize + _cellSpacing.Y))),
		    color
		);
	    }
	}

	private void CreateEmptyAtlas()
	{
	    if (_texture == null || _path == null)
		return;

	    var atlas = new Atlas
	    {
		Texture = System.IO.Path.GetFileName(_path),
		TileWidth = _gridSize,
		TileHeight = _gridSize
	    };

	    int texWidth = _texture.Value.Width;
	    int texHeight = _texture.Value.Height;

	    int cols = (int)Math.Floor((texWidth - _initialOffset.X) / (_gridSize + _cellSpacing.X));
	    int rows = (int)Math.Floor((texHeight - _initialOffset.Y) / (_gridSize + _cellSpacing.Y));

	    for (int y = 0; y < rows; y++)
	    {
		for (int x = 0; x < cols; x++)
		{
		    int px = (int)(_initialOffset.X + x * (_gridSize + _cellSpacing.X));
		    int py = (int)(_initialOffset.Y + y * (_gridSize + _cellSpacing.Y));

		    atlas.Sprites.Add(new SpriteDef
		    {
			Name = $"tile_{x}_{y}",
			X = px,
			Y = py,
			W = _gridSize,
			H = _gridSize
		    });
		}
	    }

	    string outputPath = System.IO.Path.ChangeExtension(_path, ".atlas.json");

	    string json = System.Text.Json.JsonSerializer.Serialize(atlas, new System.Text.Json.JsonSerializerOptions
	    {
		WriteIndented = true
		    });

	    System.IO.File.WriteAllText(outputPath, json);

	    Log.Write($"Saved atlas to {outputPath}");
	}
    }
}
