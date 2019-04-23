using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class PlayParticleSystemEditor
    {
        public BaseEditorWindow parentEditorWindow;
        private List<ParticleSystem> particleSystems;
        private float _runningTime;
        private GameObject go;

        private float m_LastPsTime = -1f;
        private float m_LastTime = -1f;
        public void Play(ParticleSystem particleSystem,GameObject go)
        {
            this.go = go;
            var list = new List<ParticleSystem>();
            list.Add(particleSystem);
            this.m_LastTime = -1f;

            Play(list,go);
        }

        public void Play(List<ParticleSystem> list,GameObject go)
        {
            this.go = go;
            if (particleSystems != null)
            {
                foreach (ParticleSystem item in particleSystems)
                {
                    item.Simulate(0);
                }
            }

            this.particleSystems = list;
            
            _runningTime = 0;

            EditorTickManager.Add(update);
        }

        public void Stop()
        {
            EditorTickManager.Remove(update);

            if (particleSystems != null && go!=null)
            {
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.Stop();
                }
            }
            _runningTime = 0;
            particleSystems = null;
        }
        private void update(float deltaTime)
        {
            if (parentEditorWindow != null)
            {
                parentEditorWindow.Repaint();
            }

            if (Application.isPlaying)
            {
                return;
            }
            if (particleSystems == null || go == null)
            {
                Stop();
                return;
            }

         
            _runningTime += deltaTime;
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem == null || particleSystem.gameObject==null)
                {
                    continue;
                }

                if (Mathf.Approximately(this.m_LastTime, -1f) || !Mathf.Approximately(this.m_LastTime, _runningTime))
                {
                    float num2 = Time.fixedDeltaTime * 0.5f;
                    float t = _runningTime;
                    float num4 = t - this.m_LastTime;
                    if ((((t < this.m_LastTime) || (t < num2)) || (Mathf.Approximately(this.m_LastTime, -1f) || (num4 > particleSystem.main.duration))) || !Mathf.Approximately(this.m_LastPsTime, particleSystem.time))
                    {
                        particleSystem.Simulate(0f, true, true);
                        particleSystem.Simulate(t, true, false);
                    }
                    else
                    {
                        float num5 = t % particleSystem.main.duration;
                        float num6 = num5 - particleSystem.time;
                        if (num6 < -num2)
                        {
                            num6 = (num5 + particleSystem.main.duration) - particleSystem.time;
                        }
                        particleSystem.Simulate(num6, true, false);
                    }
                    this.m_LastPsTime = particleSystem.time;
                    this.m_LastTime = _runningTime;
                }
            }
        }
    }
}