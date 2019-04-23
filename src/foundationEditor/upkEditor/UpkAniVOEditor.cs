using foundation;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    [CustomEditor(typeof (UpkAniVO))]
    public class UpkAniVOEditor : Editor
    {
        private UpkAniVO upkAniVo;

        private bool isPlaying = true;
        private int _currentFrame = 0;
        private int _totalFrame = 0;

        public void OnEnable()
        {
            upkAniVo = (UpkAniVO) target;
            _totalFrame = upkAniVo.keys.Count;
            _currentFrame = 0;
            _frameTime = 0;
        }

        public void OnDisable()
        {
            EditorTickManager.Remove(tick);
        }

        private float _frameTime=0;
        private void tick(float deltaTime)
        {
            _frameTime+=deltaTime;
            if (_frameTime >= 1.0f/upkAniVo.fps)
            {
                _frameTime = 0;
                _currentFrame++;
                if (_currentFrame > _totalFrame - 1)
                {
                    _currentFrame = 0;
                }
            }
        }
        public override void OnInspectorGUI()
        {
            if (upkAniVo == null)
            {
                return;
            }

            isPlaying = EditorGUILayout.ToggleLeft("playing", isPlaying, GUILayout.Width(50));

            upkAniVo.fps = EditorGUILayout.IntSlider(upkAniVo.fps, 1, 60);
            if (isPlaying)
            {
                EditorTickManager.Add(tick);
            }
            else
            {
                EditorTickManager.Remove(tick);
            }

            _currentFrame = _currentFrame%_totalFrame;

            SpriteInfoVO spriteInfoVO = upkAniVo.keys[_currentFrame];
            Sprite sprite = spriteInfoVO.sprite;

            if (isPlaying==false)
            {
                spriteInfoVO.delay = EditorGUILayout.FloatField("delay", spriteInfoVO.delay);
            }

            EditorGUILayout.BeginHorizontal();
            upkAniVo.loop = EditorGUILayout.ToggleLeft("loop", upkAniVo.loop, GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            GUIStyle style = EditorStyles.miniButton;
            if (isPlaying == false)
            {
                if (GUILayout.Button("prev", EditorStyles.miniButtonLeft))
                {
                    _currentFrame--;
                    if (_currentFrame < 0)
                    {
                        _currentFrame = _totalFrame - 1;
                    }
                }
                if (GUILayout.Button("next", EditorStyles.miniButtonMid))
                {
                    _currentFrame++;
                    if (_currentFrame > _totalFrame - 1)
                    {
                        _currentFrame = 0;
                    }
                }
                style = EditorStyles.miniButtonRight;
            }
            
            if (GUILayout.Button("save", style))
            {
                EditorUtility.SetDirty(upkAniVo);
                AssetDatabase.SaveAssets();
            }


            
            EditorGUILayout.EndHorizontal();

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y += rect.height + 100;
            
            rect.width = 300;
            rect.height = 300;
            rect.x = (Screen.width - rect.width)/2;

            Rect labelRect = rect;
            labelRect.height = 20;
            EditorGUI.LabelField(rect, sprite.name+"("+(_currentFrame+1)+"/"+_totalFrame+")");

            EditorUtils.DrawSprite(rect, sprite, true);
            if (isPlaying)
            {
                HandleUtility.Repaint();
            }
        }
    }
}