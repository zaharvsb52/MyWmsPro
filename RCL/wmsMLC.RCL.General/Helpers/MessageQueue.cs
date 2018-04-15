using System;
using System.Collections.Generic;
using System.Linq;

namespace wmsMLC.RCL.General.Helpers
{
    public class MessageQueue<TEnum, TDataType>
    {
        private readonly Queue<Tuple<TEnum, TDataType>> _queue = new Queue<Tuple<TEnum, TDataType>>();
        private readonly Dictionary<TEnum, Action<TDataType>> _actions = new Dictionary<TEnum, Action<TDataType>>();

        private bool _isBusy;

        private void CheckQueue()
        {
            // if queue is busy, silently return
            if (_isBusy) 
                return;

            // else extract next message and send it, turning IsBusy flag on
            if (_queue.Count == 0) 
                return;
            var element = _queue.Dequeue();
            if (!_actions.ContainsKey(element.Item1)) 
                return;
            var action = _actions[element.Item1];
            if (action == null) 
                return;
            _isBusy = true;
            action(element.Item2);
        }

        #region Public
        /// <summary>
        /// Регистрирует действие для указанного типа события. 
        /// </summary>
        public void RegisterAction(TEnum type, Action<TDataType> action)
        {
            _actions[type] = action;
        }

        /// <summary>
        /// Добавляет в очередь новое событие.
        /// </summary>
        public void RegisterMessage(TEnum type, TDataType data, bool ischeckqueue = true)
        {
            _queue.Enqueue(new Tuple<TEnum, TDataType>(type, data));
            if (ischeckqueue) 
                CheckQueue();
        }

        /// <summary>
        /// Завершает обработку текущего события и создаёт новое, если очередь не пустая.
        /// </summary>
        public void Complete()
        {
            _isBusy = false;
            CheckQueue();
        }

        /// <summary>
        /// Наличие сообщений данного типа в очереди.
        /// </summary>
        public bool HasMessages(Func<TEnum, bool> equalshandler)
        {
            return equalshandler != null && _queue.Any(p => p != null && equalshandler(p.Item1));
        }

        public int Count(Func<TEnum, bool> equalshandler)
        {
            return equalshandler == null ? 0 : _queue.Count(p => p != null && equalshandler(p.Item1));
        }
        #endregion Public
    }
}
