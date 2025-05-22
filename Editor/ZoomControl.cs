using ImGuiNET;
using System.Collections.Generic;
using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using System.Numerics;

namespace Editor
{
    class ZoomControl
    {
        private float _zoom = 1.0f;
        private const float MinZoom = 0.1f;
        private const float MaxZoom = 3.0f;

        public float Zoom => _zoom;

        public void Render()
        {

            ImGui.Text("Zoom");

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1, 2));
            ImGui.BeginGroup();

            // Slider
            if (ImGui.Button("[-]"))
            {
                _zoom -= 0.1f;
            }
            ImGui.SameLine();

            ImGui.PushItemWidth(120f);
            ImGui.SliderFloat("##zoomSlider", ref _zoom, MinZoom, MaxZoom, $"{_zoom:0.00}x", ImGuiSliderFlags.AlwaysClamp);
            ImGui.PopItemWidth();

            ImGui.SameLine();

            // Slider
            if (ImGui.Button("[+]"))
            {
                _zoom += 0.1f;
            }

            ImGui.SameLine();
            // Reset Button
            if (ImGui.Button("Reset"))
            {
                _zoom = 1.0f;
            }

            ImGui.PopStyleVar();
            ImGui.EndGroup();
        }
    }
}
