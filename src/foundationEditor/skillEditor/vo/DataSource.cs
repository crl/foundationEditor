using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;

namespace foundationEditor
{
    public class DataSource
    {
        public static Dictionary<string, List<string>> dataSource = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<ResourceVO>> resourceVOSource = new Dictionary<string, List<ResourceVO>>();
        public const string ANIMATION = "animation";
        public const string ANIMATION_PARMS = "animationParms";
        public const string BONE = "bone";
        public const string AVATAR = "avatar";
        public const string EFFECT = "effect";
        public const string MAP = "map";
        public const string SOUND="sound";

        public static void Add(string key, List<string> list,bool replace=false)
        {
            if (dataSource.ContainsKey(key) == false)
            {
                dataSource.Add(key,list);
            }else if (replace)
            {
                dataSource[key] = list;
            }
        }

        public static void Add(string key, List<ResourceVO> list, bool replace = false)
        {
            if (resourceVOSource.ContainsKey(key) == false)
            {
                resourceVOSource.Add(key, list);
            }
            else if (replace)
            {
                resourceVOSource[key] = list;
            }
        }

        public static GenericMenu GetGenericMenu(string key, Action<object> callBack)
        {
            List<string> list = null;
            if (dataSource.TryGetValue(key, out list))
            {
                //GenericMenu menu = new GenericMenu();
            }

            return null;
        }

        public static List<string> Get(string key)
        {
            List<string> list = null;
            dataSource.TryGetValue(key, out list);
            return list;
        }


        public static ResourceVO GetResourceVO(string key,string fileName)
        {
            List<ResourceVO> list = null;
            if (resourceVOSource.TryGetValue(key, out list) == false)
            {
                return null;
            }

            foreach (ResourceVO resourceVo in list)
            {
                if (resourceVo.fileName.ToLower() == fileName.ToLower())
                {
                    return resourceVo;
                }
            }

            return null;
        }
    }

}