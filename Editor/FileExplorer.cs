using ImGuiNET;
using System.Collections.Generic;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;


namespace Editor
{
    public class FileExplorer
    {
        private string _folderPath = "assets";
        private List<string> _files = new();
        private int _selectedIndex = -1;
        private TexturePreview _texturePreview = new TexturePreview();
        private AtlasExplorer _atlasExplorer = new AtlasExplorer();
        private string _filter = "";

        public FileExplorer()
        {
            if (Directory.Exists(_folderPath))
            {
                _files.AddRange(Directory.GetFiles(_folderPath));
            }
        }

        public string? SelectedFile => (_selectedIndex >= 0 && _selectedIndex < _files.Count) ? _files[_selectedIndex] : null;

        public void Render()
        {
            ImGui.Begin("File Explorer");

            if (_files.Count == 0)
            {
                ImGui.Text("No files found in assets/");
            }
            else
            {
		ImGui.InputText("Filter", ref _filter, 64);
                RenderFiles();
                _texturePreview.Render();
                _atlasExplorer.Render();
            }

            ImGui.End();
        }


	private string GetIconForFile(string path)
	{
	    string lower = path.ToLower();

	    if (lower.EndsWith(".png") || lower.EndsWith(".jpg") || lower.EndsWith(".jpeg"))
                return IconFonts.FontAwesome6.Image;
            if (lower.EndsWith(".atlas.json"))
                return IconFonts.FontAwesome6.PuzzlePiece;
            if (lower.EndsWith(".json"))
                return IconFonts.FontAwesome6.File;
            if (lower.EndsWith(".mp3") || lower.EndsWith(".wav"))
                return IconFonts.FontAwesome6.Music;

            return "";
        }

        private void RenderFiles()
        {
            for (int i = 0; i < _files.Count; i++)
            {
                string filename = Path.GetFileName(_files[i]);
                bool selected = i == _selectedIndex;
		if (!string.IsNullOrWhiteSpace(_filter) &&
		    !filename.Contains(_filter, StringComparison.OrdinalIgnoreCase))
		{
		    continue;
		}

		string icon = GetIconForFile(filename);
                // Combine icon and name with spacing
                string display = $"{icon}  {filename}";
                if (ImGui.Selectable(display, selected))
                {
                    _selectedIndex = i;
                }


		if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
		{
		    string lower = _files[i].ToLower();
		    if (lower.EndsWith(".png") || lower.EndsWith(".jpg"))
		    {
			_texturePreview.Load(_files[i]);
		    }
		    else if (lower.EndsWith(".atlas.json"))
		    {
			_atlasExplorer.Load(_files[i]);
		    }
		}
            }
        }
    }
}
