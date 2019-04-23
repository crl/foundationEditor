using System;
using UnityEditor;

namespace foundationEditor
{
    public enum FBXType
    {
        MODEL,
        ANIM
    }
    public class FBXInfo
    {
        public string rawFolder;
        public string fbxFolder;
        public string prefabFolder;
        public string rootFolder;
        public string fileName;

        public string keys="";

        public FBXType type;
        public ModelImporterAnimationType animationType= ModelImporterAnimationType.None;

        public FBXInfo()
        {

        }

        public string prefabPath
        {
            get { return prefabFolder + fileName + ".prefab"; }
        }

        public string fbxPath
        {
            get { return fbxFolder + fileName + ".fbx"; }
        }

        internal string getDisplayName()
        {
            if (string.IsNullOrEmpty(keys))
            {
                return fileName;
            }

            return fileName;
        }

        public override string ToString()
        {
            return this.fileName;
        }
    }
}