using System.Collections.Generic;

namespace foundationExport
{
    public class ExportSceneAssetsRefVO
    {
        public string sceneName;
        public Dictionary<string,List<string>> assetsMap=new Dictionary<string, List<string>>();
        public ExportPrefabRefVO exportPrefabRefVO;
        private string _saveScenePath=string.Empty;
        public string saveScenePath
        {
            get
            {
                if (_saveScenePath == string.Empty)
                {
                    string folder = "";
                    if (exportPrefabRefVO != null)
                    {
                        folder = exportPrefabRefVO.exportRootVo.name;

                        if (string.IsNullOrEmpty(folder))
                        {
                            folder = "maps";
                        }
                        _saveScenePath = folder + "/" + sceneName;
                    }
                    else
                    {
                        _saveScenePath = sceneName;
                    }
                }

                return _saveScenePath;
            }
        }

        public ExportSceneAssetsRefVO(string sceneName)
        {
            this.sceneName = sceneName;
        }
        public void add(string key, string depend)
        {
            List<string> list;
            if (assetsMap.TryGetValue(key, out list) == false)
            {
                list=new List<string>();
                assetsMap.Add(key,list);
            }

            if (list.Contains(depend))
            {
               return;
            }
            list.Add(depend);
        }
        
    }
}