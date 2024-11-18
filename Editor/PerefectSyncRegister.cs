using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRM;

namespace ekka.vrm
{
    public class PerefectSyncRegister
    {
        private static readonly string[] ARFaceAnchorBlendShapes = {
            "eyeBlinkLeft",
            "eyeLookDownLeft",
            "eyeLookInLeft",
            "eyeLookOutLeft",
            "eyeLookUpLeft",
            "eyeSquintLeft",
            "eyeWideLeft",
            "eyeBlinkRight",
            "eyeLookDownRight",
            "eyeLookInRight",
            "eyeLookOutRight",
            "eyeLookUpRight",
            "eyeSquintRight",
            "eyeWideRight",
            "jawForward",
            "jawLeft",
            "jawRight",
            "jawOpen",
            "mouthClose",
            "mouthFunnel",
            "mouthPucker",
            "mouthLeft",
            "mouthRight",
            "mouthSmileLeft",
            "mouthSmileRight",
            "mouthFrownLeft",
            "mouthFrownRight",
            "mouthDimpleLeft",
            "mouthDimpleRight",
            "mouthStretchLeft",
            "mouthStretchRight",
            "mouthRollLower",
            "mouthRollUpper",
            "mouthShrugLower",
            "mouthShrugUpper",
            "mouthPressLeft",
            "mouthPressRight",
            "mouthLowerDownLeft",
            "mouthLowerDownRight",
            "mouthUpperUpLeft",
            "mouthUpperUpRight",
            "browDownLeft",
            "browDownRight",
            "browInnerUp",
            "browOuterUpLeft",
            "browOuterUpRight",
            "cheekPuff",
            "cheekSquintLeft",
            "cheekSquintRight",
            "noseSneerLeft",
            "noseSneerRight",
            "tongueOut",
        };

        [MenuItem("GameObject/EKKA VRM Toolbox/PerefectSync Register", false, 0)]
        public static void RegistPerefectSync()
        {
            RegistPerefectSync(Selection.activeGameObject);
        }

        public static void RegistPerefectSync(GameObject rootObject)
        {
            try
            {
                if (rootObject is null)
                {
                    throw new Exception("Root object is not found.");
                }

                var vrmBlendShapeProxy = rootObject.GetComponent<VRMBlendShapeProxy>();
                if (vrmBlendShapeProxy is null)
                {
                    throw new Exception("VRMBlendShapeProxy is not found.");
                }

                var blendShapeAvatar = vrmBlendShapeProxy.BlendShapeAvatar;
                if (blendShapeAvatar is null)
                {
                    throw new Exception("BlendShapeAvatar is not found.");
                }

                var blendShapeAvatarDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(blendShapeAvatar));

                var skinnedMeshComponents = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                bool ignoreAllFlag = false;

                foreach (string ARFaceAnchorBlendShapeName in ARFaceAnchorBlendShapes)
                {
                    var blendShapeClipName = ARFaceAnchorBlendShapeName[0].ToString().ToUpper() + ARFaceAnchorBlendShapeName.Substring(1);

                    var blendShapeClip = ScriptableObject.CreateInstance("BlendShapeClip") as BlendShapeClip;

                    var blendShapeClipAlready = blendShapeAvatar.Clips.Find(n => n.BlendShapeName == blendShapeClipName);
                    if (blendShapeClipAlready is not null)
                    {
                        var blendShapeClipAssetPathAlready = AssetDatabase.GetAssetPath(blendShapeClipAlready);
                        blendShapeClip = File.Exists(blendShapeClipAssetPathAlready)
                            ? AssetDatabase.LoadAssetAtPath<BlendShapeClip>(blendShapeClipAssetPathAlready)
                            : throw new Exception("BlendShapeClip already exists, but asset file is not found");
                    }

                    blendShapeClip.BlendShapeName = blendShapeClipName;

                    var blendShapeBindings = new List<BlendShapeBinding> { };
                    foreach (var meshComponent in skinnedMeshComponents)
                    {
                        for (var index = 0; index < meshComponent.sharedMesh.blendShapeCount; index++)
                        {
                            if(ARFaceAnchorBlendShapeName == meshComponent.sharedMesh.GetBlendShapeName(index))
                            {
                                var blendShapeBinding = new BlendShapeBinding
                                {
                                    RelativePath = AnimationUtility.CalculateTransformPath(meshComponent.transform, rootObject.transform),
                                    Index = index,
                                    Weight = 100.0f
                                };
                                blendShapeBindings.Add(blendShapeBinding);
                            }
                        }
                    }

                    if (blendShapeBindings.Count == 0)
                    {
                        if (!ignoreAllFlag)
                        {
                            var chosen = EditorUtility.DisplayDialogComplex("Infomation", $"{ARFaceAnchorBlendShapeName} is not found.", "Ignore", "Ignore All", "Cancel");
                            switch (chosen)
                            {
                                case 0:
                                    Debug.LogWarning($"{ARFaceAnchorBlendShapeName} is not found : Ignored.");
                                    break;
                                case 1:
                                    ignoreAllFlag = true;
                                    break;
                                case 2:
                                    throw new Exception($"{ARFaceAnchorBlendShapeName} is not found.");
                                default:
                                    break;
                            }
                        }

                        if (ignoreAllFlag)
                        {
                            Debug.LogWarning($"{ARFaceAnchorBlendShapeName} is not found : Ignored All.");
                        }
                    }

                    blendShapeClip.Values = blendShapeBindings.ToArray();

                    if (blendShapeClipAlready is null)
                    {
                        AssetDatabase.CreateAsset(blendShapeClip, $"{blendShapeAvatarDirectory}\\PerfectSync.{blendShapeClipName}.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        blendShapeAvatar.SetClip(BlendShapeKey.CreateUnknown(blendShapeClipName), blendShapeClip);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                EditorUtility.DisplayDialog("Error", $"Faild!{Environment.NewLine}{e.Message}", "OK");
                return;
            }

            EditorUtility.DisplayDialog("Infomation", "Complete.", "OK");
        }
    }
}
