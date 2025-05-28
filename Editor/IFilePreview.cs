namespace Editor
{
    public interface IFilePreview
    {
        void Load(string path);
        void Render();
    }
}
