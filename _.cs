
public static class _
{
    public static bool IsNull(this object obj) => obj == null;

    public static bool IsNotNull(this object obj) => !obj.IsNull();

    public static T Get<T>(string name) where T : UnityEngine.Object => Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(found => found.name.Equals(name));

    public static IdentifiableType GetReferencedType(string referenceId) => Resources.FindObjectsOfTypeAll<IdentifiableType>().FirstOrDefault(found => found.ReferenceId.Equals(referenceId));

    public static Sprite ToSprite(this Texture2D texture)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
        sprite.hideFlags |= HideFlags.HideAndDontSave;
        sprite.name = texture.name;
        return sprite;
    }
}