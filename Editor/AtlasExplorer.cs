using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace Editor
{
    public class AtlasExplorer : IFilePreview
    {
        private Atlas? _atlas = null;
        private Texture2D? _texture = null;
        private bool _showPreview = true;
        private string? _lastLoadedPath = null;
	private Dictionary<string, string> _editBuffer = new();
	private bool _expandAll = false;
	private string _nameFilter = "";
        private int _visibleTiles = 0;

        public void Save()
	{
	    if (_atlas == null || string.IsNullOrEmpty(_lastLoadedPath)) return;

	    string json = JsonSerializer.Serialize(_atlas, new JsonSerializerOptions
	    {
		WriteIndented = true
	    });

	    File.WriteAllText(_lastLoadedPath, json);
	    Log.Write("Atlas saved.");
	}

	public void Load(string atlasPath)
	{
	    // Clean up previously loaded texture
	    if (_texture.HasValue)
	    {
		Raylib.UnloadTexture(_texture.Value);
	    }

	    _lastLoadedPath = atlasPath;

	    if (!File.Exists(atlasPath)) return;

	    string json = File.ReadAllText(atlasPath);
	    _atlas = JsonSerializer.Deserialize<Atlas>(json);

	    if (_atlas == null || string.IsNullOrWhiteSpace(_atlas.Texture)) return;

	    // Resolve texture path relative to the atlas path
	    string atlasDir = Path.GetDirectoryName(atlasPath) ?? "";
	    string texturePath = Path.Combine(atlasDir, _atlas.Texture);

	    if (File.Exists(texturePath))
	    {
		_texture = Raylib.LoadTexture(texturePath);
	    }
	    else
	    {
		Log.Write($"Texture not found: {texturePath}");
	    }
	}

	public void Render()
	{
	    if (_atlas == null || _texture == null)
		return;

	    ImGui.Begin("Atlas Explorer");
	    ImGui.InputText("Filter", ref _nameFilter, 64);
	    ImGui.Text($"Texture: {_atlas.Texture}");
	    ImGui.Text($"Tiles: ({_visibleTiles}/{_atlas.Sprites.Count})");
	    ImGui.Checkbox("Show Preview", ref _showPreview);
	    if (ImGui.Button("Save Atlas"))
	    {
		Save();
	    }

            ImGui.SameLine();

            if (ImGui.Button(_expandAll ? "Collapse All" : "Expand All"))
	    {
		_expandAll = !_expandAll;
	    }

	    ImGui.Separator();

	    ImGui.BeginChild("AtlasList");
            _visibleTiles = 0;

	    for (int i = 0; i < _atlas.Sprites.Count; i++)
	    {

                RenderTileInfo(i, _atlas.Sprites[i]);
            }

	    ImGui.EndChild();
	    ImGui.End();
	}

	private void RenderTileInfo(int index, SpriteDef sprite)
	{
	    ImGui.PushID(index);

	    if (!string.IsNullOrWhiteSpace(_nameFilter) &&
		!sprite.Name.Contains(_nameFilter, StringComparison.OrdinalIgnoreCase))
	    {
		ImGui.PopID();
		return;
	    }
            _visibleTiles = _visibleTiles + 1;

            if (!_editBuffer.ContainsKey(sprite.Name))
	    {
		_editBuffer[sprite.Name] = sprite.Name;
	    }

	    string temp = _editBuffer[sprite.Name];
	    if (ImGui.InputText("##Name", ref temp, 64))
	    {
		_editBuffer[sprite.Name] = temp;
		sprite.Name = temp;
	    }

	    ImGui.SetNextItemOpen(_expandAll, ImGuiCond.Always);

	    if (ImGui.TreeNode("Sprite Info"))
	    {
		ImGui.Text($"Position: ({sprite.X}, {sprite.Y})");
		ImGui.Text($"Size: {sprite.W}x{sprite.H}");

		if (_showPreview)
		{
		    float scale = 2.0f;
		    int destWidth = (int)(sprite.W * scale);
		    int destHeight = (int)(sprite.H * scale);

		    var sourceRect = new Rectangle
		    {
			X = sprite.X,
			Y = sprite.Y,
			Width = sprite.W,
			Height = sprite.H
		    };

		    rlImGui.ImageRect(_texture.Value, destWidth, destHeight, sourceRect);
		}

		ImGui.TreePop();
	    }

	    ImGui.PopID();
	    ImGui.Separator();
	}

    }
}
