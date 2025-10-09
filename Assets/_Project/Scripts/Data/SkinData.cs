using UnityEngine;

[System.Serializable]
public class SkinData 
{
    public string skinId;
    public string skinName;
    public string description;
    public Sprite skinIcon;
    public GameObject skinPrefab;
    public float priceInSolana;
    public bool isOwned;
    public bool isEquipped;
    public SkinCategory category;
    public SkinRarity rarity;
}

[System.Serializable]
public enum SkinCategory 
{
    Man,
    Woman,
    Animal,
    Farm
}

[System.Serializable]
public enum SkinRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "Game/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    public SkinData[] skins;

    public SkinData GetSkinById(string skinId)
    {
        foreach (var skin in skins)
        {
            if (skin.skinId == skinId)
                return skin;
        }
        return null;
    }

    public SkinData[] GetSkinsByCategory(SkinCategory category)
    {
        return System.Array.FindAll(skins, skin => skin.category == category);
    }
}


