using System.Text.Json;
using System.Text.Json.Serialization;
using Game;

namespace Editor
{
    public static class MapSerializer
    {
        private class SerializableVoxel
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public string Type { get; set; } = "";
        }

        public static void Save(IMap map, string filePath)
        {
            var voxels = new List<SerializableVoxel>();
            foreach (var (pos, voxel) in map.Enumerate())
            {
                voxels.Add(new SerializableVoxel
                {
                    X = pos.x,
                    Y = pos.y,
                    Z = pos.z,
                    Type = voxel.Type
                });
            }

            var json = JsonSerializer.Serialize(voxels, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static IMap Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var voxels = JsonSerializer.Deserialize<List<SerializableVoxel>>(json) ?? new();

            var map = new SparseMap();
            foreach (var v in voxels)
            {
                map.Set(v.X, v.Y, v.Z, new Voxel(v.Type));
            }

            return map;
        }
    }
}
