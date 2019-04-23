using System;
using System.Collections.Generic;
using System.IO;
using foundation;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace foundationEditor
{

    /// <summary>
    /// 代码生成器V2
    /// </summary>
    public class UICodeExport
    {

        public static string CodeExtendsion = ".cs";

        public static string UIName;

        public static bool onlyGenerateMediator = false;

        /// <summary>
        /// 模板路径
        /// </summary>
        public static string templatePath = Application.dataPath + "/Editor/templete/v2/";


        /// <summary>
        /// 模块路径
        /// </summary>
        public static string modulePathPrefix = Application.dataPath + "/Scripts/Game/module/";
        public static string modulePath;

        [MenuItem("Tools/UITool/代码生成器")]
        private static void CodeExport()
        {
            var go = Selection.activeGameObject;

            if (go == null)
            {
                EditorUtility.DisplayDialog("提示", "选择物体为null", "ok");
                return;
            }
            var el = new UICodeExport();
            el.decode(go);

            el.dispose();

        }

        public static string UpperFirstChar(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (str.Length <= 1)
            {
                return str.ToUpper();
            }

            var firstChar = str[0].ToString().ToUpper();

            var other = str.Substring(1);

            return firstChar + other;
        }

        public static string LowerFirstChar(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (str.Length <= 1)
            {
                return str.ToLower();
            }

            var firstChar = str[0].ToString().ToLower();

            var other = str.Substring(1);

            return firstChar + other;
        }


        private void dispose()
        {
            if (root != null)
            {
                root.dispose();
            }
        }

        

        private UIElementNode root;

        public UICodeExport()
        {
           

        }


        public void decode(GameObject go)
        {
            if (go.name.IndexOf("UI") == 0)
            {
                UIName = go.name;

                var folderName = UIName.Substring(2) + "UI";
                folderName = LowerFirstChar(folderName);
                modulePath = modulePathPrefix + folderName + "/";

                CodeExportSetting setting = go.GetComponent<CodeExportSetting>();
                if (setting != null)
                {
                    if (string.IsNullOrEmpty(setting.parentModuleName) == false)
                    {
                        modulePath = modulePathPrefix + setting.parentModuleName + "/";
                    }
                    onlyGenerateMediator = setting.onlyGenerateMediator;
                }
                else
                {
                    onlyGenerateMediator = false;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "请以 UI{0} 命名", "ok");
                return;
            }

            root = new UIElementNode();
            root.initilize(go);

            //生成ViewCode
            var viewCode  = new ViewCodeTemplate();
            viewCode.execute(root);

            //生成View
            var viewTemplate = new ViewTemplate();
            viewTemplate.execute(root);

            //生成Mediator
            var meditorTemplate = new MediatorTemplate();
            meditorTemplate.execute(root);

            if (onlyGenerateMediator == false)
            {
                //生成Proxy
                var proxyTemplate = new ProxyTemplate();
                proxyTemplate.execute(root);

                //生成Operater
                var operateTemplate = new OperaterTemplate();
                operateTemplate.execute(root);

                //生成Decoder
                var decoderTemplate = new DecoderTemplate();
                decoderTemplate.execute(root);
            }

            createOther(root);

            EditorUtility.DisplayDialog("提示", UIName + " 代码生成成功", "确定");

        }

        private void createOther(UIElementNode root)
        {
            foreach (var node in root.children)
            {
                if (node.gameObject.name.IndexOf("dele_") == 0)
                {
                    //生成PanelDelegate
                    var dele = new DelegateTemplate();
                    dele.execute(node);

                    var deleCode = new DelegateCodeTemplate();
                    deleCode.execute(node);
                }

                if (node.gameObject.name.IndexOf("itemRender_") == 0)
                {
                    //生成ItemRender
                    var itemRender = new ItemRenderTemplate();
                    itemRender.execute(node);

                    var itemRenderCode = new ItemRenderCodeTemplate();
                    itemRenderCode.execute(node);
                }

                if (node.children.Count > 0)
                {
                    createOther(node);
                }

            }
        }
    }

    class BaseTemplate
    {
        
        public string uri;

        public string varSpace = "        ";
        public string addressSpace = "            ";
        public string breakLine = "\r\n";

        public virtual void execute(UIElementNode node)
        {
            
        }

        protected virtual string outputAddress(UIElementNode node)
        {
            string output = "";
            string v = "";
            foreach (UIElementPropertyBase prop in node.propterties)
            {
                v = prop.outputProperty();
                if (string.IsNullOrEmpty(v) == false)
                {
                    output += addressSpace + v + breakLine;
                }
            }

            HashSet<string> defHashSet=new HashSet<string>();

            foreach (UIElementPropertyBase prop in node.localPropterties)
            {
                v = prop.outputDefine();
                if (string.IsNullOrEmpty(v) == false && defHashSet.Contains(v)==false)
                {
                    defHashSet.Add(v);
                    output += addressSpace + v + breakLine;
                }

                v = prop.outputProperty();
                if (string.IsNullOrEmpty(v) == false)
                {
                    output += addressSpace + v + breakLine;
                }
            }

            return output;
        }

        protected virtual string outputVar(UIElementNode node)
        {
            string output = "";
            string v = "";
            foreach (var prop in node.propterties)
            {
                v = prop.outputDefine();
                if (string.IsNullOrEmpty(v) == false)
                {
                    output += varSpace + v + breakLine;
                }
            }

            return output;
        }
    }

    class ViewTemplate : BaseTemplate
    {
        public ViewTemplate()
        {
            this.uri = "View.templete";
            
        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            var moduleName = UICodeExport.UIName;

            string savePath = UICodeExport.modulePath + moduleName + "" + UICodeExport.CodeExtendsion;

            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", UICodeExport.UIName);

            
            //content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varadvname]", outputAdvName(node));
            content = content.Replace("[varaddress]", outputAddress(node));
            content = content.Replace("[varadvaddress]", outputAdvAddress(node));


            FileHelper.SaveUTF8(content, savePath);
        }

        private string outputAdvAddress(UIElementNode node)
        {
            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIPageListPropertyElement)
                {
                    output += addressSpace + prop.outputAdvProperty() + breakLine;
                }
            }

            return output;
        }

        private string outputAdvName(UIElementNode node)
        {
            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIPageListPropertyElement)
                {
                    output += varSpace + prop.outputAdvDefine() + breakLine;
                }
            }

            return output;
        }

        protected override string outputAddress(UIElementNode node)
        {
            var go = node.getChildByName("btn_close");
            if (go != null)
            {
                return addressSpace + "btn_close.addEventListener(EventX.CLICK, hide);" + breakLine;
            }
            return "";
        }
    }

    class ViewCodeTemplate : BaseTemplate
    {
        public ViewCodeTemplate()
        {
            this.uri = "ViewCode.templete";

        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            var moduleName = UICodeExport.UIName;

            string savePath = UICodeExport.modulePath + "code/Code" + moduleName + UICodeExport.CodeExtendsion;


            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", UICodeExport.UIName);
            content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varaddress]", outputAddress(node));

            
            FileHelper.SaveUTF8(content, savePath);
        }


    }

    class MediatorTemplate : BaseTemplate
    {
        public MediatorTemplate()
        {
            this.uri = "Mediator.templete";
        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            var moduleName = UICodeExport.UIName;

            var proxyName = UICodeExport.UIName.Substring(2);

            string savePath = UICodeExport.modulePath + moduleName + "Mediator" + UICodeExport.CodeExtendsion;

            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);


            content = content.Replace("[name]", UICodeExport.UIName);
            content = content.Replace("[proxyName]", proxyName);
            content = content.Replace("[varimport]", outputImport(node));
            content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varaddress]",outputAddress(node));

            FileHelper.SaveUTF8(content, savePath);
        }

        protected string outputImport(UIElementNode node)
        {
            foreach (var prop in node.propterties)
            {
                if (prop is UITabPropertyElement)
                {
                    return "using clayui;";
                }
            }
            return "";
        }

        protected override string outputVar(UIElementNode node)
        {
            //只要tab
            foreach (var prop in node.propterties)
            {
                if (prop is UITabPropertyElement)
                {
                    return varSpace +"private TabNav tabNav = new TabNav();";
                }
            }

            return "";
        }

        protected override string outputAddress(UIElementNode node)
        {
            string str = "";
            foreach (var prop in node.propterties)
            {
                if (prop is UITabPropertyElement)
                {
                    str += addressSpace + "tabNav.addMediator(view.tab_"+prop.name + ", view.dele_"+ prop.name+");\r\n";
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                str += addressSpace + "tabNav.selectedIndex = 0;\r\n";
            }

            return str;
        }
    }

    class ProxyTemplate : BaseTemplate
    {
        public ProxyTemplate()
        {
            this.uri = "Proxy.templete";

        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            var proxyName = UICodeExport.UIName.Substring(2);

            string savePath = UICodeExport.modulePath + proxyName + "Proxy" + UICodeExport.CodeExtendsion;

            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", proxyName);
            //            content = content.Replace("[varname]", outputVar(node));
            //            content = content.Replace("[varaddress]",outputAddress(node));

            FileHelper.SaveUTF8(content, savePath);
        }
    }

    class OperaterTemplate : BaseTemplate
    {
        public OperaterTemplate()
        {
            this.uri = "Operater.templete";

        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            var proxyName = UICodeExport.UIName.Substring(2);

            string savePath = UICodeExport.modulePath + proxyName + "Operater" + UICodeExport.CodeExtendsion;

            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", proxyName);
            //            content = content.Replace("[varname]", outputVar(node));
            //            content = content.Replace("[varaddress]",outputAddress(node));

            FileHelper.SaveUTF8(content, savePath);
        }
    }

    class DecoderTemplate : BaseTemplate
    {
        public DecoderTemplate()
        {
            this.uri = "Decoder.templete";

        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            var proxyName = UICodeExport.UIName.Substring(2);

            string savePath = UICodeExport.modulePath + proxyName + "Decoder" + UICodeExport.CodeExtendsion;

            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", proxyName);
            //            content = content.Replace("[varname]", outputVar(node));
            //            content = content.Replace("[varaddress]",outputAddress(node));

            FileHelper.SaveUTF8(content, savePath);
        }
    }


    class DelegateTemplate : BaseTemplate
    {
        public DelegateTemplate()
        {
            this.uri = "Delegate.templete";
        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            //var moduleName = UICodeExport.UIName;

            var proxyName = UICodeExport.UIName.Substring(2);

            //模块名字 dele_xxx
            var fullName = node.gameObject.name;
            var arr = fullName.Split('_');

            var deleName = UICodeExport.UpperFirstChar(arr[1]);

            string savePath = UICodeExport.modulePath + "dele/" + deleName + "Delegate" + UICodeExport.CodeExtendsion;


            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", deleName);
            content = content.Replace("[proxyName]", proxyName);
            content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varaddress]", outputAddress(node));
            content = content.Replace("[varhandler]", outputHandler(node));


            content = content.Replace("[varadvname]", outputAdvName(node));
            content = content.Replace("[varadvaddress]", outputAdvAddress(node));


            FileHelper.SaveUTF8(content, savePath);
        }

        private string outputAdvAddress(UIElementNode node)
        {
            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIPageListPropertyElement)
                {
                    output += addressSpace + prop.outputAdvProperty() + breakLine;
                }
            }

            return output;
        }

        private string outputAdvName(UIElementNode node)
        {
            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIPageListPropertyElement)
                {
                    output += varSpace + prop.outputAdvDefine() + breakLine;
                }
            }

            return output;
        }

        protected override string outputAddress(UIElementNode node)
        {

            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIBtnPropertyElement)
                {
                    output +=   addressSpace +
                                UICodeExport.LowerFirstChar(prop.fullName) +".addEventListener(EventX.CLICK, on" +
                                UICodeExport.UpperFirstChar(prop.name) +
                                "Click);" + 
                                breakLine;
                }
            }
            return output;

            
        }

        protected  string outputHandler(UIElementNode node)
        {

            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIBtnPropertyElement)
                {
                    if (prop.name.Equals("close"))
                    {
                        output += prop.outputHandler(addressSpace + "hide();" + breakLine, varSpace,addressSpace,breakLine);
                    }
                    else
                    {
                        output += prop.outputHandler(addressSpace + "//TODO" + breakLine, varSpace, addressSpace, breakLine);
                    }
                }
            }

            return output;
        }
    }

    class DelegateCodeTemplate : BaseTemplate
    {
        public DelegateCodeTemplate()
        {
            this.uri = "DelegateCode.templete";

        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            //var moduleName = UICodeExport.UIName;

            //模块名字 dele_xxx
            var fullName = node.gameObject.name;
            var arr = fullName.Split('_');

            var deleName = UICodeExport.UpperFirstChar(arr[1]);

            string savePath = UICodeExport.modulePath + "code/Code" + deleName + "Delegate" + UICodeExport.CodeExtendsion;


            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", deleName);
            content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varaddress]",outputAddress(node));

            FileHelper.SaveUTF8(content, savePath);
        }
    }

    class ItemRenderTemplate : BaseTemplate
    {
        public ItemRenderTemplate()
        {
            this.uri = "ItemRender.templete";
        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            //var moduleName = UICodeExport.UIName;

            //模块名字 dele_xxx
            var fullName = node.gameObject.name;
            var arr = fullName.Split('_');

            var renderName = UICodeExport.UpperFirstChar(arr[1]);

            string savePath = UICodeExport.modulePath + "itemRender/" + renderName + "ItemRender" + UICodeExport.CodeExtendsion;

            if (File.Exists(savePath) == true)
            {
                //不重复生成，以免覆盖已有逻辑
                return;
            }

            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", renderName);
            content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varaddress]", outputAddress(node));
            content = content.Replace("[varhandler]", outputHandler(node));


            content = content.Replace("[varadvname]", outputAdvName(node));
            content = content.Replace("[varadvaddress]", outputAdvAddress(node));


            FileHelper.SaveUTF8(content, savePath);
        }

        private string outputAdvAddress(UIElementNode node)
        {
            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIPageListPropertyElement)
                {
                    output += addressSpace + prop.outputAdvProperty() + breakLine;
                }
            }

            return output;
        }

        private string outputAdvName(UIElementNode node)
        {
            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIPageListPropertyElement)
                {
                    output += varSpace + prop.outputAdvDefine() + breakLine;
                }
            }

            return output;
        }

        protected override string outputAddress(UIElementNode node)
        {

            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIBtnPropertyElement)
                {
                    output += addressSpace +
                            UICodeExport.LowerFirstChar(prop.fullName) + ".addEventListener(EventX.CLICK, on" +
                            UICodeExport.UpperFirstChar(prop.name) +
                            "Click);" +
                            breakLine;
                }
            }
            return output;


        }

        protected string outputHandler(UIElementNode node)
        {

            string output = "";

            foreach (var prop in node.propterties)
            {
                if (prop is UIBtnPropertyElement)
                {
                    output += prop.outputHandler(addressSpace + "//TODO" + breakLine, varSpace, addressSpace, breakLine);
                }
            }

            return output;
        }
    }

    class ItemRenderCodeTemplate : BaseTemplate
    {
        public ItemRenderCodeTemplate()
        {
            this.uri = "ItemRenderCode.templete";
        }

        public override void execute(UIElementNode node)
        {
            base.execute(node);

            //模块名字
            //var moduleName = UICodeExport.UIName;

            //模块名字 dele_xxx
            var fullName = node.gameObject.name;
            var arr = fullName.Split('_');

            var renderName = UICodeExport.UpperFirstChar(arr[1]);


            String content = FileHelper.GetUTF8Text(UICodeExport.templatePath + uri);

            content = content.Replace("[name]", renderName);
            content = content.Replace("[varname]", outputVar(node));
            content = content.Replace("[varaddress]", outputAddress(node));

            string savePath = UICodeExport.modulePath  + "code/Code" + renderName + "ItemRender" + UICodeExport.CodeExtendsion;
            FileHelper.SaveUTF8(content, savePath);
        }
    }


    public class UIElementNode
    {
        

        public UIElementNode parent;

        public GameObject gameObject;

        public List<UIElementNode> children; 

        public List<UIElementPropertyBase> propterties = new List<UIElementPropertyBase>() ;

        public List<UIElementPropertyBase> localPropterties = new List<UIElementPropertyBase>();

        private ASDictionary<string,GameObject> dic = new ASDictionary<string, GameObject>();

        public void initilize(GameObject go, UIElementNode parent = null)
        {
            this.gameObject = go;
            this.parent = parent;
            //Debug.Log("UICodeExport :initilize" + go.name);
            dic.Clear();
            propterties.Clear();
            localPropterties.Clear();

            children = AS3_getChildren(go);


            //解析属性
            parseProperties();
        }

        public GameObject getChildByName(string name)
        {
            return dic[name];
        }

        private void parseProperties()
        {
            
            foreach (UIElementNode tsf in children)
            {
                var child = tsf.gameObject;
                if (child == null)
                {
                    continue;
                }
                UIElementPropertyBase prop;
                //btn_close,tab_arena,item_ArenaRank
                string[] keyValuePair = child.name.Split('_');
                if (keyValuePair.Length > 1)
                {
                    var key = keyValuePair[0];
                    var value = keyValuePair[1];
                    prop = UIElementPropertyManager.getProperty(key, value);
                    prop.gameObject = child;

                    propterties.Add(prop);

                    //处理特殊的属性
                    handleSpecialProperty(prop, child, value);
                }
                else
                {
                    //没有前缀就不申明变量
                    Text[] texts=tsf.gameObject.GetComponentsInChildren<Text>(true);
                    foreach (Text text in texts)
                    {
                        string path=text.name;

                        Transform parent = text.transform.parent;
                        while (parent != gameObject.transform)
                        {
                            path = parent.name + "/" + path;

                            parent = parent.parent;
                        }

                        UITxtPropertyLocalElement uiTxtPropertyElement =new UITxtPropertyLocalElement(text.name);
                        uiTxtPropertyElement.path = path;
                        uiTxtPropertyElement.gameObject = text.gameObject;
                        localPropterties.Add(uiTxtPropertyElement);
                    }
                }
            }
        }

        private void handleSpecialProperty(UIElementPropertyBase prop, GameObject child, string value)
        {
            if (prop is UIPageListPropertyElement)
            {
                var pagelist = prop as UIPageListPropertyElement;
                GameObject item;
                var itemTrs = child.transform.Find("itemRender_" + value);
                bool isInList = false;
                if (itemTrs)
                {
                    item = itemTrs.gameObject;
                    isInList = true;
                }
                else
                {
                    item = getChildByName("itemRender_" + value);
                }

                RectTransform itemTransform = null;
                if (item != null)
                {
                    itemTransform = item.GetComponent<RectTransform>();
                }

                if (itemTransform != null)
                {
                    pagelist.itemClass = UICodeExport.UpperFirstChar(value) + "ItemRender";
                    if (isInList)
                    {
                        pagelist.itemSkin = StringUtil.substitute("getGameObject(\"{0}/{1}\")",child.name, "itemRender_" + value);
                    }
                    else
                    {
                        pagelist.itemSkin = "itemRender_" + value;
                    }
                    
                    pagelist.itemWidth = (int) itemTransform.sizeDelta.x;
                    pagelist.itemHeight = (int) itemTransform.sizeDelta.y;
                }
            }
        }

        private  List<UIElementNode> AS3_getChildren(GameObject go)
        {
            Transform[] transforms = go.GetComponentsInChildren<Transform>(true);

            List<UIElementNode> nodes = new List<UIElementNode>();
            foreach (Transform item in transforms)
            {
                if (item.parent == go.transform)
                {
                    var node = new UIElementNode();
                    node.initilize(item.gameObject,this);

                    dic[item.gameObject.name] = item.gameObject;
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        public void dispose()
        {
            parent = null;
            gameObject = null;

            foreach (var child in children)
            {
                child.dispose();
            }

            dic.Clear();
            dic = null;
            propterties.Clear();
            propterties = null;
        }
    }

    public class UIElementPropertyManager
    {
        private static ASDictionary dic;

        static public UIElementPropertyBase getProperty(string key,string name)
        {
            if (dic == null)
            {
                initDic();
            }
            Type type = dic[key] as Type;
            if (type == null)
            {
                //Debug.LogWarning("代码生成器警告:未找到映射的属性生成器：" + key);
                type = typeof(UIElementPropertyBase);
                return Activator.CreateInstance(type, key, name) as UIElementPropertyBase;
            }
            else
            {
                return Activator.CreateInstance(type, key, name) as UIElementPropertyBase;
            }
            
        }

        private static void initDic()
        {
            dic = new ASDictionary();
            dic["common"] = typeof(UIElementPropertyBase);
            dic["tab"] = typeof(UITabPropertyElement);
            dic["btn"] = typeof(UIBtnPropertyElement);
            dic["txt"] = typeof(UITxtPropertyElement);
            dic["dele"] = typeof(UIDelePropertyElement);
            dic["img"] = typeof(UIImgPropertyElement);
            dic["rawImg"] = typeof(UIRawImagPropertyElement);
            dic["icon"] = typeof(UIIconSlotPropertyElement);
            dic["mc"] = typeof(UIImageMovieClipPropertyElement);
            dic["list"] = typeof(UIPageListPropertyElement);
            dic["sharedBg"] = typeof(UISharedBgPropertyElement);
        }
    }


    public class UIElementPropertyBase
    {
        public string name;
        public GameObject gameObject;

        /// <summary>
        ///  是否只是局部变量
        /// </summary>
        public bool isLocalDefine = false;
        public string fullName
        {
            get
            {
                if (String.IsNullOrEmpty(key))
                {
                    return name;
                }
                else
                {
                    return key + "_" + name;
                }
            }
        }

        protected string key;

        protected string import_str;
        protected string define_str;
        protected string property_str;
        protected string handler_header_str;


        public UIElementPropertyBase(string key,string name)
        {
            this.name = name;

            this.key = key;
            this.import_str = "";
            this.define_str = "public GameObject {0};";

            this.property_str = "{0} = getGameObject(\"{0}\");";
            this.handler_header_str = "";
        }

        public virtual string outputDefine()
        {
            //声明的变量首字小写
            return StringUtil.substitute(define_str, UICodeExport.LowerFirstChar(fullName));
        }

        public virtual string outputProperty()
        {
            //tab_xx = getImage("tab_xx");
            return StringUtil.substitute(property_str, fullName);
        }

        public virtual string outputAdvDefine()
        {
            return "";
        }

        public virtual string outputAdvProperty()
        {
            return "";
        }


        public virtual string outputHandler(string contents,string varSpace = "        ",string addressSpace = "            ",string breakLine = "\r\n")
        {
            if (string.IsNullOrEmpty(handler_header_str))
            {
                return "";
            }
            string output = "";
            output = varSpace + handler_header_str + breakLine;
            output += varSpace + "{" + breakLine;
            output += contents;
            output += varSpace + "}" + breakLine + breakLine;
            return output;
        }

   
    }

    public class UITabPropertyElement : UIElementPropertyBase
    {
        
        public UITabPropertyElement(string key,string name) : base(key,name)
        {
            this.import_str = "";
            this.define_str = "public Image {0};";
            this.property_str = "{0} = getImage(\"{0}\");";
        }
        
    }

    public class UIBtnPropertyElement : UIElementPropertyBase
    {

        public UIBtnPropertyElement(string key,string name) : base(key,name)
        {
            this.import_str = "";
            this.define_str = "public ClayButton {0};";
            this.property_str = "{0} = new ClayButton(getGameObject(\"{0}\"));";
            this.handler_header_str = "private void on" + UICodeExport.UpperFirstChar(name) + "Click(EventX e)";
        }

    }

    public class UITxtPropertyElement : UIElementPropertyBase
    {
        public UITxtPropertyElement(string key,string name) : base(key,name)
        {
            this.import_str = "";
            this.define_str = "public Text {0};";
            this.property_str = "{0} = getText(\"{0}\");\r\n            {0}.raycastTarget = false;\r\n            {0}.text = @\"{1}\";";
        }

        public override string outputProperty()
        {
            string oldValue=gameObject.GetComponent<Text>().text;
            return StringUtil.substitute(property_str, fullName, oldValue);
        }
    }

    public class UITxtPropertyLocalElement : UITxtPropertyElement
    {
        public string path;
        public UITxtPropertyLocalElement(string name) : base("", name)
        {
            this.import_str = "";
            this.define_str = "Text _localText;";
            this.property_str =
                "_localText = getText(\"{2}\");\r\n            " +
                "_localText.raycastTarget = false;\r\n            " +
                "_localText.text = @\"{1}\";";
        }

        public override string outputProperty()
        {
            string oldValue = gameObject.GetComponent<Text>().text;
            return StringUtil.substitute(property_str, name, oldValue,path);
        }
    }

    public class UIDelePropertyElement : UIElementPropertyBase
    {

        public UIDelePropertyElement(string key,string name) : base(key,name)
        {
            var deleClassName = UICodeExport.UpperFirstChar(name) + "Delegate";
            this.import_str = "";
            this.define_str = "public " + deleClassName + " {0};";
            this.property_str = "{0} = addDelegate<" + deleClassName + ">(getGameObject(\"{0}\"));\r\n            " +
                                "{0}.hide();";
        }
    }

    public class UIImgPropertyElement : UIElementPropertyBase
    {

        public UIImgPropertyElement(string key, string name) : base(key, name)
        {
            this.import_str = "";
            this.define_str = "public Image {0};";
            this.property_str = "{0} = getImage(\"{0}\");";
        }

    }

    public class UIRawImagPropertyElement : UIElementPropertyBase
    {
        public UIRawImagPropertyElement(string key, string name) : base(key, name)
        {
            this.import_str = "";
            this.define_str = "public RawImage {0};";
            this.property_str = "{0} = getRawImage(\"{0}\");";
        }

    }

    public class UIIconSlotPropertyElement : UIElementPropertyBase
    {
        public UIIconSlotPropertyElement(string key, string name) : base(key, name)
        {
            this.import_str = "";
            this.define_str = "public IconSlot {0};";
            this.property_str = "{0} = new IconSlot();\r\n"+"            {0}.rawImage = getRawImage(\"{0}/RawImage\");";
        }

    }

    public class UIImageMovieClipPropertyElement : UIElementPropertyBase
    {
        public UIImageMovieClipPropertyElement(string key, string name) : base(key, name)
        {
            this.import_str = "";
            this.define_str = "public ImageMovieClip {0};";
            this.property_str = "{0} = new ImageMovieClip(getGameObject(\"{0}\"));";
        }

    }

    public class UIPageListPropertyElement : UIElementPropertyBase
    {
        public string itemClass = "";
        public string itemSkin = "";
        public int itemWidth = 50;
        public int itemHeight = 50;

        public UIPageListPropertyElement(string key, string name) : base(key, name)
        {
            
            this.import_str = "";
            this.define_str = "public PageList {0};";

            this.property_str = "";
        }

        public override string outputDefine()
        {
            //不声明在code中
            return "";
        }

        public override string outputProperty()
        {
            return "";
        }

        public override string outputAdvDefine()
        {
            return base.outputDefine();
        }

        public override string outputAdvProperty()
        {
            string str_Factory = "{0}var {1}SkinFactory = new SkinFactory<{2}>({3});\r\n";
            string str_list = "            {0}{1} = new PageList({1}SkinFactory,{2},{3});\r\n";
            string str_skin = "            {0}{1}.skin = getGameObject(\"{1}\");\r\n";
            string prefix = "";

            if (string.IsNullOrEmpty(itemClass))
            {
                prefix = "//";
            }

            return StringUtil.substitute(str_Factory, prefix, fullName, itemClass, itemSkin) +
                        StringUtil.substitute(str_list, prefix, fullName, itemWidth, itemHeight) +
                        StringUtil.substitute(str_skin, prefix, fullName);

        }
    }

    public class UISharedBgPropertyElement : UIElementPropertyBase
    {
        public UISharedBgPropertyElement(string key, string name) : base(key, name)
        {
            this.import_str = "";
            this.define_str = "public SharedPanelBackground {0};";
            this.property_str = "GameObject {0}_container = getGameObject(\"{0}\");\r\n            " +
                                "{0} = {0}_container.GetComponentInChildren<SharedPanelBackground>();";
        }

    }

}
