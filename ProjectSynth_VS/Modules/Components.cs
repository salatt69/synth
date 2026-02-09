using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ProjectSynth.Modules
{
    public static class Components
    {
        public static T AddOrGet<T>(GameObject go) where T : Component
        {
            return go.GetComponent<T>() ?? go.AddComponent<T>();
        }

        public static T AddOrGetAndCopy<T>(GameObject dst, GameObject src)
            where T : Component
        {
            var srcComp = src.GetComponent<T>();
            if (!srcComp)
                throw new Exception($"Source missing component {typeof(T).Name}");

            var dstComp = AddOrGet<T>(dst);
            CopySerializedFields(srcComp, dstComp);
            return dstComp;
        }

        public static void CopySerializedFields(Component src, Component dst)
        {
            if (!src || !dst) return;

            var type = src.GetType();
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var field in type.GetFields(flags))
            {
                if (field.IsStatic || field.IsInitOnly || field.IsNotSerialized)
                    continue;

                bool isPublic = field.IsPublic;
                bool hasSerialize = field.GetCustomAttribute<SerializeField>() != null;

                if (!isPublic && !hasSerialize)
                    continue;

                try
                {
                    field.SetValue(dst, field.GetValue(src));
                }
                catch
                {
                    // some Unity internals can fail safely
                }
            }
        }
    }
}
