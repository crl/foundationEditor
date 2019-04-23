namespace foundationEditor
{
    using foundation;
    using System;
    using UnityEditor;
    public class EditorTickManager
    {
        private static QueueHandle<float> updateQueue;
        private static double preTime = -1;
        private static float deltaTime;
        private static double timer;
        static EditorTickManager()
        {
            updateQueue=new QueueHandle<float>();
            EditorApplication.update += editorUpdate;
        }

        public static double getTimer()
        {
            return timer;
        }

        public static float getDeltaTime()
        {
            return deltaTime;
        }


        public static bool Add(Action<float> handle)
        {
            if (updateQueue.length == 0)
            {
                preTime = -1;
            }
            return updateQueue.___addHandle(handle,0);
        }

        public static bool Remove(Action<float> handle)
        {
            return updateQueue.___removeHandle(handle);
        }
        
        private static void editorUpdate()
        {
            timer = EditorApplication.timeSinceStartup;
            if (preTime == -1)
            {
                preTime = timer;
            }
            if (updateQueue.length == 0)
            {
                return;
            }
            
            deltaTime =(float)(timer- preTime);
            updateQueue.dispatch(deltaTime);
            preTime = timer;
        }
    }
}