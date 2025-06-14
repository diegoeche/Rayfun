using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using Game;

namespace Editor
{
    public enum MappingStrategy {
	Simple,         // Always use same sprite
	Contextual2D,   // Based on surroundings (e.g., tiles above/below)
	Oriented3D      // Based on voxel orientation
    }
    public class VoxelMapping
    {
	public string SpriteKey { get; set; } = "";
	public MappingStrategy Strategy { get; set; } = MappingStrategy.Simple;
    }

    public class VoxelSpriteMapper
    {
	private readonly GameAtlas _atlas;
	private readonly string[] _initialVoxelTypes = new[] { "grass", "dirt", "water", "sword" };

	private Dictionary<string, VoxelMapping> _mappings = new();
	private int _selectedVoxelIndex = 0;
	private string _voxelFilter = "";
	private string _newVoxelName = "";

	public VoxelSpriteMapper(GameAtlas atlas)
	{
	    _atlas = atlas;

	    // Init with defaults
	    foreach (var voxel in _initialVoxelTypes)
	    {
		_mappings[voxel] = new VoxelMapping();
	    }
	}

	private const string SavePath = "voxel_mappings.json";

	public void SaveToFile()
	{
	    var options = new JsonSerializerOptions { WriteIndented = true };
	    var json = JsonSerializer.Serialize(_mappings, options);
	    File.WriteAllText(SavePath, json);
	}

	public void LoadFromFile()
	{
	    if (!File.Exists(SavePath)) return;

	    var json = File.ReadAllText(SavePath);
	    var loaded = JsonSerializer.Deserialize<Dictionary<string, VoxelMapping>>(json);

	    if (loaded != null)
	    {
		foreach (var (key, value) in loaded)
		{
		    _mappings[key] = value;
		}
	    }
	}


	public void Render()
	{
	    ImGui.Begin("Voxel Sprite Mapper");

	    ImGui.InputText("Filter Voxels", ref _voxelFilter, 100);


	    ImGui.Separator();
	    ImGui.Text("Add New Voxel Type:");

	    ImGui.InputText("##NewVoxel", ref _newVoxelName, 64);
	    ImGui.SameLine();

	    if (ImGui.Button(IconFonts.FontAwesome6.Plus) && !string.IsNullOrWhiteSpace(_newVoxelName))
	    {
		string key = _newVoxelName.Trim().ToLowerInvariant();

		if (!_mappings.ContainsKey(key))
		{
		    _mappings[key] = new VoxelMapping();
		    _newVoxelName = "";
		    Log.Write($"Voxel type '{key}' added.");
		}
		else
		{
		    Log.Write($"Voxel type '{key}' already exists.");
		}
	    }

	    ImGui.Separator();

	    // Scrollable list of all voxel types
	    ImGui.BeginChild("##voxel_mapping_list", new Vector2(0, 400), ImGuiChildFlags.None);

            foreach (var voxelType in _mappings.Keys)
	    {
		if (!string.IsNullOrEmpty(_voxelFilter) &&
		    !voxelType.ToLowerInvariant().Contains(_voxelFilter.ToLowerInvariant()))
		{
		    continue;
		}

		var mapping = _mappings[voxelType];
		ImGui.PushID(voxelType);

		ImGui.Text($"Voxel: '{voxelType}'");

		// SpriteKey input
		string spriteKey = mapping.SpriteKey;
		if (ImGui.InputText("##SpriteKey", ref spriteKey, 256))
		{
		    mapping.SpriteKey = spriteKey;
		}

		// Strategy combo
		if (ImGui.BeginCombo("##Strategy", mapping.Strategy.ToString()))
		{
		    foreach (MappingStrategy strategy in Enum.GetValues<MappingStrategy>())
		    {
			if (ImGui.Selectable(strategy.ToString(), mapping.Strategy == strategy))
			{
			    mapping.Strategy = strategy;
			}
		    }
		    ImGui.EndCombo();
		}

		// Sprite picker
		ImGui.BeginChild("##sprite_picker", new Vector2(0, 100), ImGuiChildFlags.None);
		string filter = mapping.SpriteKey.ToLowerInvariant();

		foreach (var key in _atlas.ListSprites())
		{
		    if (string.IsNullOrEmpty(filter) || key.ToLowerInvariant().Contains(filter))
		    {
			if (!string.IsNullOrEmpty(key) && _atlas.Get(key) is { } sprite)
			{
			    var rect = sprite.SourceRect;
			    var tex = sprite.Texture;
			    float scale = 2.0f;
			    int destWidth = (int)(rect.Width * scale);
			    int destHeight = (int)(rect.Height * scale);

			    rlImGui.ImageRect(tex, destWidth, destHeight, rect);
			    ImGui.SameLine();
			}

			if (ImGui.Selectable(key, key == mapping.SpriteKey))
			{
			    mapping.SpriteKey = key;
			}
		    }
		}

		ImGui.EndChild();

		ImGui.Separator();
		ImGui.PopID();
	    }

	    ImGui.EndChild(); // ##voxel_mapping_list

	    if (ImGui.Button("Save"))
	    {
		SaveToFile();
                Log.Write("Voxel Mappings Saved");
            }
	    ImGui.SameLine();
	    if (ImGui.Button("Load"))
	    {
		LoadFromFile();
	    }
	    ImGui.End(); // Voxel Sprite Mapper
	}
	public Dictionary<string, VoxelMapping> GetMappings()
	{
	    return _mappings;
	}
    }
}
