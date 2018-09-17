using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SUNCGLoader { 

    public class Loader {

        private House house;

        private EmptyMono coRunner;

        public GameObject HouseToScene(House house)
        {
            coRunner = new GameObject("CoroutineRunner").AddComponent<EmptyMono>();
            this.house = house;
            GameObject root = new GameObject("House_" + house.id);
            foreach (Level level in house.levels)
            {
                LoadLevel(root, level);
            }
            return root;
        }

        private void LoadLevel(GameObject root, Level level)
        {
            GameObject levelRoot = new GameObject("Level_" + level.id);
            levelRoot.transform.parent = root.transform;
            foreach (Node n in level.nodes)
            {
                LoadNode(levelRoot, n);
            }

        }

        private void LoadNode(GameObject levelRoot, Node node)
        { 
            if (node.valid == 1)
            {
                GameObject nodeObj;
                switch (node.type)
                {
                    case "Object":
                        nodeObj = new GameObject("Node_" + node.modelId);
                        nodeObj.transform.parent = levelRoot.transform;
                        LoadNodeMesh(node, nodeObj);
                        break;
                    case "Room":
                        string[] wallsFloorCeiling = new string[] { "w", "f", "c" };
                        foreach (string extension in wallsFloorCeiling)
                        {
                            nodeObj = new GameObject("Node_" + node.modelId + extension);
                            nodeObj.transform.parent = levelRoot.transform;
                            bool loaded = LoadNodeMesh(node, nodeObj, extension);
                            //Not all 3 exists for all
                            if (!loaded)
                            {
                                GameObject.Destroy(nodeObj);
                            }
                        }
                        break;
                    case "Box":
                        break;
                    case "Ground":
                        nodeObj = new GameObject("Node_" + node.modelId + "f");
                        nodeObj.transform.parent = levelRoot.transform;
                        LoadNodeMesh(node, nodeObj, "f");
                        break;
                }
            }
        }

        //Returns true if sucessfully loaded
        private bool LoadNodeMesh(Node node, GameObject nodeObj, string modelIdAppend = "")
        {
            string pathToObj = Config.SUNCGDataPath;
            if (node.type == "Room" || node.type == "Ground")
            {
                pathToObj += "room/" + this.house.id + "/" + node.modelId + modelIdAppend + ".obj";
            }
            else if (node.type == "Object")
            {
                pathToObj += "object/" + node.modelId + "/" + node.modelId + modelIdAppend + ".obj";
            }
            if (!File.Exists(pathToObj))
            {
                return false;
            }

            MeshRenderer mr = nodeObj.AddComponent<MeshRenderer>();
            MeshFilter mf = nodeObj.AddComponent<MeshFilter>();
            Mesh m = new ObjImporter().ImportFile(pathToObj);
            mf.mesh = m;
            //Room meshes have no materials in JSON, they may have submeshes though
            if (node.materials != null) {
                if (node.materials.Length > 0)
                {
                    UnityEngine.Material[] materials = new UnityEngine.Material[node.materials.Length];
                    int i = 0;
                    foreach (Material currMat in node.materials)
                    {
                        materials[i] = LoadMaterial(currMat);
                        i++;
                    }
                    mr.materials = materials;
                }
            } else
            {
                UnityEngine.Material[] materials = new UnityEngine.Material[m.subMeshCount];
                UnityEngine.Material sharedMat = new UnityEngine.Material(Shader.Find("Standard"));
                for (int matIndex = 0; matIndex < materials.Length; matIndex++)
                {
                    materials[matIndex] = sharedMat;
                }
                mr.sharedMaterials = materials;
            }
            //Room meshes have no transform
            if (node.transform != null)
            {
                Vector4 column1 = new Vector4(node.transform[0], node.transform[1], node.transform[2], node.transform[3]);
                Vector4 column2 = new Vector4(node.transform[4], node.transform[5], node.transform[6], node.transform[7]);
                Vector4 column3 = new Vector4(node.transform[8], node.transform[9], node.transform[10], node.transform[11]);
                Vector4 column4 = new Vector4(node.transform[12], node.transform[13], node.transform[14], node.transform[15]);
                Matrix4x4 objToWorld = new Matrix4x4(column1, column2, column3, column4);
                SetTransformFromMatrix(nodeObj.transform, ref objToWorld);
            }
            return true;
        }

        private UnityEngine.Material LoadMaterial(Material suncgMat)
        {
            UnityEngine.Material mat = new UnityEngine.Material(Shader.Find("Standard"));
            mat.name = suncgMat.name;
            Color c = Color.white;
            ColorUtility.TryParseHtmlString(suncgMat.diffuse, out c);
            mat.color = c;
            if(suncgMat.texture != null)
            {
                coRunner.StartCoroutine(LoadSunCGTextureIntoMaterial(suncgMat.texture, mat));
            }
            return mat;
        }

        private IEnumerator LoadSunCGTextureIntoMaterial(string textureName, UnityEngine.Material mat)
        {
            string texturePath = Config.SUNCGDataPath + "/texture/" + textureName + ".jpg";
            using (WWW www = new WWW(texturePath))
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.DXT1, false);
                yield return www;
                www.LoadImageIntoTexture(tex);
                mat.mainTexture = tex;
            }
        }

        #region matrixUtilities
        //matrix utilities
        public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }


        public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        
        public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix(ref matrix);
            localRotation = ExtractRotationFromMatrix(ref matrix);
            localScale = ExtractScaleFromMatrix(ref matrix);
        }

        
        public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }

        public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
       
        public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

        public static Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = IdentityMatrix;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }
        #endregion
    }

}
