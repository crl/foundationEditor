using System;
using foundation;
using gameSDK;

namespace foundationEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventUIAttribute : Attribute
    {
        public Type type;
        public int priority = 0;
        public EventUIAttribute(Type type,int priority = 0)
        {
            this.type = type;
            this.priority = priority;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EventStackAttribute : Attribute
    {
        public Type[] types;
        public EventStackAttribute(params Type[] types)
        {
            this.types = types;
        }
    }

    public class IEventUI
    {
        public virtual string label {
            get { return OnGetLabel();}
        }

        public virtual string OnGetLabel() { return "" ;}

        private ISkillEvent e;
        private EditorUI p;
        public virtual void createUI(ISkillEvent value, EditorUI p)
        {
            this.e = value;
            this.p = p;
        }

        protected void repaint()
        {
            if (p != null)
            {
                p.simpleDispatch(EventX.REPAINT);
            }
        }
    }
}