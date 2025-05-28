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

            ImGui.Text($"Texture: {_atlas.Texture}");
            ImGui.Text($"Tiles: {_atlas.Sprites.Count}");
            ImGui.Checkbox("Show Preview", ref _showPreview);
            ImGui.Separator();

            ImGui.BeginChild("AtlasList");

            foreach (var sprite in _atlas.Sprites)
            {
                bool nodeOpen = ImGui.TreeNode(sprite.Name);
                if (nodeOpen)
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
            }

            ImGui.EndChild();
            ImGui.End();
        }
    }
}
