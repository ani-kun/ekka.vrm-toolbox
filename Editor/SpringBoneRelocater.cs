using System;
using UnityEditor;
using UnityEngine;
using VRM;

namespace ekka.vrm
{
    // Relocate VRM Spring Bone from secondary.
    public class SpringBoneRelocater
    {
        [MenuItem("GameObject/EKKA VRM Toolbox/Relocate Spring Bone", false, 0)]
        public static void RelocateSpringBone()
        {
            RelocateSpringBone(Selection.activeGameObject);
        }

        public static void RelocateSpringBone(GameObject rootObject)
        {
            try
            {
                if (rootObject is null)
                {
                    throw new Exception("Root object is not found.");
                }

                var secondaryObject = rootObject.transform.Find("secondary");
                if (secondaryObject is null)
                {
                    throw new Exception("secondary is not found in children.");
                }

                var springBones = secondaryObject.GetComponents<VRMSpringBone>();
                foreach (var springBone in springBones)
                {
                    var rootBones = springBone.RootBones;
                    if (rootBones.Count != 0)
                    {
                        var destination = Undo.AddComponent(rootBones[0].gameObject, springBone.GetType());
                        if (destination is null)
                        {
                            throw new Exception("Add component to destination is faild.");
                        }

                        EditorUtility.CopySerialized(springBone, destination);
                        Undo.DestroyObjectImmediate(springBone);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                EditorUtility.DisplayDialog("Error", $"Faild!{Environment.NewLine}{e.Message}", "OK");
                return;
            }

        }

    }
}
