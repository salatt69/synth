using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Modules
{
    public static class TextureDump
    {
        public static void DumpAddressablePngToDesktop(string address, string fileName = null)
        {
            var src = Addressables.LoadAssetAsync<Texture2D>(address).WaitForCompletion();
            if (!src) throw new Exception($"Failed to load Texture2D: {address}");

            // GPU -> CPU readable copy
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(src, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var readable = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readable.Apply(false, false);

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var outName = string.IsNullOrEmpty(fileName) ? (src.name + ".png") : fileName;
            var path = Path.Combine(desktop, outName);

            File.WriteAllBytes(path, readable.EncodeToPNG());
            UnityEngine.Object.Destroy(readable);

            Debug.Log($"[TextureDump] Saved: {path}");
        }
    }
}
