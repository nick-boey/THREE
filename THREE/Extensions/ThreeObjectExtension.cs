using FastDeepCloner;

namespace THREE;

[Serializable]
public static class ThreeObjectExtension
{
    public static T DeepCopy<T>(this T source) where T : new()
    {
        return (T)DeepCloner.Clone(source);
    }
}