using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRM;

namespace ekka.vrm
{
    public class BlendShapeToAnimationClip
    {
        [MenuItem("GameObject/EKKA VRM Toolbox/BlendShape To AnimationClip", false, 0)]
        public static void ConvertToAnimationClip()
        {
            ConvertToAnimationClip(Selection.activeGameObject);
        }

        public static void ConvertToAnimationClip(GameObject rootObject)
        {
            try
            {
                if (rootObject is null)
                {
                    throw new Exception("Root object is not found.");
                }

                var skinnedMeshComponents = rootObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshComponents is null || skinnedMeshComponents.Length <= 0)
                {
                    EditorUtility.DisplayDialog("Error", "No Skinned Mesh Renderer.", "OK");
                    return;
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

                var folderPath = Path.GetRelativePath(Application.dataPath, EditorUtility.SaveFolderPanel("Save", "Assets", ""));
                if (folderPath.Length != 0)
                {
                    foreach (var blendShapeClip in blendShapeAvatar.Clips)
                    {
                        var animationClip = new AnimationClip();

                        foreach (var blendShapeBinding in blendShapeClip.Values)
                        {
                            var skinnedMeshComponent = skinnedMeshComponents
                                .Where(x => x.gameObject.name == blendShapeBinding.RelativePath)
                                .First(x => x);

                            var propertyName = "blendShape." + skinnedMeshComponent.sharedMesh.GetBlendShapeName(blendShapeBinding.Index);
                            var curveBinding = EditorCurveBinding.FloatCurve(blendShapeBinding.RelativePath, typeof(SkinnedMeshRenderer), propertyName);
                            var curve = new AnimationCurve(new Keyframe(0f, blendShapeBinding.Weight));

                            AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
                        }

                        if (!animationClip.empty)
                        {
                            var filePath = "Assets/" + folderPath + "/" + rootObject.name + "_" + blendShapeClip.BlendShapeName + ".anim";
                            if (string.IsNullOrEmpty(filePath))
                            {
                                return;
                            }
                            else
                            {

                                AssetDatabase.CreateAsset(animationClip,  filePath);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }
                        }
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
