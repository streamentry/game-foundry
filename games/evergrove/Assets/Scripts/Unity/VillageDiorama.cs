#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Optional Unity-only 2.5D diorama visual experiment. No prefabs, meshes,
    /// textures, external art, cloud state, model inference or asset purchases.
    /// Every visible consequence comes from a pure C# VillageScenePlan.
    ///
    /// The existing 2D illustration remains the control and safe fallback.
    /// Fixed orthographic camera + ~12fps subtle motion keep battery and
    /// motion sensitivity costs bounded.
    /// </summary>
    public sealed class VillageDiorama : MonoBehaviour
    {
        private const int SceneLayer = 29;
        private const int TextureWidth = 768;
        private const int TextureHeight = 384;
        private const float AnimationPeriod = 1f / 12f;

        private Camera sceneCamera;
        private RenderTexture output;
        private Material sharedMaterial;
        private Transform stage;
        private readonly List<Transform> villagers = new List<Transform>();
        private readonly List<Vector3> villagerOrigins = new List<Vector3>();
        private readonly List<Transform> butterflies = new List<Transform>();
        private readonly List<Vector3> butterflyOrigins = new List<Vector3>();
        private readonly List<Transform> ripples = new List<Transform>();
        private readonly List<Vector3> rippleOrigins = new List<Vector3>();
        private string displayedFingerprint;
        private bool motionEnabled;
        private bool activeVisual;
        private float lastFrame;
        private bool initialized;

        public RenderTexture Preview { get { return output; } }
        public bool Ready { get { return initialized && output != null; } }

        public bool Initialize()
        {
            if (initialized) return Ready;
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32)) return false;
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return false;

            sharedMaterial = new Material(shader);
            sharedMaterial.name = "Mosselora / restrained garden palette";
            output = new RenderTexture(TextureWidth, TextureHeight, 16, RenderTextureFormat.ARGB32);
            output.name = "Mosselora garden miniature";
            output.filterMode = FilterMode.Bilinear;
            output.wrapMode = TextureWrapMode.Clamp;
            output.antiAliasing = 1;
            if (!output.Create())
            {
                ReleaseResources();
                return false;
            }

            var cameraObj = new GameObject("Fixed orthographic preview camera");
            cameraObj.transform.SetParent(transform, false);
            sceneCamera = cameraObj.AddComponent<Camera>();
            sceneCamera.orthographic = true;
            sceneCamera.orthographicSize = 4.35f;
            sceneCamera.aspect = (float)TextureWidth / TextureHeight;
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
            sceneCamera.backgroundColor = Palette("#E5ECDD");
            sceneCamera.cullingMask = 1 << SceneLayer;
            sceneCamera.nearClipPlane = 0.1f;
            sceneCamera.farClipPlane = 50f;
            sceneCamera.targetTexture = output;
            sceneCamera.allowHDR = false;
            sceneCamera.allowMSAA = false;
            sceneCamera.enabled = false;
            sceneCamera.transform.position = new Vector3(8.9f, 9.7f, 12.0f);
            sceneCamera.transform.LookAt(new Vector3(0f, 0.3f, 0f), Vector3.up);

            initialized = true;
            return true;
        }

        private static Color Palette(string html)
        {
            ColorUtility.TryParseHtmlString(html, out Color color);
            return color;
        }

        private void Colorize(Renderer renderer, string hex)
        {
            renderer.sharedMaterial = sharedMaterial;
            var block = new MaterialPropertyBlock();
            Color color = Palette(hex);
            block.SetColor("_Color", color);
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void PutOnSceneLayer(GameObject item)
        {
            item.layer = SceneLayer;
        }

        private GameObject Primitive(PrimitiveType shape, string name, Vector3 position,
                                     Vector3 scale, string color, Transform parent = null)
        {
            GameObject obj = GameObject.CreatePrimitive(shape);
            obj.name = name;
            PutOnSceneLayer(obj);
            obj.transform.SetParent(parent == null ? stage : parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null) Colorize(renderer, color);
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            return obj;
        }

        private void Box(string name, Vector3 at, Vector3 scale, string color,
                         Transform parent = null, float rotation = 0f)
        {
            GameObject box = Primitive(PrimitiveType.Cube, name, at, scale, color, parent);
            if (Mathf.Abs(rotation) > 0.01f)
                box.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
        }

        private void Ball(string name, Vector3 at, Vector3 scale, string color,
                          Transform parent = null)
        {
            Primitive(PrimitiveType.Sphere, name, at, scale, color, parent);
        }

        private void Cylinder(string name, Vector3 at, Vector3 scale, string color,
                              Transform parent = null)
        {
            Primitive(PrimitiveType.Cylinder, name, at, scale, color, parent);
        }

        /// <summary>Thin, curved water ribbon whose banks remain predictable.</summary>
        private void Water(bool flowing)
        {
            const int segments = 16;
            var vertices = new Vector3[(segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var tris = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float z = -2.67f + t * 5.34f;
                float sway = Mathf.Sin(t * 5.0f + 0.3f) * 0.31f;
                vertices[i * 2] = new Vector3(-0.52f + sway, 0.04f, z);
                vertices[i * 2 + 1] = new Vector3(0.52f + sway, 0.04f, z);
                uvs[i * 2] = new Vector2(0f, t);
                uvs[i * 2 + 1] = new Vector2(1f, t);
                if (i == segments) continue;
                int a = i * 2, index = i * 6;
                tris[index] = a;
                tris[index + 1] = a + 2;
                tris[index + 2] = a + 1;
                tris[index + 3] = a + 1;
                tris[index + 4] = a + 2;
                tris[index + 5] = a + 3;
            }
            var stream = new GameObject("River that runs through the village");
            PutOnSceneLayer(stream);
            stream.transform.SetParent(stage, false);
            var filter = stream.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mesh.name = "Mosselora winding water";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            filter.sharedMesh = mesh;
            Colorize(stream.AddComponent<MeshRenderer>(), flowing ? "#6FB4BA" : "#C4B69C");

            if (!flowing) return;
            for (int i = 0; i < 4; i++)
            {
                float z = -1.85f + i * 1.15f;
                float t = (z + 2.67f) / 5.34f;
                float x = Mathf.Sin(t * 5f + 0.3f) * 0.31f;
                var ripple = Primitive(PrimitiveType.Cube, "Slow water glimmer " + i,
                    new Vector3(x - 0.12f, 0.062f, z), new Vector3(0.27f, 0.012f, 0.026f),
                    "#C8E5DD").transform;
                ripples.Add(ripple);
                rippleOrigins.Add(ripple.localPosition);
            }
        }

        private void Tree(Vector3 root, int age, string name)
        {
            float maturity = Mathf.Min(1.2f, 0.75f + age * 0.10f);
            var tree = new GameObject(name);
            PutOnSceneLayer(tree);
            tree.transform.SetParent(stage, false);
            tree.transform.localPosition = root;
            Cylinder("Warm bark", new Vector3(0f, 0.50f * maturity, 0f),
                new Vector3(0.13f, 0.52f * maturity, 0.13f), "#826451", tree.transform);
            Ball("Leaf canopy", new Vector3(0f, 1.28f * maturity, 0f),
                new Vector3(1.15f * maturity, 0.8f * maturity, 1.02f * maturity),
                "#5D906C", tree.transform);
            Ball("Sunlit leaves", new Vector3(-0.28f * maturity, 1.49f * maturity, -0.15f),
                new Vector3(0.78f * maturity, 0.56f * maturity, 0.74f * maturity),
                "#A2BC82", tree.transform);
            Ball("Deep leaves", new Vector3(0.30f * maturity, 1.1f * maturity, 0.15f),
                new Vector3(0.72f * maturity, 0.59f * maturity, 0.77f * maturity),
                "#5D896B", tree.transform);
        }

        private void Cottage()
        {
            Box("Cottage", new Vector3(-2.8f, 0.50f, 0.9f),
                new Vector3(1.45f, 0.95f, 1.25f), "#EAC99E");
            var roofWest = Primitive(PrimitiveType.Cube, "Sloping copper roof west",
                new Vector3(-3.16f, 1.19f, 0.9f),
                new Vector3(1.02f, 0.15f, 1.55f), "#956851");
            roofWest.transform.localRotation = Quaternion.Euler(0f, 0f, -31f);
            var roofEast = Primitive(PrimitiveType.Cube, "Sloping copper roof east",
                new Vector3(-2.43f, 1.19f, 0.9f),
                new Vector3(1.02f, 0.15f, 1.55f), "#956851");
            roofEast.transform.localRotation = Quaternion.Euler(0f, 0f, 31f);
            Box("Cottage door", new Vector3(-2.8f, 0.31f, 1.535f),
                new Vector3(0.29f, 0.62f, 0.045f), "#725848");
            Box("Cottage window", new Vector3(-3.29f, 0.72f, 1.54f),
                new Vector3(0.20f, 0.23f, 0.05f), "#E8B958");
            Box("Stone path", new Vector3(-2.8f, 0.02f, 2.2f),
                new Vector3(0.62f, 0.065f, 1.12f), "#D6CEAA");
        }

        private void Bridge(int crossings)
        {
            // Cross the river from west to east at z=-0.4.
            for (int i = 0; i < 7; i++)
                Box("Warm timber plank " + i,
                    new Vector3(-1.13f + i * 0.37f, 0.25f, -0.36f),
                    new Vector3(0.33f, 0.13f, 1.05f), "#B98B64");
            for (int side = 0; side < 2; side++)
            {
                float z = side == 0 ? -0.85f : 0.12f;
                Box("Bridge rail", new Vector3(0f, 0.72f, z),
                    new Vector3(2.62f, 0.09f, 0.09f), "#926E50");
                for (int i = 0; i < 4; i++)
                    Box("Railing support", new Vector3(-1.11f + i * 0.74f, 0.48f, z),
                        new Vector3(0.10f, 0.46f, 0.10f), "#785844");
            }
            if (crossings >= 3)
                Box("Gathering bench", new Vector3(1.9f, 0.24f, -0.75f),
                    new Vector3(0.85f, 0.22f, 0.4f), "#987254");
        }

        private void Flower(Vector3 location, string petalColor)
        {
            Cylinder("Green flower stem", new Vector3(location.x, location.y + 0.18f, location.z),
                new Vector3(0.025f, 0.18f, 0.025f), "#558467");
            Ball("Flower heart", new Vector3(location.x, location.y + 0.35f, location.z),
                new Vector3(0.12f, 0.10f, 0.12f), "#F4C767");
            for (int p = 0; p < 5; p++)
            {
                float angle = Mathf.PI * 2f * p / 5f;
                Ball("Petal", new Vector3(location.x + Mathf.Cos(angle) * 0.12f,
                    location.y + 0.36f, location.z + Mathf.Sin(angle) * 0.12f),
                    new Vector3(0.18f, 0.09f, 0.18f), petalColor);
            }
        }

        private void Butterfly(Vector3 at, string wingColor)
        {
            var group = new GameObject("Tiny butterfly");
            PutOnSceneLayer(group);
            group.transform.SetParent(stage, false);
            group.transform.localPosition = at;
            Ball("Left wing", new Vector3(-0.10f, 0f, 0f),
                new Vector3(0.22f, 0.08f, 0.17f), wingColor, group.transform);
            Ball("Right wing", new Vector3(0.10f, 0f, 0f),
                new Vector3(0.22f, 0.08f, 0.17f), wingColor, group.transform);
            Cylinder("Body", Vector3.zero,
                new Vector3(0.025f, 0.075f, 0.025f), "#544F54", group.transform);
            butterflies.Add(group.transform);
            butterflyOrigins.Add(group.transform.localPosition);
        }

        private void Villager(string name, Vector3 at, string clothes, string hair)
        {
            var group = new GameObject(name);
            PutOnSceneLayer(group);
            group.transform.SetParent(stage, false);
            group.transform.localPosition = at;
            Cylinder("Simple linen outfit", new Vector3(0f, 0.36f, 0f),
                new Vector3(0.18f, 0.34f, 0.18f), clothes, group.transform);
            Ball("Head", new Vector3(0f, 0.83f, 0f),
                new Vector3(0.37f, 0.34f, 0.36f), "#D6A480", group.transform);
            Ball("Hair", new Vector3(0f, 0.97f, -0.07f),
                new Vector3(0.40f, 0.15f, 0.36f), hair, group.transform);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Boot", new Vector3(side * 0.115f, 0.10f, 0f),
                    new Vector3(0.15f, 0.22f, 0.23f), "#5F5A52", group.transform);
                Ball("Warm hand", new Vector3(side * 0.23f, 0.45f, 0f),
                    new Vector3(0.11f, 0.12f, 0.11f), "#D6A480", group.transform);
            }
            villagers.Add(group.transform);
            villagerOrigins.Add(group.transform.localPosition);
        }

        private void Build(VillageScenePlan plan)
        {
            if (stage != null)
            {
                stage.gameObject.SetActive(false); // avoid one-frame ghosting
                ReleaseRiverMesh();
                Destroy(stage.gameObject);
            }
            var root = new GameObject("Your little village — a stateful miniature");
            PutOnSceneLayer(root);
            root.transform.SetParent(transform, false);
            stage = root.transform;
            villagers.Clear();
            villagerOrigins.Clear();
            butterflies.Clear();
            butterflyOrigins.Clear();
            ripples.Clear();
            rippleOrigins.Clear();

            // Controlled, legible diorama layers, not an asset-generated world.
            Box("Earth base", new Vector3(0f, -0.45f, 0f),
                new Vector3(8.9f, 0.64f, 5.75f), "#9D8065");
            Box("Soft grass ground", new Vector3(0f, -0.09f, 0f),
                new Vector3(8.55f, 0.11f, 5.54f), "#91B891");
            Box("Upper grass bank", new Vector3(-2.25f, -0.01f, -1.3f),
                new Vector3(3.65f, 0.09f, 2.20f), "#A0C39A");
            Box("Meadow bank", new Vector3(2.25f, -0.01f, 0.6f),
                new Vector3(3.54f, 0.09f, 2.75f), "#A5C69C");

            // Distant stones hold the silhouette without new materials.
            for (int i = 0; i < 7; i++)
                Ball("Riverbank stone", new Vector3(-3.8f + i * 1.25f, 0.02f,
                    i % 2 == 0 ? -2.50f : 2.49f),
                    new Vector3(0.33f, 0.15f, 0.36f), "#CBC7A6");

            Water(plan.WaterRestored);
            Cottage();
            if (plan.HasBridge) Bridge(plan.CrossingCount);
            else
            {
                Box("Old broken footbridge west", new Vector3(-0.79f, 0.17f, -0.36f),
                    new Vector3(0.56f, 0.10f, 0.56f), "#B79875");
                Box("Old broken footbridge east", new Vector3(0.80f, 0.17f, -0.36f),
                    new Vector3(0.56f, 0.10f, 0.56f), "#B79875");
            }

            int treeCount = Mathf.Min(6, plan.TreeCount);
            for (int i = 0; i < treeCount; i++)
            {
                Vector3 location = new Vector3(
                    i % 2 == 0 ? -2.04f - 0.38f * (i / 2) : 2.12f + 0.38f * (i / 2),
                    0f,
                    -1.40f + (i / 2) * 1.12f);
                Tree(location, plan.TreeCount, "A tree the player chose to plant " + (i + 1));
            }

            string[] petalColors = { "#E2A2B6", "#F7DE9E", "#BCB4DC", "#F2C6BA" };
            for (int i = 0; i < Mathf.Min(18, plan.FlowerCount * 3); i++)
            {
                Vector3 at = new Vector3(1.3f + (i % 6) * 0.41f, 0.01f,
                    0.64f + (i / 6) * 0.53f);
                Flower(at, petalColors[i % petalColors.Length]);
            }
            if (plan.HasButterflies)
            {
                Butterfly(new Vector3(1.95f, 1.12f, 1.17f), "#F0D18B");
                if (plan.FlowerCount >= 2)
                    Butterfly(new Vector3(2.65f, 1.40f, 1.50f), "#D8AFC8");
            }

            // Three people are always present; what they do and where they
            // gather depends on the player's history.
            Villager("Mira, who cares for trees",
                plan.NeighborsGather ? new Vector3(-1.38f, 0.08f, -0.70f) :
                    new Vector3(-2.10f, 0.08f, 1.65f), "#5E8978", "#725742");
            Villager("Orin, who restores crossings",
                plan.HasBridge ? new Vector3(1.06f, 0.08f, -0.91f) :
                    new Vector3(-1.38f, 0.08f, -1.65f), "#BC896E", "#69554E");
            Villager("Fen, who protects meadows",
                plan.FlowerCount > 0 ? new Vector3(2.55f, 0.08f, 1.92f) :
                    new Vector3(2.48f, 0.08f, -1.54f), "#888DAA", "#604B49");
            if (plan.HasVisitors)
                Villager("A visitor who arrived through the crossing",
                    new Vector3(1.14f, 0.08f, 1.12f), "#C4B483", "#9B785A");
            if (plan.LandscapePhase >= 2)
            {
                for (int i = 0; i < 4; i++)
                    Box("Worn footpath", new Vector3(-1.7f + i * 0.53f, 0.04f, 1.5f),
                        new Vector3(0.36f, 0.04f, 0.24f), "#D6CAAC");
            }
            if (plan.LandscapePhase >= 3 && plan.TreeCount > 0)
                Box("A little resting bench", new Vector3(-2.55f, 0.21f, -0.64f),
                    new Vector3(0.94f, 0.18f, 0.35f), "#937258");

            displayedFingerprint = plan.Fingerprint;
            RenderFrame();
        }

        private void RenderFrame()
        {
            if (sceneCamera == null || output == null) return;
            sceneCamera.Render();
            lastFrame = Time.unscaledTime;
        }

        public bool Show(VillageScenePlan plan, bool animate)
        {
            if (!Initialize()) return false;
            motionEnabled = animate;
            activeVisual = true;
            enabled = true;
            if (displayedFingerprint != plan.Fingerprint) Build(plan);
            else RenderFrame();
            return true;
        }

        public void Hide()
        {
            activeVisual = false;
            enabled = false;
        }

        private void Update()
        {
            if (!activeVisual || !motionEnabled || stage == null || sceneCamera == null) return;
            float now = Time.unscaledTime;
            if (now - lastFrame < AnimationPeriod) return;
            for (int i = 0; i < villagers.Count; i++)
            {
                Vector3 p = villagerOrigins[i];
                p.y += Mathf.Sin(now * 1.13f + i * 1.5f) * 0.025f;
                villagers[i].localPosition = p;
            }
            for (int i = 0; i < butterflies.Count; i++)
            {
                Vector3 p = butterflyOrigins[i];
                p.y += Mathf.Sin(now * 2.1f + i) * 0.17f;
                p.x += Mathf.Cos(now * 1.26f + i) * 0.17f;
                butterflies[i].localPosition = p;
            }
            for (int i = 0; i < ripples.Count; i++)
            {
                Vector3 p = rippleOrigins[i];
                p.z += Mathf.Sin(now * 0.8f + i * 1.8f) * 0.11f;
                ripples[i].localPosition = p;
            }
            RenderFrame();
        }

        private void ReleaseRiverMesh()
        {
            if (stage == null) return;
            // Primitive meshes are Unity-owned and must never be destroyed.
            foreach (MeshFilter filter in stage.GetComponentsInChildren<MeshFilter>(true))
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh != null && mesh.name == "Mosselora winding water")
                {
                    filter.sharedMesh = null;
                    Destroy(mesh);
                }
            }
        }

        private void ReleaseResources()
        {
            if (sceneCamera != null) sceneCamera.targetTexture = null;
            if (output != null)
            {
                output.Release();
                Destroy(output);
                output = null;
            }
            if (sharedMaterial != null)
            {
                Destroy(sharedMaterial);
                sharedMaterial = null;
            }
            if (stage != null)
            {
                ReleaseRiverMesh();
                Destroy(stage.gameObject);
                stage = null;
            }
            initialized = false;
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }
    }
}
#endif
