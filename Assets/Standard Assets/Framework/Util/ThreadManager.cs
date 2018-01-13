
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WWFramework.Core;

namespace WWFramework.Util
{
    public class ThreadManager : Singleton<ThreadManager>
    {
        private const string ThreadManagerName = "ThreadManagerName";

        private List<Task> _taskList;

        #region 具体任务
        private class Task
        {
            public enum ThreadState
            {
                None,
                InProgress,
                Finished,
                Dead,
            }
            private ThreadState _state;
            public ThreadState State
            {
                get { return _state; }
            }

            private Action _onStart;
            private Action _onFinished;

            public Task(Action onStart, Action onFinished)
            {
                _state = ThreadState.None;

                _onStart = onStart;
                _onFinished = onFinished;
            }

            public void OnStart()
            {
                _state = ThreadState.InProgress;
                ThreadPool.QueueUserWorkItem(InProgress);
            }

            private void InProgress(object obj)
            {
                if (_onStart != null)
                {
                    _onStart();
                }
                _state = ThreadState.Finished;
            }

            public void OnFinished()
            {
                if (_onFinished != null)
                {
                    _onFinished();
                }
                _state = ThreadState.Dead;
            }
        }
        #endregion

        protected override void OnInit()
        {
            base.OnInit();

            _taskList = new List<Task>();

            if (Application.isPlaying)
            {
                Timer.Instance.AddTask(ThreadManagerName, f => Update());
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.update += Update;
#endif
            }
        }

        private void Update()
        {
            for (int i = _taskList.Count - 1; i >= 0; i--)
            {
                var task = _taskList[i];
                switch (task.State)
                {
                    case Task.ThreadState.None:
                        {
                            task.OnStart();
                            break;
                        }
                    case Task.ThreadState.Finished:
                        {
                            task.OnFinished();
                            break;
                        }
                    case Task.ThreadState.Dead:
                        {
                            _taskList.RemoveAt(i);
                            break;
                        }
                }
            }
        }

        public void AddTask(Action onStart, Action onFinished)
        {
            var task = new Task(onStart, onFinished);
            _taskList.Add(task);
        }
    }
}