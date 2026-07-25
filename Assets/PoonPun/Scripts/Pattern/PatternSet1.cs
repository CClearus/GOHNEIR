using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSet1", menuName = "Scriptable Objects/PatternSet1")]
public class PatternSet1 : ScriptableObject
{
    [SerializeField] private List<BulletPattern> patterns = new List<BulletPattern>();
}

[System.Serializable]
public class BulletPattern
{
    public string id;
    public List<BulletData> bullets = new List<BulletData>();
}

[System.Serializable]
public class BulletData
{
    public Vector3 position;
    public float damage;
}
