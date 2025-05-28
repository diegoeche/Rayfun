namespace Editor
{
    public class SpriteDef
    {
        public string Name { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    public class Atlas
    {
        public string Texture { get; set; } = "";
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public List<SpriteDef> Sprites { get; set; } = new();
    }
}
