using UnityEditor;
using UnityEngine;

public static class RemoveScaleCurves
{
    [MenuItem("CONTEXT/AnimationClip/Remove Scale Curves")]
    static void RemoveScale(MenuCommand command)
    {
        AnimationClip clip = (AnimationClip)command.context;
        RemoveMatchingCurves(clip, "m_LocalScale");
    }

    [MenuItem("CONTEXT/AnimationClip/Remove Position Curves")]
    static void RemovePosition(MenuCommand command)
    {
        AnimationClip clip = (AnimationClip)command.context;
        RemoveMatchingCurves(clip, "m_LocalPosition");
    }

    [MenuItem("Assets/Remove Scale Curves From Selected Clip")]
    static void RemoveScaleFromSelected()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is AnimationClip clip)
            {
                RemoveMatchingCurves(clip, "m_LocalScale");
            }
        }
    }

    [MenuItem("Assets/Remove Scale Curves From Selected Clip", true)]
    static bool ValidateRemoveScaleFromSelected() => HasSelectedClip();

    [MenuItem("Assets/Remove Position Curves From Selected Clip")]
    static void RemovePositionFromSelected()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is AnimationClip clip)
            {
                RemoveMatchingCurves(clip, "m_LocalPosition");
            }
        }
    }

    [MenuItem("Assets/Remove Position Curves From Selected Clip", true)]
    static bool ValidateRemovePositionFromSelected() => HasSelectedClip();

    static bool HasSelectedClip()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is AnimationClip) return true;
        }
        return false;
    }

    static void RemoveMatchingCurves(AnimationClip clip, string propertyPrefix)
    {
        if (clip == null) return;

        int removed = 0;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            if (binding.propertyName.StartsWith(propertyPrefix))
            {
                AnimationUtility.SetEditorCurve(clip, binding, null);
                removed++;
            }
        }

        Debug.Log($"Removed {removed} '{propertyPrefix}' curve(s) from '{clip.name}'.");
    }
}
