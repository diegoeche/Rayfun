using ImGuiNET;

namespace Editor
{
    public class EntityEditor
    {
        public void Render()
        {
            ImGui.Begin("Entity Editor");

            // Button to create a new entity
            if (ImGui.Button("Create Entity"))
            {
                CreateNewEntity();
            }

            ImGui.End();
        }

        private void CreateNewEntity()
        {
            // Logic to create a new entity
            Log.Write("New entity created.");
        }
    }
}
