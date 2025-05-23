using ImGuiNET;
using System.Collections.Generic;
using Raylib_cs;
using rlImGui_cs;
using System.Numerics;


namespace Editor
{
    public class TextureExplorer
    {
        private string _folderPath = "assets";
        private List<string> _files = new();
        private int _selectedIndex = -1;
        private TexturePreview _preview = new TexturePreview();

        public TextureExplorer()
        {
            if (Directory.Exists(_folderPath))
            {
                _files.AddRange(Directory.GetFiles(_folderPath));
            }
        }

        public string? SelectedFile => (_selectedIndex >= 0 && _selectedIndex < _files.Count) ? _files[_selectedIndex] : null;

        public void Render()
        {
            ImGui.Begin("Texture Explorer");

            if (_files.Count == 0)
            {
                ImGui.Text("No files found in assets/");
            }
            else
            {
                RenderFiles();
                _preview.Render();
            }

            ImGui.End();
        }

        private void RenderFiles()
        {
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
                    _preview.Load(_files[i]);
                }
            }
        }
    }
}
