using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Build instruction", menuName = "Build instruction")]
public class BuildInstructionScript : ScriptableObject
{
    public ItemsPackScript Recipe;
    public List<StageClass> Stages = new List<StageClass>();

    [Serializable]
    public class StageClass { public List<ItemScript> StageComponents = new List<ItemScript>(); }
}
