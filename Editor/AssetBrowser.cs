using ImGuiNET;
using System.Collections.Generic;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using System.Numerics;

enum ToolType { Select, Drag }

namespace Editor {
    public class AssetBrowser
    {
	private string _folderPath = "assets";
	private List<string> _files = new();
	private int _selectedIndex = -1;
	private Texture2D? _previewTexture = null;
	private string? _previewPath = null;
	private ZoomControl _zoomControl = new ZoomControl();
	private ToolType _current = ToolType.Select;
	private string[] labels = Enum.GetNames<ToolType>();

	public AssetBrowser()
	{
	    if (Directory.Exists(_folderPath))
	    {
		_files.AddRange(Directory.GetFiles(_folderPath));
	    }
	}

	public string? SelectedFile => (_selectedIndex >= 0 && _selectedIndex < _files.Count)
	    ? _files[_selectedIndex]
	    : null;

	public void Render()
	{
	    ImGui.Begin("Asset Browsers");

	    if (_files.Count == 0)
	    {
		ImGui.Text("No files found in assets/");
	    }
	    else
	    {
                RenderFiles();
                RenderPreview();
	    }

	    ImGui.End();
	}

        private void RenderFiles() {
	    for (int i = 0; i < _files.Count; i++)
	    {
		string filename = Path.GetFileName(_files[i]);
		bool selected = i == _selectedIndex;

		if (ImGui.Selectable(filename, selected))
		{
		    _selectedIndex = i;
		}

		if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
		{
		    if (_previewTexture.HasValue)
		    {
			Raylib.UnloadTexture(_previewTexture.Value);
		    }

		    _previewPath = _files[i];
		    _previewTexture = Raylib.LoadTexture(_previewPath);
		}
	    }

	}

        public void RenderPreview()
	{
	    if (_previewTexture == null)
		return;

	    ImGui.Begin($"Asset Preview '{_previewPath}'");

	    // Available space for whole content
	    var totalSize = ImGui.GetContentRegionAvail();

	    // Define width for image view (75%) and toolbar (25%)
	    float toolbarWidth = 220f;
	    float imageRegionWidth = totalSize.X - toolbarWidth - 16f; // 16f padding fudge

	    float imageRegionHeight = totalSize.Y;

	    // Left: scrollable image panel
	    ImGui.BeginChild(
		"ImageRegion",
		new Vector2(imageRegionWidth,
			    imageRegionHeight),
		ImGuiChildFlags.None,
		ImGuiWindowFlags.HorizontalScrollbar
	    );

	    float maxWidth = ImGui.GetContentRegionAvail().X;

	    float scale = Math.Min(1.0f, maxWidth / _previewTexture.Value.Width);

	    if (_previewTexture.HasValue)
	    {
		var texture = _previewTexture.Value;
		int zoomedWidth = (int)Math.Ceiling(texture.Width * _zoomControl.Zoom);
		int zoomedHeight = (int)Math.Ceiling(texture.Height * _zoomControl.Zoom);
		rlImGui.ImageSize(
		    texture,
		    zoomedWidth,
		    zoomedHeight
		);
	    }

	    ImGui.EndChild();
	    ImGui.SameLine(); // place next to image

	    ImGui.BeginChild("Toolbar", new Vector2(toolbarWidth + 20, totalSize.Y));

	    _zoomControl.Render();


	    if (ImGui.BeginCombo("Tool", labels[(int)_current]))
	    {
		foreach (ToolType tool in Enum.GetValues<ToolType>())
		{
		    bool selected = tool == _current;
		    if (ImGui.Selectable(labels[(int)tool], selected))
			_current = tool;

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
