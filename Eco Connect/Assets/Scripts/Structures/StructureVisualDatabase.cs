using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StructureVisualDatabase",
    menuName = "EcoConnect/Structure Visual Database"
)]
public class StructureVisualDatabase : ScriptableObject
{
    [System.Serializable]
    public class StructureVisualEntry
    {
        public StructureType structureType;
        public List<Sprite> sprites = new List<Sprite>();
    }

    [SerializeField]
    private List<StructureVisualEntry> entries =
        new List<StructureVisualEntry>();

    public Sprite GetSprite(
        StructureType structureType,
        int variantIndex)
    {
        foreach (StructureVisualEntry entry in entries)
        {
            if (entry.structureType != structureType)
                continue;

            if (entry.sprites == null ||
                entry.sprites.Count == 0)
                return null;

            int index = Mathf.Clamp(
                variantIndex,
                0,
                entry.sprites.Count - 1
            );

            return entry.sprites[index];
        }

        return null;
    }
}

