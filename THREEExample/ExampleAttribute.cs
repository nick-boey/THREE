/**
 * This WinForms project  and Example templates were created by referring to Three.cs( (https://github.com/lathoub/three.cs).
 * */

using System;

namespace THREEExample;

[AttributeUsage(AttributeTargets.Class)]
public class ExampleAttribute : Attribute
{
    public readonly ExampleCategory Category;

    public readonly string Subcategory;

    public string CommonControlName;

    public string Documentation;

    public ExampleAttribute(string title, ExampleCategory category, string subcategory,
        string commonUIControlName = null)
    {
        Title = title;
        Category = category;
        Subcategory = subcategory;
        CommonControlName = commonUIControlName;
    }

    public string Title { get; internal set; }

    public override string ToString()
    {
        return string.Format("{0}: {1}", Category, Title);
    }
}

public enum ExampleCategory
{
    OpenTK = 0,
    LearnThreeJS = 1,
    ThreeJs = 2,
    Misc = 3,
    Others = 4
}