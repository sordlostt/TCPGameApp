using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic.Utils
{
    public class Timer
    {
        public delegate void OnTimeElapsed();
        public OnTimeElapsed TimeElapsed;

        public System.Diagnostics.Stopwatch stopWatch;
        public float interval;
        public float elapsed;

        public bool raiseEvents;

        public async void Start()
        {
            stopWatch = new System.Diagnostics.Stopwatch();
            raiseEvents = true;
            stopWatch.Start();
            await Task.Run(() => Update());
            if (raiseEvents)
            {
                TimeElapsed?.Invoke();
            }
        }

        private void Update()
        {
            while (true)
            {
                elapsed = stopWatch.ElapsedMilliseconds / 1000.0f;

                if (elapsed >= interval) break;
            }
        }
    }
}
