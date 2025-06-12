using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using Game;

namespace Editor
{
    public class MapExplorer
    {
        private readonly string _mapFolder = "maps";
        private List<string> _mapFiles = new();
        private int _selectedIndex = -1;
        private string _filter = "";
        private bool _showNewModal = false;
        private bool _showSaveAsModal = false;
        private string _filenameInput = "";

        public string? SelectedMap =>
            (_selectedIndex >= 0 && _selectedIndex < _mapFiles.Count)
                ? _mapFiles[_selectedIndex]
                : null;

        public bool RequestedLoad { get; private set; } = false;

	public string? FirstMap =>
	    _mapFiles.Count > 0 ? _mapFiles[0] : null;

	public string? LastMap =>
	    _mapFiles.Count > 0 ? _mapFiles[^1] : null;

        public MapExplorer()
        {
            ReloadMapList();
        }

        public void ClearRequests()
        {
            RequestedLoad = false;
        }

        public void Render(IMap map)
        {
            ImGui.Begin("Map Explorer");

            ImGui.InputText("Filter", ref _filter, 64);
            RenderMapList();

            if (ImGui.Button("New"))
            {
                _filenameInput = "untitled";
                _showNewModal = true;
            }

            ImGui.SameLine();
            if (ImGui.Button("Save As"))
            {
                _filenameInput = SelectedMap != null
                    ? Path.GetFileNameWithoutExtension(SelectedMap)
                    : "exported_map";
                _showSaveAsModal = true;
            }

            RenderModals(map);
            ImGui.End();
        }

        private void RenderMapList()
        {
            for (int i = 0; i < _mapFiles.Count; i++)
            {
                string filename = Path.GetFileName(_mapFiles[i]);

                if (!string.IsNullOrWhiteSpace(_filter) &&
                    !filename.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool selected = i == _selectedIndex;
                if (ImGui.Selectable(filename, selected))
                {
                    _selectedIndex = i;
                }

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    _selectedIndex = i;
                    RequestedLoad = true;
                }
            }
        }

        private void RenderModals(IMap map)
        {
            if (_showNewModal)
            {
                ImGui.OpenPopup("New Map");
                _showNewModal = false;
            }

            if (ImGui.BeginPopupModal("New Map", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.InputText("Filename", ref _filenameInput, 64);
                if (ImGui.Button("Create"))
                {
                    string path = Path.Combine(_mapFolder, _filenameInput + ".json");
                    File.WriteAllText(path, "[]");
                    ReloadMapList();
                    _selectedIndex = _mapFiles.IndexOf(path);
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            if (_showSaveAsModal)
            {
                ImGui.OpenPopup("Save Map As");
                _showSaveAsModal = false;
            }

            if (ImGui.BeginPopupModal("Save Map As", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.InputText("Filename", ref _filenameInput, 64);
                if (ImGui.Button("Save"))
                {
                    string path = Path.Combine(_mapFolder, _filenameInput + ".json");
                    MapSerializer.Save(map, path);
                    ReloadMapList();
                    _selectedIndex = _mapFiles.IndexOf(path);
                    Log.Write($"Saved map as: {path}");
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        private void ReloadMapList()
        {
            _mapFiles.Clear();
            if (Directory.Exists(_mapFolder))
            {
                _mapFiles.AddRange(Directory.GetFiles(_mapFolder, "*.json"));
            }
        }
    }
}
