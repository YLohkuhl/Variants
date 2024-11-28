using Il2CppInterop.Runtime;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppSamDriver.Decal;
using MelonLoader;
using UnityEngine;
using Variants.Persist;

namespace Variants
{
    [RegisterTypeInIl2Cpp]
    public class VariantAppearanceApplicator : SRBehaviour
    {
        internal Dictionary<string, Mesh> OriginalMeshes = [];
        internal Dictionary<string, Material> OriginalMaterials = [];

        public bool IsApplied { get; set; }

        public VariantAppearance Appearance { get; set; }

        public IdentifiableActor IdentifiableActor { get; set; }

        ///

        void Start()
        {
            IdentifiableActor = GetComponent<IdentifiableActor>();
            if (!IdentifiableActor)
            {
                Destroy(this);
                VariantLogger.Warning($"[{transform.gameObject.name}] This behaviour can only be applied to identifiable actors!");
                return;
            }

            FindActorVariant();

            if (Appearance.IsNotNull())
                ApplyAppearance();
        }

        void Update()
        {
            if (Appearance.IsNull() && IsApplied)
                ApplyOriginalAppearance();
        }
        
        ///

        public bool IsAppearanceApplied() => IsApplied;

        ///

        public void FindActorVariant()
        {
            long actorId = IdentifiableActor.GetActorId().Value;

            var v = VariantPersistence.Instance.Plugin.Variant;

            if (v.Actors.ContainsKey(actorId))
            {
                Appearance = VariantAppearances.Find(x => x.PersistentId.Equals(v.Actors[actorId]));

                if (Appearance.IsNull())
                    VariantLogger.Warning($"[{transform.gameObject.name}] Actor key was found but failed to find actor variant appearance. ({v.Actors[actorId]})");
            }
        }

        ///

        public void ApplyAppearance()
        {
            if (Appearance.IsNull() || Appearance.Design.IsNull())
                return;

            ApplyMeshes(Appearance.Design.Meshes);
            ApplyMaterials(Appearance.Design.Materials);
            // ApplyExpressions(Appearance.Design.Expressions);

            IsApplied = true;
            VariantPersistence.Instance.Plugin.Variant.Actors.TryAdd(IdentifiableActor.GetActorId().Value, Appearance.PersistentId);
        }

        public void ApplyOriginalAppearance()
        {
            foreach (var pair in OriginalMeshes)
            {
                if (pair.Value.IsNull())
                    continue;

                List<Transform> transforms = [];

                foreach (var x in FindTransforms(pair.Key))
                    transforms.Add(x.transform);

                ///

                foreach (var transform in transforms)
                {
                    MeshFilter filter = transform.GetComponent<MeshFilter>();
                    SkinnedMeshRenderer renderer = transform.GetComponent<SkinnedMeshRenderer>();

                    if (filter)
                        filter.mesh = pair.Value;
                    else if (renderer)
                        renderer.sharedMesh = pair.Value;
                }
            }

            ///

            foreach (var pair in OriginalMaterials)
            {
                if (pair.Value.IsNull())
                    continue;

                List<Transform> transforms = [];

                foreach (var x in FindTransforms(pair.Key))
                    transforms.Add(x.transform);

                ///

                foreach (var transform in transforms)
                {
                    Renderer renderer = transform.GetComponent<Renderer>();

                    if (renderer?.material)
                        renderer.material = pair.Value;
                }
            }

            IsApplied = false;
            Appearance = null;

            VariantPersistence.Instance.Plugin.Variant.Actors.Remove(IdentifiableActor.GetActorId().Value);
        }

        ///

        private List<Transform> FindTransforms(string name) => GetComponentsInChildren<Transform>().Where(x => x.name.Contains(name)).ToList();

        ///

        public void ApplyMeshes(VariantMesh[] vMeshes)
        {
            if (vMeshes.IsNull()) 
                return;

            foreach (var v in vMeshes)
            {
                if (v.Transform.IsNull()) 
                    continue;

                foreach (var transform in FindTransforms(v.Transform))
                {
                    MeshFilter filter = transform.GetComponent<MeshFilter>();
                    SkinnedMeshRenderer renderer = transform.GetComponent<SkinnedMeshRenderer>();

                    Mesh mesh = null;

                    if (filter?.mesh)
                    {
                        mesh = filter.mesh;
                        filter.mesh = v.Mesh;
                    }

                    else if (renderer?.sharedMesh)
                    {
                        mesh = renderer.sharedMesh;
                        renderer.sharedMesh = v.Mesh;
                    }

                    OriginalMeshes.TryAdd(transform.name, mesh);
                }
            }
        }

        ///

