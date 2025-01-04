using THREE;

namespace View3D;

public class SelectableElement : Element, ISelectableElement
{
    public Material BaseMaterial { get; set; }
    public Material? HoverInactiveMaterial { get; set; }
    public Material? HoverActiveMaterial { get; set; }
    public Material? SelectedMaterial { get; set; }

    public bool HoverEnabled { get; set; } = true;
    public bool SelectionEnabled { get; set; } = true;
    public bool IsInactiveHovered { get; set; }
    public bool IsActiveHovered { get; set; }
    public bool IsSelected { get; set; }

    public SelectableElement(Geometry geometry, Material material, Material? hoverInactiveMaterial = null,
        Material? hoverActiveMaterial = null, Material? selectedMaterial = null)
    {
        Geometry = geometry;
        BaseMaterial = material;
        Material = material;

        if (hoverInactiveMaterial == null || hoverActiveMaterial == null)
        {
            HoverEnabled = false;
        }

        if (selectedMaterial == null)
        {
            SelectionEnabled = false;
        }

        HoverInactiveMaterial = hoverInactiveMaterial;
        HoverActiveMaterial = hoverActiveMaterial;
        SelectedMaterial = selectedMaterial;
    }

    public void InactiveHover()
    {
        if (!HoverEnabled) return;
        if (IsInactiveHovered) return;

        IsInactiveHovered = true;
        IsActiveHovered = false;

        // Selection takes precedence over hover
        if (!IsSelected)
        {
            Material = HoverInactiveMaterial ?? BaseMaterial;
        }
    }

    public void ActiveHover()
    {
        if (!HoverEnabled) return;
        if (IsActiveHovered) return;

        IsInactiveHovered = false;
        IsActiveHovered = true;

        // Selection takes precedence over hover
        if (!IsSelected)
        {
            Material = HoverActiveMaterial ?? BaseMaterial;
        }
    }

    public void Unhover()
    {
        IsInactiveHovered = false;
        IsActiveHovered = false;

        // Selection takes precedence over hover
        if (!IsSelected)
        {
            Material = BaseMaterial;
        }
    }

    public void Select()
    {
        if (!SelectionEnabled) return;

        IsSelected = true;
        Material = SelectedMaterial ?? BaseMaterial;
    }

    public void Deselect()
    {
        IsSelected = false;
        Material = BaseMaterial;
    }
}