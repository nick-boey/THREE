using THREE;

namespace View3D;

/// <summary>
/// Interface for 3D objects that can be selected.
/// </summary>
public interface ISelectableObject
{
    public Material BaseMaterial { get; set; }
    public Material? HoverInactiveMaterial { get; set; }
    public Material? HoverActiveMaterial { get; set; }
    public Material? SelectedMaterial { get; set; }

    public bool HoverEnabled { get; set; }
    public bool SelectionEnabled { get; set; }

    public bool IsHovered { get; set; }
    public bool IsSelected { get; set; }

    public void Hover();
    public void Unhover();
    
    public void Select();
    public void Deselect();
}