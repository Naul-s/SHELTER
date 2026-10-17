using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public static class AutoAssignMapTextures
{
    private const string TARGET_FOLDER = "Assets/Map";

    [MenuItem("Tools/Auto Assign Map Textures")]
    public static void AssignTextures()
    {
        string[] materialGuids =
            AssetDatabase.FindAssets("t:Material", new[] { TARGET_FOLDER });

        string[] textureGuids =
            AssetDatabase.FindAssets("t:Texture2D", new[] { TARGET_FOLDER });

        Dictionary<string, Texture2D> textures =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        // ------------------------------------------------------------
        // Texture 목록 수집
        // ------------------------------------------------------------

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (texture == null)
                continue;

            textures[texture.name] = texture;
        }

        int materialCount = 0;
        int colorCount = 0;
        int normalCount = 0;
        int metallicCount = 0;

        List<string> notFound = new List<string>();

        // ------------------------------------------------------------
        // Material 처리
        // ------------------------------------------------------------

        foreach (string guid in materialGuids)
        {
            string materialPath =
                AssetDatabase.GUIDToAssetPath(guid);

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material == null)
                continue;

            materialCount++;

            string materialName = material.name;

            // --------------------------------------------------------
            // 현재 이름 규칙:
            //
            // 1m = Material
            // 1c = Color
            // 1n = Normal
            //
            // 2m = Material
            // 2c = Color
            // 2n = Normal
            // --------------------------------------------------------

            if (materialName.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                string baseName =
                    materialName.Substring(0, materialName.Length - 1);

                string colorName = baseName + "c";
                string normalName = baseName + "n";

                // ----------------------------------------------------
                // Color
                // ----------------------------------------------------

                if (textures.TryGetValue(colorName, out Texture2D colorTexture))
                {
                    if (material.HasProperty("_BaseMap"))
                    {
                        material.SetTexture("_BaseMap", colorTexture);
                    }
                    else if (material.HasProperty("_MainTex"))
                    {
                        // Standard Shader 대응
                        material.SetTexture("_MainTex", colorTexture);
                    }

                    colorCount++;
                }

                // ----------------------------------------------------
                // Normal
                // ----------------------------------------------------

                if (textures.TryGetValue(normalName, out Texture2D normalTexture))
                {
                    string normalPath =
                        AssetDatabase.GetAssetPath(normalTexture);

                    TextureImporter importer =
                        AssetImporter.GetAtPath(normalPath)
                        as TextureImporter;

                    if (importer != null &&
                        importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType =
                            TextureImporterType.NormalMap;

                        importer.SaveAndReimport();
                    }

                    if (material.HasProperty("_BumpMap"))
                    {
                        material.SetTexture("_BumpMap", normalTexture);
                        material.EnableKeyword("_NORMALMAP");

                        // 정상적으로 노멀맵이 보이도록
                        material.SetFloat("_BumpScale", 1f);
                    }

                    normalCount++;
                }

                // ----------------------------------------------------
                // Metallic
                //
                // 혹시 나중에 1metallic, 2metallic 같은 파일이 있으면
                // 여기서 자동 연결 가능
                // ----------------------------------------------------

                string metallicName = baseName + "metallic";

                if (textures.TryGetValue(
                    metallicName,
                    out Texture2D metallicTexture))
                {
                    if (material.HasProperty("_MetallicGlossMap"))
                    {
                        material.SetTexture(
                            "_MetallicGlossMap",
                            metallicTexture
                        );

                        material.EnableKeyword(
                            "_METALLICSPECGLOSSMAP"
                        );

                        material.SetFloat("_Metallic", 1f);

                        metallicCount++;
                    }
                }

                EditorUtility.SetDirty(material);

                // ----------------------------------------------------
                // 찾지 못한 텍스처 기록
                // ----------------------------------------------------

                bool hasColor = textures.ContainsKey(colorName);
                bool hasNormal = textures.ContainsKey(normalName);

                if (!hasColor && !hasNormal)
                {
                    notFound.Add(
                        $"{materialName} -> {colorName}, {normalName}"
                    );
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ------------------------------------------------------------
        // 결과 출력
        // ------------------------------------------------------------

        Debug.Log(
            "========================================\n" +
            "Map Texture 자동 연결 완료\n" +
            "========================================\n" +
            $"검사한 Material : {materialCount}\n" +
            $"Color 연결       : {colorCount}\n" +
            $"Normal 연결      : {normalCount}\n" +
            $"Metallic 연결    : {metallicCount}\n" +
            $"연결 실패        : {notFound.Count}\n" +
            "========================================"
        );

        if (notFound.Count > 0)
        {
            Debug.LogWarning(
                "텍스처를 찾지 못한 Material:\n" +
                string.Join("\n", notFound)
            );
        }
    }
}