using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

using UnityEngine;

using EndlessExpedition.Managers;
using EndlessExpedition.Serialization;

using DG.Tweening;

namespace EndlessExpedition
{
    public enum GameState
    {
        Booting,
        Playing,
        Paused,
        MainMenu,
        LoadingNew,
        LoadingSave,
        StartingNew,
        StartingSave
    }

    public class Main : MonoBehaviour
    {

        //DebugOnly //save-2016-01-02_01-29-59-PM
        public string saveGameName = "null";

        private void Awake()
        {
            float startTime = Time.realtimeSinceStartup;
			CMD.Init(gameObject);

            if (saveGameName != "null" && saveGameName != "")
            {
                Savegame save = new Savegame();
                save.LoadSaveFolder(saveGameName);
                GameManager.saveDataContainer.saveGame = save;
                GameManager.saveDataContainer.state = SaveDataState.Load;
            }

            if (GameManager.saveDataContainer.state == SaveDataState.Load)
                GameManager.gameState = GameState.LoadingSave;
            else
                GameManager.gameState = GameState.LoadingNew;

            //Setup mod refs
            Entities.BuildingModules.BuildingModule.SearchModules();
            Entities.EntityBehaviourScript.SearchBehaviourScripts();

            //Load Managers
            ManagerInstance.OnLoad();

            //Start Managers
            ManagerInstance.OnStart();

            //DEBUG: move cam to middle of level
            int center = TerrainManager.WORLD_WIDTH * EndlessExpedition.Terrain.TerrainChunk.SIZE / 2 - 8;
            Camera.main.transform.position = new Vector3(center, center, -20);

            DOTween.Init();
            CMD.Log("Total Main boot time: " + (Time.realtimeSinceStartup - startTime) + "ms");
        }

        private void Update()
        {
            OnTick();
            if (GameManager.gameState == GameState.Playing)
            {
                OnUpdate();
                Timer.UPDATE_TIMERS();
            }
        }

        //Custom events
        private void OnTick()
        {
            ManagerInstance.OnTick();
        }

        private void OnUpdate()
        {
            ManagerInstance.OnUpdate();
        }

        public static Main instance
        {
            get
            {
                return GameObject.Find("Main").GetComponent<Main>();
            }
        }
    }

    public static class Extensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }

        public static List<Type> GetListOfType<T>() where T : class
        {
            List<Type> types = new List<Type>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                types.Add(type);
            }
            return types;
        }

        /// <summary>
        /// Quick cast to Vector2
        /// </summary>
        /// <param name="vector">The vector to cast</param>
        /// <returns>the parameter as Vector2</returns>
        public static Vector2 V2(this Vector3 vector)
        {
            return vector;
        }
    }

    public class Timer
    {
        private static List<Timer> TIMERS = new List<Timer>();
        public static void UPDATE_TIMERS()
        {
            for (int i = 0; i < TIMERS.Count; i++)
            {
                if(!TIMERS[i].m_paused)
                    TIMERS[i].UpdateTimer();
            }
        }

        public delegate void OnTimerCompleteEventHandler();
        public OnTimerCompleteEventHandler OnTimerCompleteEvent;

        private float m_duration;
        private float m_startTime;
        private float m_pausedTime;

        private bool m_done;
        private bool m_paused;

        public Timer(float duration, OnTimerCompleteEventHandler callback = null)
        {
            m_duration = duration;
            m_startTime = Time.time;
            m_done = false;

            if (callback != null)
                OnTimerCompleteEvent += callback;

            TIMERS.Add(this);
        }
        ~Timer()
        {
            TIMERS.Remove(this);
        }

        public void Start()
        {
            m_paused = false;
            m_pausedTime = 0f;
            m_startTime = Time.time;
            m_done = false;

            if(!TIMERS.Contains(this))
                TIMERS.Add(this);
        }
        public void Stop()
        {
            if (TIMERS.Contains(this))
                TIMERS.Remove(this);
        }
        public void TogglePause()
        {
            m_paused = !m_paused;
            if(m_paused)
                m_pausedTime = Time.time;
            else
                m_pausedTime = 0f;
        }
        public void Pause()
        {
            if (!m_paused)
            {
                m_paused = true;
                m_pausedTime = Time.time;
            }
        }
        public void Resume()
        {
            m_paused = false;
            if(m_pausedTime > 0f)
            {
                m_startTime += m_pausedTime - m_startTime;
            }
        }

        /// <summary>
        /// This method is called by the game engine, no need to call it yourself. You can if you want to, cheeky bastard.
        /// </summary>
        public void UpdateTimer()
        {
            if (Time.time >= m_startTime + m_duration)
            {
                m_done = true;
                if (OnTimerCompleteEvent != null)
                    OnTimerCompleteEvent();
            }
            else
            {
                m_done = false;
            }
        }

        public bool IsDone
        {
            get
            {
                return Time.time >= m_startTime + m_duration ? true : false;
            }
        }
        public float Duration
        {
            get
            {
                return m_duration;
            }
        }
    }
}
