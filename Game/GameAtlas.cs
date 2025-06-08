using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Raylib_cs;
using Editor;

namespace Game
{
    public class SpriteEntry
    {
	public string Key { get; set; } = "";
	public Texture2D Texture { get; set; }
	public Rectangle SourceRect { get; set; }
    }

    public class GameAtlas
    {
        private readonly Dictionary<string, SpriteEntry> _sprites = new();
        private readonly Dictionary<string, Texture2D> _loadedTextures = new(); // texture cache
        private readonly Dictionary<string, string> _textureToAtlasFile = new(); // optional: for reverse lookup

        public void Load(string atlasJsonPath)
        {
            string json = File.ReadAllText(atlasJsonPath);
            var atlas = JsonSerializer.Deserialize<Atlas>(json);
            if (atlas == null || string.IsNullOrWhiteSpace(atlas.Texture)) return;

            string atlasDir = Path.GetDirectoryName(atlasJsonPath) ?? "";
            string texturePath = Path.GetFullPath(Path.Combine(atlasDir, atlas.Texture));

            if (!File.Exists(texturePath)) return;

            if (!_loadedTextures.TryGetValue(texturePath, out var texture))
            {
                texture = Raylib.LoadTexture(texturePath);
                _loadedTextures[texturePath] = texture;
                _textureToAtlasFile[texturePath] = atlasJsonPath;
            }

            string namespaceKey = Path.GetFileNameWithoutExtension(atlasJsonPath);

            foreach (var sprite in atlas.Sprites)
            {
                string compositeKey = $"{namespaceKey}:{sprite.Name}";
                _sprites[compositeKey] = new SpriteEntry
                {
                    Key = compositeKey,
                    Texture = texture,
                    SourceRect = new Rectangle(sprite.X, sprite.Y, sprite.W, sprite.H)
                };
            }
        }

        public SpriteEntry? Get(string key) => _sprites.TryGetValue(key, out var entry) ? entry : null;

        // 🔍 List loaded atlas JSON files
        public IEnumerable<string> ListAtlas()
        {
            return _textureToAtlasFile.Values.Distinct();
        }

        public IEnumerable<string> ListSprites()
        {
            return _sprites.Keys;
        }
    }

}
