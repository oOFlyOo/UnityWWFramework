
using System;
using System.Collections.Generic;
using UnityEngine;
using WWFramework.Core;

namespace WWFramework.Util
{
    /// <summary>
    /// 所有时间，包括 Coroutine 都可以走这里
    /// </summary>
    public class Timer : MonoSingleton<Timer>
    {
        public static readonly YieldInstruction WaitForEndOfFrame = new WaitForEndOfFrame();
        public static readonly YieldInstruction WaitForFixedUpdate = new WaitForFixedUpdate();

        private List<Task> _taskList;

#region 具体任务
        private class Task
        {
            private string _name;
            protected bool _isValid;

            private bool _timeScale;

            protected Action<float> _onUpdated;

            public string Name
            {
                get { return _name; }
            }

            public bool IsValid
            {
                get { return _isValid; }
            }

            public Task(string name, bool timeScale, Action<float> onUpdated)
            {
                _isValid = true;

                _name = name;
                _timeScale = timeScale;
                _onUpdated = onUpdated;
            }

            protected float GetRealTime(float deltaTime, float unscaledDeltaTime)
            {
                return _timeScale? deltaTime : unscaledDeltaTime;
            }


            public virtual void PassTime(float deltaTime, float unscaledDeltaTime)
            {
                if (_onUpdated != null)
                {
                    OnUpdate(GetRealTime(deltaTime, unscaledDeltaTime));
                }
            }

            protected virtual void OnUpdate(float realTime)
            {
                _onUpdated(realTime);
            }

            public bool Cancel()
            {
                var valid = _isValid;
                _isValid = false;

                return valid;
            }
        }

        private class CdTask: Task
        {
            private float _interval;
            private float _lastActiveTime;

            public CdTask(string name, bool timeScale, Action<float> onUpdated, float interval) : base(name, timeScale, onUpdated)
            {
                _lastActiveTime = 0;

                _interval = interval;
            }

            protected override void OnUpdate(float realTime)
            {
                _lastActiveTime += realTime;
                if (_lastActiveTime >= _interval)
                {
                    _lastActiveTime = 0;
                    base.OnUpdate(realTime);
                }
            }
        }


        private class DuractionTask: CdTask
        {
            private float _remainTime;
            private Action _onFinshed;

            public DuractionTask(string name, bool timeScale, Action<float> onUpdated, float duraction, float remainTime, Action onFinshed) : base(name, timeScale, onUpdated, duraction)
            {
                _remainTime = remainTime;
                _onFinshed = onFinshed;
            }

            public override void PassTime(float deltaTime, float unscaledDeltaTime)
            {
                base.PassTime(deltaTime, unscaledDeltaTime);

                _remainTime -= GetRealTime(deltaTime, unscaledDeltaTime);
                if (_remainTime <= 0)
                {
                    if (_onFinshed != null)
                    {
                        _onFinshed();
                    }

                    _isValid = false;
                }
            }
        }
        #endregion

        protected override void OnInit()
        {
            base.OnInit();

            _taskList = new List<Task>();
        }

        private void AddTask(Task task)
        {
            _taskList.Add(task);
        }

        private Task GetFirstMatchValidTask(string name)
        {
            foreach (var task in _taskList)
            {
                if (task.Name == name && task.IsValid)
                {
                    return task;
                }
            }

            return null;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var unscaledDeltaTime = Time.unscaledDeltaTime;

            // 这里是倒叙的，为了方便而已，所以不要以为先 add 的先执行
            for (int i = _taskList.Count - 1; i >= 0; i--)
            {
                if (_taskList[i].IsValid)
                {
                    _taskList[i].PassTime(deltaTime, unscaledDeltaTime);
                }

                if (!_taskList[i].IsValid)
                {
                    _taskList.RemoveAt(i);
                }
            }
        }


        public void AddTask(string name, Action<float> onUpdate, bool timesScale = false)
        {
            AddTask(new Task(name, timesScale, onUpdate));
        }


        public void AddTask(string name, float interval, Action<float> onUpdate, bool timeScale = false)
        {
            AddTask(new CdTask(name, timeScale, onUpdate, interval));
        }

        public void AddTask(string name, float duraction, Action onFinish,
            bool timeScale = false)
        {
            AddTask(name, 0, null, duraction, onFinish, timeScale);
        }

        public void AddTask(string name, float interval, Action<float> onUpdate, float duraction, Action onFinish,
            bool timeScale = false)
        {
            AddTask(new DuractionTask(name, timeScale, onUpdate, interval, duraction, onFinish));
        }

        public bool RemoveTask(string name)
        {
            var task = GetFirstMatchValidTask(name);
            if (task != null)
            {
                task.Cancel();

                return true;
            }

            return false;
        }
    }
}