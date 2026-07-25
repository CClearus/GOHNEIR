using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Set Random Target Via Tag",
    story: "Set Random [Target] From [Tag]",
    category: "Action",
    id: "1cd93bb343c8d0bff30ad191eac3054c")]
public partial class SetRandomTargetFromTagAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<string> Tag;

    protected override Status OnStart()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(Tag.Value);

        if (objects.Length == 0)
            return Status.Failure;

        Target.Value = objects[UnityEngine.Random.Range(0, objects.Length)];

        return Status.Success;
    }
}