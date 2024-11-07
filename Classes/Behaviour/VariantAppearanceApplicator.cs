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
            ApplyExpressions(Appearance.Design.Expressions);

            IsApplied = true;
            VariantPersistence.Instance.Plugin.Variant.Actors.TryAdd(IdentifiableActor.GetActorId().Value, Appearance.PersistentId);
        }

        public void ApplyOriginalAppearance()
        {
            SlimeAppearanceApplicator applicator = GetComponent<SlimeAppearanceApplicator>();

            if (applicator)
            {
                applicator.ApplyAppearance();

                IsApplied = false;
                Appearance = null;

                VariantPersistence.Instance.Plugin.Variant.Actors.Remove(IdentifiableActor.GetActorId().Value);
                return;
            }

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

            if (vMaterial.EnabledKeywords.IsNotNull())
                foreach (var keyword in vMaterial.EnabledKeywords)
                    material.EnableKeyword(keyword);

            if (vMaterial.DisabledKeywords.IsNotNull())
                foreach (var keyword in vMaterial.DisabledKeywords)
                    material.DisableKeyword(keyword);
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

        public void ApplyExpressions(VariantExpression[] vExpressions)
        {
            if (vExpressions.IsNull())
                return;

            SlimeAppearanceApplicator applicator = GetComponent<SlimeAppearanceApplicator>();
            if (!applicator)
                return;

            var runtime = Appearance.Design.RuntimeExpressions;

            foreach (var v in vExpressions)
            {
                SlimeExpressionFace expression = null;

                if (applicator.Appearance.Face._expressionToFaceLookup.ContainsKey(v.Expression))
                    expression = applicator.Appearance.Face._expressionToFaceLookup[v.Expression];

                if (expression.IsNull())
                    continue;

                foreach (var renderer in applicator._faceRenderers)
                {
                    if (renderer?.Renderer?.sharedMaterials?.Count < 1)
                        continue;

                    Material[] materials = [null, null];

                    ///

                    string mName = $"{expression.ToString().ToLower()}.{Appearance.PersistentId.Replace("VariantAppearance.", "")}";

                    if (runtime.ContainsKey(v.Expression))
                    {
                        materials[0] = Appearance.Design.RuntimeExpressions[v.Expression][0];
                        materials[1] = Appearance.Design.RuntimeExpressions[v.Expression][1];
                        continue;
                    }

                    if (v.Eyes.Premade || v.Mouth.Premade)
                    {
                        materials[0] = v.Eyes.Premade;
                        if (materials[0])
                        {
                            materials[0].hideFlags |=  HideFlags.HideAndDontSave;
                            materials[0].name = mName;
                        }

                        materials[1] = v.Eyes.Premade;
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

                    renderer.Renderer.sharedMaterials = materials;
                    runtime.TryAdd(v.Expression, materials);
                }
            }
        }

        /////

        //public void ApplyAppearance()
        //{
        //    if (Appearance.IsNull())
        //        return;

        //    ///

        //    VariantAppearance.VariantDesign design = Appearance.Design;

        //    ///

        //    if (design.Meshes.IsNotNull())
        //    {
        //        foreach (var v in design.Meshes)
        //        {
        //            if (v.Transform.IsNull())
        //                continue;

        //            List<Transform> transforms = [];

        //            foreach (var x in GetComponentsInChildren<Transform>())
        //                if (x.name.Contains(v.Transform))
        //                    transforms.Add(x.transform);

        //            ///

        //            foreach (var transform in transforms)
        //            {
        //                if (transform.IsNull())
        //                    continue;

        //                MeshFilter filter = transform.GetComponent<MeshFilter>();
        //                SkinnedMeshRenderer renderer = transform.GetComponent<SkinnedMeshRenderer>();

        //                Mesh mesh = null;

        //                ///

        //                if (filter?.mesh)
        //                {
        //                    mesh = filter.mesh;
        //                    filter.mesh = v.Mesh;
        //                }
        //                else if (renderer?.sharedMesh)
        //                {
        //                    mesh = renderer.sharedMesh;
        //                    renderer.sharedMesh = v.Mesh;
        //                }

        //                ///

        //                OriginalMeshes.TryAdd(transform.name, mesh);
        //            }
        //        }
        //    }

        //    ///

        //    int i = 0;
        //    Dictionary<string, Material> materials = [];

        //    if (design.Materials.IsNotNull())
        //    {
        //        foreach (var v in design.Materials)
        //        {
        //            if (v.Transform.IsNull())
        //                continue;

        //            List<Transform> transforms = [];

        //            foreach (var x in GetComponentsInChildren<Transform>())
        //                if (x.name.Contains(v.Transform))
        //                    transforms.Add(x.transform);

        //            ///

        //            foreach (var transform in transforms)
        //            {
        //                if (transform.IsNull())
        //                    continue;

        //                Renderer renderer = transform.GetComponent<Renderer>();

        //                if (!renderer?.material)
        //                    continue;

        //                OriginalMaterials.TryAdd(transform.name, renderer.material);

        //                ///

        //                string mName = $"{v.Transform.ToLower()}.{Appearance.PersistentId.Replace("VariantAppearance.", "").ToLower()}";

        //                if (v.Premade.IsNotNull())
        //                {
        //                    renderer.material = v.Premade;
        //                    renderer.material.hideFlags |= HideFlags.HideAndDontSave;
        //                    renderer.material.name = mName;

        //                    materials.TryAdd(mName, renderer.material);
        //                    continue;
        //                }

        //                if (Appearance.Design.RuntimeMaterials?.Length > 0)
        //                {
        //                    renderer.material = Appearance.Design.RuntimeMaterials[i];
        //                    continue;
        //                }

        //                ///

        //                // this is different from runtime materials but for stuff with LODs and such
        //                if (materials.ContainsKey(mName))
        //                {
        //                    renderer.material = materials.FirstOrDefault(x => x.Key.Equals(mName)).Value;
        //                    continue;
        //                }

        //                renderer.material = Instantiate(renderer.material);
        //                renderer.material.hideFlags |= HideFlags.HideAndDontSave;
        //                renderer.material.name = mName;
        //                renderer.material.shader = v.Shader ?? renderer.material.shader;

        //                ///

        //                if (v.DisabledKeywords.IsNotNull())
        //                    foreach (var keyword in v.DisabledKeywords)
        //                        renderer.material.DisableKeyword(keyword);

        //                if (v.EnabledKeywords.IsNotNull())
        //                    foreach (var keyword in v.EnabledKeywords)
        //                        renderer.material.EnableKeyword(keyword);

        //                ///

        //                foreach (var property in v.Properties)
        //                {
        //                    switch (property.Value)
        //                    {
        //                        case Color color:
        //                            renderer.material.SetColor(property.Key, color);
        //                            break;

        //                        case Texture texture:
        //                            renderer.material.SetTexture(property.Key, texture);
        //                            break;

        //                        case Vector4 vector:
        //                            renderer.material.SetVector(property.Key, vector);
        //                            break;

        //                        case float point:
        //                            renderer.material.SetFloat(property.Key, point);
        //                            break;

        //                        case int number:
        //                            renderer.material.SetInt(property.Key, number);
        //                            break;
        //                    }
        //                }

        //                ///

        //                materials.TryAdd(renderer.material.name, renderer.material);
        //            }

        //            ///

        //            i++;
        //        }
        //    }

        //    ///

        //    Dictionary<SlimeFace.SlimeExpression, Material[]> faceMaterials = [];

        //    if (design.Expressions.IsNotNull())
        //    {
        //        SlimeAppearanceApplicator applicator = GetComponent<SlimeAppearanceApplicator>();
        //        if (!applicator)
        //            return;

        //        foreach (var v in design.Expressions)
        //        {
        //            SlimeExpressionFace expression = null;

        //            if (applicator.Appearance.Face._expressionToFaceLookup.ContainsKey(v.Expression))
        //                expression = applicator.Appearance.Face._expressionToFaceLookup[v.Expression];

        //            if (expression.IsNull())
        //                continue;

        //            ///

        //            string mName = $"{v.Expression.ToString().ToLower()}.{Appearance.PersistentId.Replace("VariantAppearance.", "")}";

        //            if (expression.Eyes)
        //            {
        //                if (v.Eyes.Premade.IsNotNull())
        //                {
        //                    expression.Eyes = v.Eyes.Premade;
        //                    expression.Eyes.hideFlags |=  HideFlags.HideAndDontSave;
        //                    expression.Eyes.name = mName;
        //                }

        //                if (Appearance.Design.RuntimeMaterials?.Length > 0)
        //                {
        //                    expression.Eyes = Appearance.Design.RuntimeExpressions[v.Expression][0];
        //                    continue;
        //                }


        //                ///

        //                expression.Eyes = Instantiate(expression.Eyes);
        //                expression.Eyes.hideFlags |= HideFlags.HideAndDontSave;
        //                expression.Eyes.name = mName;
        //                expression.Eyes.shader = v.Eyes.Shader ?? expression.Eyes.shader;

        //                ///

        //                if (v.Eyes.DisabledKeywords.IsNotNull())
        //                    foreach (var keyword in v.Eyes.DisabledKeywords)
        //                        expression.Eyes.DisableKeyword(keyword);

        //                if (v.Eyes.EnabledKeywords.IsNotNull())
        //                    foreach (var keyword in v.Eyes.EnabledKeywords)
        //                        expression.Eyes.EnableKeyword(keyword);

        //                ///

        //                foreach (var property in v.Eyes.Properties)
        //                {
        //                    switch (property.Value)
        //                    {
        //                        case Color color:
        //                            expression.Eyes.SetColor(property.Key, color);
        //                            break;

        //                        case Texture texture:
        //                            expression.Eyes.SetTexture(property.Key, texture);
        //                            break;

        //                        case Vector4 vector:
        //                            expression.Eyes.SetVector(property.Key, vector);
        //                            break;

        //                        case float point:
        //                            expression.Eyes.SetFloat(property.Key, point);
        //                            break;

        //                        case int number:
        //                            expression.Eyes.SetInt(property.Key, number);
        //                            break;
        //                    }
        //                }

        //                ///

        //                faceMaterials.TryAdd(v.Expression, [expression.Eyes, expression.Mouth]);
        //            }
        //        }
        //    }

        //    ///

        //    if (Appearance.Design.RuntimeMaterials.IsNull() || Appearance.Design.RuntimeMaterials.Length < 1)
        //        Appearance.Design.RuntimeMaterials = [.. materials.Values];

        //    IsApplied = true;

        //    ///

        //    VariantPersistence.Instance.Plugin.Variant.Actors.TryAdd(IdentifiableActor.GetActorId().Value, Appearance.PersistentId);
        //}
    }
}
