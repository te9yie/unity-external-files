using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class Game : MonoBehaviour
{
    [SerializeField] private SpriteRenderer actor = default;
    [SerializeField] private SpriteRenderer frame = default;

    void Start()
    {
        Assert.IsNotNull(actor);
        Assert.IsNotNull(frame);
        var modDir = Path.Combine(Application.dataPath, "Mods");
        var mod = ReadModInfo(Path.Combine(modDir, "mod.json"));
        if (mod != null)
        {
            var actorTex = ReadTexture(Path.Combine(modDir, mod.actor.path));
            if (actorTex != null)
            {
                var rect = new Rect(0, 0, actorTex.width, actorTex.height);
                var sprite = Sprite.Create(actorTex, rect, new Vector2(0.5f, 0.5f), 100);
                actor.sprite = sprite;
            }
            var frameTex = ReadTexture(Path.Combine(modDir, mod.frame.path));
            if (frameTex != null)
            {
                var rect = new Rect(0, 0, frameTex.width, frameTex.height);
                var sprite = Sprite.Create(frameTex, rect, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, mod.frame.border);
                frame.sprite = sprite;
            }
        }
    }

    private ModInfo ReadModInfo(string path)
    {
        try
        {
            using (var stream = new FileStream(path, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonUtility.FromJson<ModInfo>(json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    private Texture2D ReadTexture(string path)
    {
        try
        {
            using (var stream = new FileStream(path, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var bin = reader.ReadBytes((int)reader.BaseStream.Length);
                var width = ParseBigEndianInt32(bin, 16);
                var height = ParseBigEndianInt32(bin, 20);
                var texture = new Texture2D(width, height);
                texture.LoadImage(bin);
                return texture;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    private int ParseBigEndianInt32(byte[] source, int offset)
    {
        var bin = new byte[4];
        Array.Copy(source, offset, bin, 0, 4);
        Array.Reverse(bin);
        return BitConverter.ToInt32(bin, 0);
    }
}

[Serializable]
class SpriteInfo
{
    public string path = default;
    public Vector4 border = default;
}

[Serializable]
class ModInfo
{
    public SpriteInfo frame = default;
    public SpriteInfo actor = default;
}
