using Assimp;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Reflection.Emit;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ExportMeshesBetter))]
    [HarmonyPatch(typeof(ModelExporter), nameof(ModelExporter.ProcessMesh))]
    internal sealed class ExportMeshesBetter : ResoniteBugFixMonkey<ExportMeshesBetter>
    {

        public override IEnumerable<string> Authors => Contributors.Onan;
        
        public static bool Prefix(Assimp.Mesh assimpMesh, FrooxEngine.Mesh engineMesh, int materialIndex)
        {
            Logger.Info(() => $"Running bone fix to mesh data for assimp export!");
            MeshX meshx = engineMesh.Data;
            if (meshx.HasBoneBindings)
            {

                List<List<VertexWeight>> vertexWeights = new List<List<VertexWeight>>();
                for (int i = 0; i < meshx.BoneCount; i++)
                {
                    vertexWeights.Add(new List<VertexWeight>());
                }

                for (int i = 0; i < meshx.VertexCount; i++)
                {
                    //Logger.Info(() => (meshx.GetVertex(i).BoneBinding.boneIndex0).ToString());
                    if (meshx.GetVertex(i).BoneBinding.boneIndex0 >= 0)
                    {
                        vertexWeights[meshx.GetVertex(i).BoneBinding.boneIndex0].Add(new VertexWeight(i, meshx.GetVertex(i).BoneBinding.weight0));
                    }
                    if (meshx.GetVertex(i).BoneBinding.boneIndex1 >= 0)
                    {
                        vertexWeights[meshx.GetVertex(i).BoneBinding.boneIndex1].Add(new VertexWeight(i, meshx.GetVertex(i).BoneBinding.weight1));
                    }
                    if (meshx.GetVertex(i).BoneBinding.boneIndex2 >= 0)
                    {
                        vertexWeights[meshx.GetVertex(i).BoneBinding.boneIndex2].Add(new VertexWeight(i, meshx.GetVertex(i).BoneBinding.weight2));
                    }
                    if (meshx.GetVertex(i).BoneBinding.boneIndex3 >= 0)
                    {
                        vertexWeights[meshx.GetVertex(i).BoneBinding.boneIndex3].Add(new VertexWeight(i, meshx.GetVertex(i).BoneBinding.weight3));
                    }
                }

                //VertexWeight vertexWeight = new VertexWeight();
                for (int i = 0; i < meshx.BoneCount; i++)
                {

                    Matrix4x4 bindPose = meshx.bones[i].BindPose.ToAssimp();
                    //bindPose.Inverse();
                    //Matrix4x4 Local = AssimpExtensions.GetLocalToGlobal(meshx.bones[i].Node, meshx.bones[i].Mesh.RawBoneBindings[0]. .World);

                    assimpMesh.Bones.Add(new Assimp.Bone(meshx.bones[i].Name, bindPose, vertexWeights[i].ToArray()));
                }
                Logger.Info(() => ("Finished bone patch code!"));
            }
            else
            {
                Logger.Info(() => "Nevermind on Running bone fix to mesh data...");
            }
            return true;

        }


        private static Matrix4x4 GetGlobalToLocal(Node node)
        {
            if (node == null)
            {
                return Matrix4x4.Identity;
            }
            Matrix4x4 matrix = node.Transform;
            while (node.Parent != null)
            {
                node = node.Parent;
                matrix = node.Transform * matrix;
            }
            return matrix;
        }

    }
}