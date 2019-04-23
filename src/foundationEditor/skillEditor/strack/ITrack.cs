using gameSDK;
using UnityEngine;

namespace foundationEditor
{
    public interface ITrack
    {
        void OnGUI(Rect rect, SkillLineVO lineVo, int index);
        void drawPoint(SkillPointVO skillPointVo, SkillLineVO lineVo,Rect r, Texture2D texture2D);
    }
}