        public void ApplyMaterials(VariantMaterial[] vMaterials)
        {
            if (vMaterials.IsNull()) 
                return;

            var runtime = Appearance.Design.RuntimeMaterials;

            foreach (var v in vMaterials)
            {
                if (v.Transform.IsNull()) 
                    continue;

                foreach (var transform in FindTransforms(v.Transform))
                {
                    Renderer renderer = transform.GetComponent<Renderer>();
                    if (!renderer?.material) 
                        continue;

                    OriginalMaterials.TryAdd(transform.name, renderer.material);

                    ///

                    string mName = $"{v.Transform.ToLower()}.{Appearance.PersistentId.Replace("VariantAppearance.", "").ToLower()}";

                    if (runtime.ContainsKey(mName))
                    {
                        renderer.material = runtime[mName];
                        continue;
                    }

                    if (v.Premade)
                    {
                        renderer.material = v.Premade;
                        renderer.material.hideFlags |= HideFlags.HideAndDontSave;
                        renderer.material.name = mName;

                        runtime.TryAdd(mName, renderer.material);
                        continue;
                    }

                    ///

                    renderer.material = Instantiate(renderer.material);
                    renderer.material.hideFlags |= HideFlags.HideAndDontSave;
                    renderer.material.name = mName;
                    renderer.material.shader = v.Shader ?? renderer.material.shader;

                    ///

                    ApplyMaterialKeywords(renderer.material, v);
                    ApplyMaterialProperties(renderer.material, v);

                    ///

                    runtime.TryAdd(renderer.material.name, renderer.material);
                }
            }
        }

        public void ApplyMaterialKeywords(Material material, VariantMaterial vMaterial)
        {
            if (material.IsNull() || vMaterial.IsNull())
                return;

            ///

            if (vMaterial.DisabledKeywords.IsNotNull())
                foreach (var keyword in vMaterial.DisabledKeywords)
                    material.DisableKeyword(keyword);

            if (vMaterial.EnabledKeywords.IsNotNull())
                foreach (var keyword in vMaterial.EnabledKeywords)
                    material.EnableKeyword(keyword);
        }

        public void ApplyMaterialProperties(Material material, VariantMaterial vMaterial)
        {
            if (material.IsNull() || vMaterial.IsNull())
                return;

            ///

            foreach (var property in vMaterial.Properties)
            {
                switch (property.Value)
                {
                    case Color color:
                        material.SetColor(property.Key, color);
                        break;

                    case Texture texture:
                        material.SetTexture(property.Key, texture);
                        break;

                    case Vector4 vector:
                        material.SetVector(property.Key, vector);
                        break;

                    case float point:
                        material.SetFloat(property.Key, point);
                        break;

                    case int number:
                        material.SetInt(property.Key, number);
                        break;
                }
            }
        }

        ///

        /*public void ApplyExpressions(VariantExpression[] vExpressions)
        {
            if (vExpressions.IsNull())
                return;

            SlimeAppearanceApplicator applicator = GetComponent<SlimeAppearanceApplicator>();
            if (!applicator)
                return;

            var runtime = Appearance.Design.RuntimeExpressions;

            foreach (var v in vExpressions)
            {
                SlimeExpressionFace expression = applicator.Appearance.Face.GetExpressionFace(v.Expression);

                if (expression.IsNull())
                    continue;

                Material[] materials = [null, null];

                ///

                string mName = $"{expression.ToString().ToLower()}.{Appearance.PersistentId.Replace("VariantAppearance.", "")}";

                if (runtime.ContainsKey(v.Expression))
                    continue;

                if (v.Eyes.Premade || v.Mouth.Premade)
                {
                    materials[0] = v.Eyes.Premade;
                    if (materials[0])
                    {
                        materials[0].hideFlags |=  HideFlags.HideAndDontSave;
                        materials[0].name = mName;
                    }

                    materials[1] = v.Mouth.Premade;
                    if (materials[1])
                    {
                        materials[1].hideFlags |=  HideFlags.HideAndDontSave;
                        materials[1].name = mName;
                    }

                    runtime.TryAdd(v.Expression, materials);
                    continue;
                }

                ///

                if (expression.Eyes)
                {
                    materials[0] = Instantiate(expression.Eyes);
                    materials[0].hideFlags |= HideFlags.HideAndDontSave;
                    materials[0].name = mName;
                    materials[0].shader = v.Eyes.Shader ?? expression.Eyes.shader;

                    ///

                    ApplyMaterialKeywords(materials[0], v.Eyes);
                    ApplyMaterialProperties(materials[0], v.Eyes);
                }

                if (expression.Mouth)
                {
                    materials[1] = Instantiate(expression.Mouth);
                    materials[1].hideFlags |= HideFlags.HideAndDontSave;
                    materials[1].name = mName;
                    materials[1].shader = v.Mouth.Shader ?? expression.Mouth.shader;

                    ///

                    ApplyMaterialKeywords(materials[1], v.Mouth);
                    ApplyMaterialProperties(materials[1], v.Mouth);
                }

                ///

                runtime.TryAdd(v.Expression, materials);
            }

            applicator.SetExpression(SlimeFace.SlimeExpression.HAPPY);
        }*/
    }
}
