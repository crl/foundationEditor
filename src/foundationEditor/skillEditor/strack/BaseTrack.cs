using System;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class BaseTrack : ITrack
    {
        private static string[] playCountString = new[] { "循环", "不循环", "1次", "2次", "3次", "4次", "5次", "6次", "8次", "10次"};
        private static int[] playCountInt = new int[] { -1, 0, 1, 2, 3, 4, 5, 6, 8, 10 };

        public virtual Color stackColor
        {
            get
            {
                return Color.cyan;
            }
        }

        public virtual Color defaultPointColor
        {
            get
            {
                return Color.black;
            }
        }

        public virtual void drawPoint(SkillPointVO pointVo,SkillLineVO lineVo,Rect r, Texture2D texture2D)
        {
            ISkillEvent e = pointVo.evt;
            if (pointVo.evt is EmptyEvent)
            {
                GUI.color = Color.red;
            }
            else
            {
                if (e.enabled == false || lineVo.enabled==false)
                {
                    GUI.color = Color.gray;
                }
                else
                {
                    GUI.color = defaultPointColor;
                }
            }
            GUI.DrawTexture(r, texture2D);
            GUI.color = Color.white;
        }

        public void OnGUI(Rect rr, SkillLineVO lineVo, int i)
        {
            if (lineVo.enabled==false)
            {
                GUI.color = Color.gray;
            }
            else
            {
                GUI.color = stackColor;
            }

            GUI.DrawTexture(rr, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
            string name = lineVo.name;
            if (string.IsNullOrEmpty(name))
            {

                name = this.GetType().Name;
            }
            GUI.BeginGroup(rr);

            rr = new Rect(5, 3, 20, (int)rr.height - 8);
            lineVo.enabled = EditorGUI.ToggleLeft(rr, "", lineVo.enabled);
            rr.x += 20;
            rr.width = 120;
            lineVo.name = EditorGUI.TextField(rr, name);

            rr.x += 120;
            rr.width = 60;
            lineVo.targetType = (EventTargetType) EditorGUI.EnumPopup(rr, lineVo.targetType);

            rr.x += 60;
            rr.width = 60;

            lineVo.playCount = EditorGUI.IntPopup(rr, lineVo.playCount, playCountString, playCountInt);

            GUI.EndGroup();
        }


    }
}