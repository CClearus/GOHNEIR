using UnityEditor;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;

[CustomPropertyDrawer(typeof(NavMeshAgentTypeAttribute))]
public class NavMeshAgentTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        int count = NavMesh.GetSettingsCount();
        var names = new string[count];
        var ids = new int[count];
        int selected = 0;

        for (int i = 0; i < count; i++)
        {
            int id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            names[i] = NavMesh.GetSettingsNameFromID(id);
            ids[i] = id;
            if (id == property.intValue) selected = i;
        }

        EditorGUI.BeginProperty(position, label, property);
        int newSelected = EditorGUI.Popup(position, label.text, selected, names);
        if (count > 0) property.intValue = ids[newSelected];
        EditorGUI.EndProperty();
    }
}
