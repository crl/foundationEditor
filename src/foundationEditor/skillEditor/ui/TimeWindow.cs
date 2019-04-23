using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using foundation;
using gameSDK;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class TimeWindow : EditorBox
    {
        public static string EditorPrefs_Key= "TimeEditorPrefs_";
        public const int frameTime = 33;//1000/33=30帧/s；
        public const int totalFrame = 300;
        public BaseObject selectedbaseObject;

        private EditorButton playButton;

        private EditorSlider timeScaleSlider;
        private string _configPath;
        private SkillTimeLineVO _skillTimeLineVo;
        private string _currentFilePath;

        public TimeLine timeLine;
        private SkillTimeLineVO _currentSkillTimeLineVo;
        private float _currentTime = 0;
        private int _endFrame = 0;
        private BaseSkill _currentBaseSkill;
        private bool _isPlaying = false;
        private EditorButton fileNameButton;
        private EditorButton commitButton;
        private EditorBox skillListBox;
        public TimeWindow()
        {
            this.styleString = "PreBackground";
            EditorBox box = new EditorBox(false);
            box.styleString = "box";
            EditorButton newButton = new EditorButton("创建");
            newButton.styleString =  "ButtonLeft";
            newButton.expandWidth = false;
            newButton.addEventListener(EventX.ITEM_CLICK, newHandle);
            box.addChild(newButton);

            EditorButton button = new EditorButton("加载");
            button.styleString = "ButtonMid";
            button.expandWidth = false;
            button.addEventListener(EventX.ITEM_CLICK, loadHandle);
            box.addChild(button);

            button = new EditorButton("保存");
            button.styleString = "ButtonMid";
            button.expandWidth = false;
            button.addEventListener(EventX.ITEM_CLICK, saveHandle);
            box.addChild(button);

            button = new EditorButton("另存");
            button.styleString = "ButtonMid";
            button.expandWidth = false;
            button.addEventListener(EventX.ITEM_CLICK, toSaveHandle);
            box.addChild(button);

            playButton = new EditorButton("播放");
            playButton.styleString = "ButtonRight";
            playButton.expandWidth = false;
            playButton.addEventListener(EventX.ITEM_CLICK, toggleHandle);
            box.addChild(playButton);

            EditorPlayControlBar cb=new EditorPlayControlBar();
            box.addChild(cb);


            box.addChild(new EditorSpace());

            fileNameButton = new EditorButton();
            fileNameButton.expandWidth = false;
            fileNameButton.addEventListener(EventX.ITEM_CLICK, openPathHandle);
            box.addChild(fileNameButton);
            fileNameButton.visible = false;

            commitButton = new EditorButton("提交");
            commitButton.expandWidth = false;
            commitButton.addEventListener(EventX.ITEM_CLICK, commitPathHandle);
            box.addChild(commitButton);
            commitButton.visible = false;


            timeScaleSlider = new EditorSlider("时间缩放");
            timeScaleSlider.setRank(0.1f, 2f, 1.0f);
            timeScaleSlider.widthOption = GUILayout.Width(200);
            timeScaleSlider.addEventListener(EventX.CHANGE, timeScaleHandle);
            box.addChild(timeScaleSlider);

            this.addChild(box);

            EditorButton resetButton = new EditorButton("reset");
            resetButton.expandWidth = false;
            resetButton.addEventListener(EventX.ITEM_CLICK, resetTimeHandle);
            box.addChild(resetButton);

            EditorLabel label = new EditorLabel("fps:" + (int)(1000 / frameTime) + "帧/s");
            box.addChild(label);

            timeLine = new TimeLine();
            timeLine.genericMenuEditorCallBack = genericMenuEditorCallBack;
            timeLine.addMenuEditorCallBack = addMenuEditorCallBack;
            timeLine.addEventListener(EventX.SELECT, innerDirectDispatchEvent);

            skillListBox=new EditorBox(true);

            this.addChild(timeLine);
            this.addChild(skillListBox);
        }

        private void addMenuEditorCallBack(string cmd)
        {
            if (_currentSkillTimeLineVo == null)
            {
                return;
            }
            SkillLineVO lineVo = new SkillLineVO();
            lineVo.typeFullName = cmd;
            _currentSkillTimeLineVo.addLine(lineVo);

            updateView(_currentSkillTimeLineVo);
        }
        private void genericMenuEditorCallBack(Vector2 v, string cmd)
        {
            if (_currentSkillTimeLineVo == null)
            {
                return;
            }
            int row = (int)v.y;
            int col = (int) v.x;
            int toIndex = 0;
            SkillLineVO lineVo;
            SkillPointVO pointVo;
            switch (cmd)
            {
                case "Remove":
                    lineVo = _currentSkillTimeLineVo.lines[row];
                    if (lineVo != null)
                    {
                        if (EditorUtility.DisplayDialog("Delete", "确定删除?", "确定", "取消"))
                        {
                            _currentSkillTimeLineVo.removeLine(lineVo);
                        }
                    }
                    break;

                case "Up":
                    toIndex=row - 1;
                    if (toIndex < 0)
                    {
                        return;
                    }
                    lineVo = _currentSkillTimeLineVo.lines[row];
                    if (lineVo != null)
                    {
                        _currentSkillTimeLineVo.removeLine(lineVo);
                        _currentSkillTimeLineVo.lines.Insert(toIndex, lineVo);
                    }
                    break;
                case "Down":
                    toIndex = row + 1;
                    if (toIndex > _currentSkillTimeLineVo.lines.Count-1)
                    {
                        return;
                    }
                    lineVo = _currentSkillTimeLineVo.lines[row];
                    if (lineVo != null)
                    {
                        _currentSkillTimeLineVo.removeLine(lineVo);
                        _currentSkillTimeLineVo.lines.Insert(toIndex, lineVo);
                    }
                    break;

                case "AddPointer":
                    lineVo = _currentSkillTimeLineVo.lines[row];
                    if (lineVo != null)
                    {
                        lineVo.insert(col, new SkillPointVO());
                        refreashLineVO(lineVo);
                    }
                    break;
                case "RemovePointer":
                    lineVo = _currentSkillTimeLineVo.lines[row];
                    if (lineVo != null)
                    {
                        pointVo = lineVo.points[col];
                        lineVo.removePoint(pointVo);
                        refreashLineVO(lineVo);
                    }
                    break;

                case "Copy":
                    lineVo = _currentSkillTimeLineVo.lines[row];
                    if (lineVo != null)
                    {
                        pointVo = lineVo.points[col];
                        if (pointVo != null)
                        {
                            timeLinePointCopy = new ByteArray();
                            timeLinePointCopy.WriteObject(pointVo);
                            timeLinePointCopy.Position = 0;
                        }
                    }

                    break;

                case "Parse":

                    if (timeLinePointCopy != null)
                    {
                        timeLinePointCopy.Position = 0;
                    }
                    SkillPointVO pointVoCopy = null;
                    try
                    {
                        pointVoCopy = timeLinePointCopy.ReadObject() as SkillPointVO;
                    }
                    catch (Exception ex)
                    {
                        ShowNotification("粘贴数据非事件点数据:" + ex.Message);
                    }
                    if (pointVoCopy != null)
                    {
                        lineVo = _currentSkillTimeLineVo.lines[row];
                        if (lineVo != null)
                        {
                            pointVo = lineVo.points[col];
                            SkillEvent skillEvent = pointVoCopy.evt;
                            if (skillEvent!=null)
                            {
                                pointVo.addEvent(skillEvent);
                            }
                        }
                    }
                    break;
            }
            updateView(_currentSkillTimeLineVo);
        }

        private static ByteArray timeLinePointCopy;
        private void refreashLineVO(SkillLineVO lineVo)
        {
            int i = 0;
            foreach (SkillPointVO pointVo in lineVo.points)
            {
                pointVo.startTime = i * frameTime;
                i++;
            }
        }

        public void init(string configPath, PropertyWindow propertyWindow)
        {
            _configPath = configPath;
            timeLine.propertyWindow = propertyWindow;
        }

        private void toggleHandle(EventX obj)
        {
            if (playButton.text != "停止")
            {
                playHandle(null);
            }
            else
            {
                stopHandle(null);

                if (Event.current.control)
                {
                    playHandle(null);
                }
            }
        }

        private void pauseHandle(EventX e)
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        private void pauseNextHandle(EventX e)
        {
            //EditorApplication.
        }


        private void saveHandle(EventX e)
        {
            if (timeLine.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                string path = OpenSaveFilePanel("default");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                _currentFilePath = path;
            }

            SkillTimeLineVO newSkillTimeLineVo = getRuntimeSkillVO();
            FileHelper.SaveAMF(newSkillTimeLineVo, _currentFilePath);

            setEditFileFullPath(_currentFilePath);

            ShowNotification("保存成功!");
        }

        private void toSaveHandle(EventX e)
        {
            if (timeLine.Count == 0)
            {
                return;
            }

            string saveName = "default";
            if (string.IsNullOrEmpty(_currentFilePath)==false)
            {
                saveName = Path.GetFileNameWithoutExtension(_currentFilePath);
            }
            string path = OpenSaveFilePanel(saveName);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _currentFilePath = path;
            SkillTimeLineVO newSkillTimeLineVo = getRuntimeSkillVO();
            FileHelper.SaveAMF(newSkillTimeLineVo, _currentFilePath);

            setEditFileFullPath(_currentFilePath);

            ShowNotification("保存成功!");
        }

        private void resetTimeHandle(EventX e)
        {
            timeScaleSlider.value = 1.0f;
            timeScaleHandle(e);
        }
        private void timeScaleHandle(EventX e)
        {
            Time.timeScale = timeScaleSlider.value;
        }
        private void openPathHandle(EventX e)
        {
           string dir= Path.GetDirectoryName(_currentFilePath);
            if (Directory.Exists(dir))
            {
                EditorUtility.OpenWithDefaultApp(dir);
            }
        }

        private void commitPathHandle(EventX e)
        {
            string dir = Path.GetDirectoryName(_currentFilePath);
            if (Directory.Exists(dir))
            {
                string cmd = string.Format("/command:commit /path:{0}", dir);
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(EditorConfigUtils.svnExe, cmd);
                process.EnableRaisingEvents = true;
                process.Start();
            }
        }

        private string OpenSaveFilePanel(string saveName)
        {
            string dir=EditorPrefs.GetString(EditorPrefs_Key+"_open");
            if (Directory.Exists(dir) == false)
            {
                dir = _configPath;
            }
            return EditorUtility.SaveFilePanel("选择文件", dir, saveName, "amf");
        }

        private void loadHandle(EventX e)
        {
            string dir = EditorPrefs.GetString(EditorPrefs_Key + "_open");
            if (Directory.Exists(dir) == false)
            {
                dir = _configPath;
            }
            string path = EditorUtility.OpenFilePanel("选择文件", dir, "amf");
            if (string.IsNullOrEmpty(path))
            {
                ShowNotification("请选择文件");
                return;
            }

            EditorPrefs.SetString(EditorPrefs_Key+"_TimeLine",path);
            load(path);
        }

        private void load(string path)
        {
            try
            {
                _skillTimeLineVo = FileHelper.GetAMF(path) as SkillTimeLineVO;
            }
            catch (Exception ex)
            {
                ShowNotification("文件不正确" + ex.Message);
                return;
            }

            if (_skillTimeLineVo == null)
            {
                ShowNotification("文件不正确 is null");
                return;
            }

            if (timeLine.propertyWindow != null)
            {
                timeLine.propertyWindow.hide();
            }

            _currentFilePath = path;
            updateView(_skillTimeLineVo);

            setEditFileFullPath(_currentFilePath);

            playHandle(new EventX(EventX.READY));

        }

        private void ShowNotification(string message)
        {
            if (window != null)
            {
                window.ShowNotification(new GUIContent(message));
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
        }

        private void setEditFileFullPath(string value)
        {
            this.fileNameButton.data = value;
            string name = Path.GetFileNameWithoutExtension(value);
            if (string.IsNullOrEmpty(name))
            {
                name = "";
            }

            if (File.Exists(value))
            {
                string dir = Path.GetDirectoryName(value);
                EditorPrefs.SetString(EditorPrefs_Key + "_open", dir);
                this.fileNameButton.visible = this.commitButton.visible = true;
                this.fileNameButton.text = name;
            }
            else
            {
                this.fileNameButton.visible = this.commitButton.visible = false;
            }
        }

        private void playHandle(EventX eventX)
        {
            SkillTimeLineVO skillTimeLineVo = getRuntimeSkillVO(true);

            if (skillTimeLineVo == null || selectedbaseObject == null)
            {
                return;
            }

            playButton.text = "停止";
            Time.timeScale = timeScaleSlider.value; // = 1.0f;
            _currentTime = 0;
            int currentMaxTime = 0;
            foreach (SkillLineVO lineVo in skillTimeLineVo.lines)
            {
                foreach (SkillPointVO pointVo in lineVo.points)
                {
                    if (pointVo.startTime < currentMaxTime) continue;
                    currentMaxTime = pointVo.startTime;
                }
            }

            _endFrame = Mathf.CeilToInt(currentMaxTime / frameTime);

          
            if (_currentBaseSkill != null)
            {
                _currentBaseSkill.stop();
            }
            _currentBaseSkill = new BaseSkill();

            SkillExData exData = selectedbaseObject.getSkillExData();
            exData.isHero = true;

            _currentBaseSkill.createBy(selectedbaseObject, null, exData);
            _currentBaseSkill.play(skillTimeLineVo);

            window.AddTick(tickHandle);
        }

        //private List<string> relevancySkilList=new List<string>();
        public void searchSkillListBy(string fileName)
        {
            skillListBox.removeAllChildren();

            string path=_configPath + fileName;
            if (Directory.Exists(path) == false)
            {
                path = _configPath + StringUtil.trimDig(fileName);
                if (Directory.Exists(path) == false)
                {
                    return;
                }
            }

            EditorBox lineBox = new EditorBox(false);
            skillListBox.addChild(lineBox);

            List<string> extendsList=new List<string>();
            extendsList.Add("*.amf");
            List<string> files=FileHelper.FindFile(path, extendsList);

            int totalWidth=0;
            foreach (string file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                Vector2 playSize = GUI.skin.button.CalcSize(new GUIContent(name));
                totalWidth += (int)playSize.x;

                EditorButton btn=new EditorButton(name);
                btn.expandWidth = false;
                btn.addEventListener(EventX.ITEM_CLICK, skillItemClickHandle);
                btn.data = file;
                lineBox.addChild(btn);

                if (totalWidth > stage.stageWidth-650)
                {
                    totalWidth = 0;
                    lineBox =new EditorBox(false);
                    skillListBox.addChild(lineBox);
                }
            }
        }

        private void skillItemClickHandle(EventX e)
        {
            EditorButton btn =e.target as EditorButton;
            string file = (string)btn.data;
            if (File.Exists(file))
            {
                load(file);
            }
            else
            {
                ShowNotification("文件不存在");
            }
        }

        private void stopHandle(EventX e)
        {
            playButton.text = "播放";
            if (_currentBaseSkill != null)
            {
                _currentBaseSkill.stop();
                _currentBaseSkill = null;
            }
            _isPlaying = false;
            window.RemoveTick(tickHandle);
        }
        private void tickHandle(float deltaTime)
        {
            _currentTime += (int)(deltaTime*1000);

            int index = (int)(_currentTime / frameTime);
            //Debug.Log("d:"+index+"\t"+currentTime);
          
            this.window.Repaint();
            if (index > totalFrame - 1 || index > _endFrame)
            {
                stopHandle(null);
                return;
            }
            timeLine.keyFramePosition = index;
            if (_isPlaying == false)
            {
                _isPlaying = true;
            }
        }

        private void newHandle(EventX obj)
        {
            _currentFilePath = null;
            SkillTimeLineVO skillTimeLineVo = new SkillTimeLineVO();

            SkillLineVO vo = new SkillLineVO();
            vo.typeFullName = typeof(AnimatorTrack).FullName;
            skillTimeLineVo.addLine(vo);

            updateView(skillTimeLineVo);

            setEditFileFullPath("新创文件");
        }

        public SkillTimeLineVO getRuntimeSkillVO(bool isEditorUse = false)
        {
            SkillTimeLineVO skillTimeLineVo = new SkillTimeLineVO();
            List<SkillLineVO> lines = timeLine.dataProvider;

            if (lines == null)
            {
                return null;
            }

            int len = lines.Count;
            for (int i = 0; i < len; i++)
            {
                SkillLineVO lineVO = lines[i];
                List<SkillPointVO> pointVos = lineVO.points as List<SkillPointVO>;
                if (pointVos == null)
                {
                    continue;
                }

                List<SkillPointVO> resultPointVos = new List<SkillPointVO>();
                foreach (SkillPointVO pointVo in pointVos)
                {
                    if (pointVo.isEmpty == false)
                    {
                        resultPointVos.Add(pointVo);
                    }
                }

                if (resultPointVos.Count > 0)
                {
                    SkillLineVO newLineVO = new SkillLineVO();
                    newLineVO.copyFrom(lineVO);
                    newLineVO.points = resultPointVos;
                    if (newLineVO.enabled == false && isEditorUse)
                    {
                        continue;
                    }
                    skillTimeLineVo.addLine(newLineVO);
                }
            }
            return skillTimeLineVo;
        }

        public void updateView(SkillTimeLineVO value)
        {
            timeLine.Clear();

            _currentSkillTimeLineVo = new SkillTimeLineVO();
            foreach (SkillLineVO lineVO in value.lines)
            {
                List<SkillPointVO> points = new List<SkillPointVO>();

                for (int i = 0; i < totalFrame; i++)
                {
                    SkillPointVO pointVo = new SkillPointVO();
                    pointVo.startTime = i * frameTime;
                    points.Add(pointVo);
                }

                int len = Math.Min(lineVO.points.Count, totalFrame);
                for (int i = 0; i < len; i++)
                {
                    SkillPointVO pointVo = lineVO.points[i];
                    int index = (int)(pointVo.startTime / frameTime);

                    if (index >= totalFrame)
                    {
                        this.ShowNotification("超出时间线:"+index+ "被修正:"+totalFrame);
                        index = totalFrame - 1;
                        pointVo.startTime = index * frameTime;
                    }

                    points[index] = pointVo;
                }

                SkillLineVO newLineVo = new SkillLineVO();
                newLineVo.copyFrom(lineVO);
                newLineVo.points = points;
                _currentSkillTimeLineVo.addLine(newLineVo);
            }

            timeLine.dataProvider = _currentSkillTimeLineVo.lines;
        }

        private bool isFirst = true;



        public override void onRender()
        {
            if (isFirst)
            {
                string loadPath = EditorPrefs.GetString(EditorPrefs_Key + "_TimeLine");
                if (File.Exists(loadPath))
                {
                    load(loadPath);
                }
                isFirst = false;
            }
            base.onRender();
        }
    }
}