using System.IO;
using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class ProjectAutoCopy
    {
        [MenuItem("Tools/ProjectAutoCopy")]
        static void autoCopy()
        {
            ASDictionary<string, string>  dic=EditorConfigUtils.GetAutoCopys();
            int count = 0;
            foreach (string fromPath in dic.Keys)
            {
                string fPath = fromPath;
                if (Directory.Exists(fPath))
                {
                    continue;
                }
                string tPath = dic[fromPath];
                if (Directory.Exists(tPath))
                {
                    continue;
                }
                FileHelper.CopyDirectory(fPath, tPath);
                count++;
            }
            Debug.Log("auto copy directory count:" + count);
        }
    }
}