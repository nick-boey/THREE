using THREE;

namespace View3D;

/// <summary>
/// Interface for 3D objects that can be selected.
/// </summary>
public interface ISelectableElement
{
    public Material BaseMaterial { get; set; }
    public Material? HoverInactiveMaterial { get; set; }
    public Material? HoverActiveMaterial { get; set; }
    public Material? SelectedMaterial { get; set; }

    public bool HoverEnabled { get; set; }
    public bool SelectionEnabled { get; set; }

    public bool IsInactiveHovered { get; set; }
    public bool IsActiveHovered { get; set; }
    public bool IsSelected { get; set; }

    public void InactiveHover();
    public void ActiveHover();
    public void Unhover();

    public void Select();
    public void Deselect();
}