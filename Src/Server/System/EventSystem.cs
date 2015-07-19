using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Phoenix.Core.Networking;
using System.Threading;

namespace Server.Events
{
    public interface IEventRequester
    {
        object WhoIam{get;}
        bool isThere{get;}
    }
    class Event
    {
        public EventType Type;
        protected DateTime _startTime;
        protected DateTime _endTime;
        protected DateTime _lastExec;

        protected TimeSpan _interval;
        protected TimeSpan _notify;

        event Action<Game.Player> onNotify;

        Dictionary<IEventRequester, Action> _work;
        protected bool _repeating = false;

        public Event()
        {
            _work = new Dictionary<IEventRequester, Action>();
            _startTime = DateTime.Now;
        }
        public void onTick()
        {
            if (!_repeating &&  DateTime.Now > _endTime)
            {
            }
            else if (DateTime.Now > _lastExec.Add(_interval))
            {
                if (_notify != null)
                    if ((DateTime.Now - _startTime) >= _notify)
                        if (onNotify != null)
                            foreach (var p in _work.Keys.Where(c => c is Game.Player)) onNotify((Game.Player)p);

                Queue<Game.Player> dced = new Queue<Game.Player>(250);

                ParallelOptions op = new ParallelOptions();
                op.MaxDegreeOfParallelism = 1 + (_work.Count / 2);

                Parallel.ForEach(_work.ToList(), p =>
                {
                    if (p.Key is Game.Player)
                    {
                        if (!(p.Key as Game.Player).isThere)
                            dced.Enqueue((p.Key as Game.Player));
                        else
                            _work[p.Key]();
                    }
                });

                while (dced.Count > 0)
                    _work.Remove(dced.Dequeue());

                _lastExec = DateTime.Now;
            }
        }
        public TimeSpan Interval
        {
            get { return _interval; }
        }
        public bool HasRequests { get { return (_work.Count > 0); } }
        public int Count { get { return _work.Count; } }
        public void DoAdd(IEventRequester src, Action wrk)
        {
            if (!_work.ContainsKey(src))
                _work.Add(src, wrk);
        }
        public void DoRem(IEventRequester src)
        {
            if (_work.ContainsKey(src))
                _work.Remove(src);
        }
    }

    class TimedRepeatingEvent : Event
    {
        public TimedRepeatingEvent(int sec)
        {
            _interval = new TimeSpan(0, 0, sec-1);
            _repeating = true;
        }
        ~TimedRepeatingEvent()
        {
        }

        
    }

    public enum EventType
    {
        Player_Network,
    }
    public class EventSystem
    {
        List<Event> _events;
        Thread _main;

        public EventSystem()
        {
            _events = new List<Event>();
        }

        public void RegisterEvent(EventType typ, IEventRequester src, Action wrk)
        {
            switch (typ)
            {
                case EventType.Player_Network:
                    {
                        if (_events.Count > 0)
                        {
                            bool added = false;
                            foreach (var e in _events.Where(c => c.Type == typ && c.Count < 25))
                            {
                                e.DoAdd(src, wrk);
                                added = true;
                                break;
                            }
                            if (!added)
                            {
                                var tmp = new TimedRepeatingEvent(1);
                                tmp.Type = typ;
                                tmp.DoAdd(src, wrk);
                                _events.Add(tmp);
                            }
                        }
                        else
                        {
                            var tmp = new TimedRepeatingEvent(1);
                            tmp.Type = typ;
                            tmp.DoAdd(src, wrk);
                            _events.Add(tmp);
                        }
                    } break;
            }
        }


        public void Start()
        {
            _main = new Thread(new ThreadStart(wrk));
            _main.IsBackground = true;
            _main.Name = "EventSystem";
            _main.Start();
        }
        public void Stop()
        {
            if (_main.IsAlive)
                _main.Abort();
        }


        void wrk()
        {
            do
            {
                try
                {
                    foreach (var even in _events.ToList())
                    {
                        try
                        {
                            if (even.HasRequests)
                                even.onTick();
                            else
                                _events.Remove(even);
                        }
                        catch (Exception e) { DebugSystem.Write(new ExceptionData(e)); }
                    }
                }
                catch (Exception f) { DebugSystem.Write(new ExceptionData(f)); }
                Thread.Sleep(1);
            } while (true);
        }
    }
}
