using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace LoveNes
{
    public class Clock : IDisposable
    {
        private readonly List<IClockSink> _clockSinks;
        private readonly List<IClockSink> _clock3Sinks;

        private readonly IObservable<long> _masterClockObservable;
        private IDisposable _ticker;

        public Clock(long masterClock = 21477272)
        {
            _clockSinks = new List<IClockSink>();
            _clock3Sinks = new List<IClockSink>();
            _masterClockObservable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromTicks(10000000 / masterClock));
        }

        /// <summary>
        /// 添加时钟终端
        /// </summary>
        /// <param name="sink">终端</param>
        public void AddSink(IClockSink sink)
        {
            _clockSinks.Add(sink);
        }

        /// <summary>
        /// 添加 3 倍时钟终端
        /// </summary>
        /// <param name="sink">终端</param>
        public void Add3TimesSink(IClockSink sink)
        {
            _clock3Sinks.Add(sink);
        }

        /// <summary>
        /// 上电
        /// </summary>
        public void PowerUp()
        {
            short counter = 0;
            _clockSinks.ForEach(o => o.OnPowerUp());
            _clock3Sinks.ForEach(o => o.OnPowerUp());

            _ticker = _masterClockObservable.Subscribe(_ =>
              {
                  switch (counter)
                  {
                      case 0:
                          _clockSinks.AsParallel().ForEach(o => o.OnTick());
                          _clock3Sinks.AsParallel().ForEach(o => o.OnTick());
                          counter++;
                          break;
                      case 4:
                      case 8:
                          _clock3Sinks.AsParallel().ForEach(o => o.OnTick());
                          counter++;
                          break;
                      case 11:
                          counter = 0;
                          break;
                      default:
                          counter++;
                          break;
                  }
              });
        }

        /// <summary>
        /// 复位
        /// </summary>
        public void Reset()
        {
            _clockSinks.ForEach(o => o.OnReset());
            _clock3Sinks.ForEach(o => o.OnReset());
        }

        public void Dispose()
        {
            _ticker?.Dispose();
        }
    }

    /// <summary>
    /// 时钟终端
    /// </summary>
    public interface IClockSink
    {
        /// <summary>
        /// Tick
        /// </summary>
        void OnTick();

        /// <summary>
        /// 上电
        /// </summary>
        void OnPowerUp();

        /// <summary>
        /// 复位
        /// </summary>
        void OnReset();
    }
}