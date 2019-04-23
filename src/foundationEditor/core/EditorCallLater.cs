using System;
using System.Collections.Generic;
using foundation;
using UnityEditor;

namespace foundationEditor
{

    public class EditorCallLater
    {
        private static List<ActionNode<float>> updateQueue;

        static EditorCallLater()
        {
            updateQueue = new List<ActionNode<float>>();
        }

        public static void Add(Action handler, float delayTime = 0.016f)
        {
            if (delayTime <= 0)
            {
                handler();
                return;
            }
            if (delayTime < 0.016f)
            {
                delayTime = 0.016f;
            }

            float t = (float) EditorTickManager.getTimer() + delayTime;
            foreach (ActionNode<float> node in updateQueue)
            {
                if (node.action == handler)
                {
                    node.data = t;
                    return;
                }
            }

            ActionNode<float> actionNode = new ActionNode<float>();
            actionNode.action = handler;
            actionNode.data = t;

            updateQueue.Add(actionNode);

            if (updateQueue.Count > 0)
            {
                EditorTickManager.Add(Update);
            }
        }

        private static List<ActionNode<float>> todoList=new List<ActionNode<float>>();
        private static void Update(float deltaTime)
        {
            float c = (float)EditorTickManager.getTimer();
            foreach (ActionNode<float> node in updateQueue)
            {
                if (node.data > c)
                {
                    node.action();
                    todoList.Add(node);
                }
            }

            if (todoList.Count > 0)
            {
                foreach (ActionNode<float> node in todoList)
                {
                    updateQueue.Remove(node);
                }

                if (updateQueue.Count < 1)
                {
                    EditorTickManager.Remove(Update);
                }
            }
        }

        public static void Remove(Action handler)
        {
            int index = -1;
            int i = 0;
            foreach (ActionNode<float> node in updateQueue)
            {
                if (node.action == handler)
                {
                    index = i;
                    break;
                }
                i++;
            }
            if (index != -1)
            {
                updateQueue.RemoveAt(index);
            }

            if (updateQueue.Count < 1)
            {
                EditorTickManager.Remove(Update);
            }
        }
    }
}