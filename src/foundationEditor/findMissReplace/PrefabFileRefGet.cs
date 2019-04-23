using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace foundationEditor
{
    public class PrefabFileRefGet
    {
        public string fileName;
        public string filePath;

        public List<FileRefVO> fileRefs = new List<FileRefVO>();

        public PrefabFileRefGet(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                return;
            }
            this.filePath = filePath;
            fileName = Path.GetFileName(filePath);

        }

        public void start()
        {
            if (File.Exists(filePath) == false)
            {
                return;
            }
            string content=null;

            try
            {
                using (StreamReader reader = File.OpenText(filePath))
                {
                    content = reader.ReadLine();
                    if (content != "%YAML 1.1")
                    {
                        return;
                    }
                    content = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            string[] sections = content.Split(new string[] {"---"}, StringSplitOptions.RemoveEmptyEntries);

            string[] lines;
            string key;
            foreach (string section in sections)
            {
                lines = section.Split('\n');

                if (lines.Length > 2)
                {
                    key = lines[1].Trim();
                    for (int i = 2; i < lines.Length; i++)
                    {
                        if (string.IsNullOrEmpty(lines[i]))
                        {
                            continue;
                        }
                        string v=lines[i].Trim();
                        int indexL = v.IndexOf("{");

                        if (indexL == -1)
                        {
                            continue;
                        }

                        while (true)
                        {
                            int indexR = v.IndexOf("}");
                            if (indexR == -1)
                            {
                                i++;
                                v += lines[i].Trim();
                                continue;
                            }
                            break;
                        }

                        getKeyValuePair(v, key);
                    }
                }
            }
        }

        public void replace()
        {
            string content;
            using (StreamReader reader = File.OpenText(filePath))
            {
                content = reader.ReadToEnd();
                reader.Close();
            }

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            string replaceValue=null;
            foreach (FileRefVO scriptVo in fileRefs)
            {
                if(scriptVo.isChange==false)continue;

                replaceValue = scriptVo.templeteLine.Replace("$0", scriptVo.fileID);
                replaceValue = replaceValue.Replace("$1", scriptVo.guid);

                content = content.Replace(scriptVo.lineValue, replaceValue);
            }

            using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                Byte[] info = Encoding.ASCII.GetBytes(content);
                fileStream.Write(info, 0, info.Length);
                fileStream.Close();
            }
        }


        public static PrefabFileRefGet get(string filePath)
        {
            PrefabFileRefGet a= new PrefabFileRefGet(filePath);
            a.start();
            return a;
        }

        private void getKeyValuePair(string item,string header)
        {
            int indexL = item.IndexOf("{");
            int indexR = item.IndexOf("}");

            if (indexL == -1 || indexR == -1)
            {
                return;
            }
            string context = item.Substring(indexL + 1, indexR - indexL - 1);
            if (string.IsNullOrEmpty(context))
            {
                return;
            }

            string[] propertys = context.Split(',');
            string[] keyValues;

            string key;
            string value;
            Dictionary<string, string> keyValuePair = new Dictionary<string, string>();
            foreach (string property in propertys)
            {
                if (string.IsNullOrEmpty(property))
                {
                    continue;
                }

                keyValues = property.Split(':');

                if (keyValues.Length != 2)
                {
                    continue;
                }

                key = keyValues[0].Trim();
                value = keyValues[1].Trim();
                keyValuePair.Add(key, value);
            }

            if (keyValuePair.ContainsKey("guid") && keyValuePair.ContainsKey("fileID"))
            {
                FileRefVO vo = new FileRefVO();
                vo.guid = keyValuePair["guid"];
                vo.fileID = keyValuePair["fileID"];
                vo.type = int.Parse(keyValuePair["type"]);
                vo.lineValue = item;

                item = item.Replace(vo.guid, "$1");
                vo.templeteLine = item.Replace(vo.fileID, "$0");

                vo.guidPath = AssetDatabase.GUIDToAssetPath(vo.guid);
                if (item.StartsWith("m_Script:"))
                {
                    if (vo.guidPath.EndsWith(".dll"))
                    {
                        FileIDUtil.getAllFileIDByDll(vo.guidPath);
                        vo.fileIDName = FileIDUtil.getFileNameByFileID(int.Parse(vo.fileID));
                    }
                    vo.isMonoScript = true;
                }
                fileRefs.Add(vo);
            }
        }
    }


    public class FileRefVO
    {
        public string lineValue;
        public string templeteLine;
        public string guid;

        public string fileID;

        public int type;

        public string guidPath;
        public string fileIDName;

        public bool isMonoScript = false;
        public bool isChange = false;
    }
}
