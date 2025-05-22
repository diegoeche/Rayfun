using ImGuiNET;
using System.Collections.Generic;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;

public class AssetBrowser
{
    private string _folderPath = "assets";
    private List<string> _files = new();
    private int _selectedIndex = -1;
    private Texture2D? _previewTexture = null;

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
        ImGui.Begin("Asset Browser");

        if (_files.Count == 0)
        {
            ImGui.Text("No files found in assets/");
        }
        else
        {
            for (int i = 0; i < _files.Count; i++)
            {
                string filename = Path.GetFileName(_files[i]);
                bool selected = i == _selectedIndex;

                if (ImGui.Selectable(filename, selected))
                {
                    _selectedIndex = i;
                }

		if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
		{
		    if (_previewTexture.HasValue)
		    {
			Raylib.UnloadTexture(_previewTexture.Value);
		    }

		    _previewTexture = Raylib.LoadTexture(_files[i]);
		}
                RenderPreview();
            }
        }

        ImGui.End();
    }

    public void RenderPreview()
    {
	if (_previewTexture == null)
	    return;

	ImGui.Begin("Asset Preview");

	// ImGui.Text(Path.GetFileName(_previewPath));

	float maxWidth = ImGui.GetContentRegionAvail().X;
	float scale = Math.Min(1.0f, maxWidth / _previewTexture.Value.Width);

        if (_previewTexture.HasValue) {
	    rlImGui.Image(_previewTexture.Value);
	}

	ImGui.End();
    }
}
