using System;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace wmsMLC.General.BL.Validation
{
    [DebuggerDisplay("Count = {Count}, Description = {ToString()}")]
    public class ValidateErrors : IEnumerable<KeyValuePair<string, ValidateErrorsList>>
    {
        #region .  Variables  .
        private readonly Dictionary<string, ValidateErrorsList> _internalDictionary;
        private readonly string _rootName;
        #endregion

        public ValidateErrors(string rootName)
        {
            _rootName = rootName;
            _internalDictionary = new Dictionary<string, ValidateErrorsList>();
        }

        public void Clear()
        {
            _internalDictionary.Clear();
        }
        public void Remove(string key)
        {
            _internalDictionary.Remove(key);
        }
        public void Remove(string key, string description)
        {
            if (!_internalDictionary.ContainsKey(key) ||
                _internalDictionary[key] == null)
                return;

            _internalDictionary[key].RemoveByDescription(description);
            if (_internalDictionary[key].Count == 0)
                Remove(key);
        }
        public void Set(string key, ValidateErrorsList errors)
        {
            _internalDictionary.Remove(key);
            _internalDictionary.Add(key, errors);
        }
        public void Add(string key, ValidateError error)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (error == null)
                throw new ArgumentNullException("error");

            if (_internalDictionary.ContainsKey(key))
            {
                if (!_internalDictionary[key].Any(e => e.Equals(error)))
                    _internalDictionary[key].Add(error);
                //TODO: Хорошо бы ругаться на то, что в коллекции уже есть ошибка. Нужно проверить где у нас такое возможно
                //throw new ArgumentException(string.Format("Error '{0}' already exists", error));
            }
            else
            {
                _internalDictionary.Add(key, new ValidateErrorsList { error });
            }
        }

        public bool Contains(string key, string description)
        {
            return _internalDictionary.ContainsKey(key) &&
                   _internalDictionary[key] != null &&
                   _internalDictionary[key].Contains(description);
        }

        public int Count
        {
            get { return _internalDictionary.Count; }
        }

        public ValidateErrorsList this[string columnName]
        {
            get
            {
                return _internalDictionary.ContainsKey(columnName)
                    ? _internalDictionary[columnName]
                    : null;
            }
        }

        public string GetCriticalErrorDescription()
        {
            if (_internalDictionary.ContainsKey(_rootName))
                return _internalDictionary[_rootName].GetCriticalErrorDescription();

            var sb = new StringBuilder();
            foreach (var e in _internalDictionary)
                sb.AppendLine(string.Format("{0}: {1}", e.Key, e.Value.GetCriticalErrorDescription()));
            return sb.ToString();
        }

        public ValidateErrorLevel GetMaxErrorLevel()
        {
            if (_internalDictionary.Count == 0)
                return ValidateErrorLevel.None;

            return this.Max(i => i.Value.GetMaxErrorLevel());
        }

        public override string ToString()
        {
            if (_internalDictionary.Count == 0)
                return null;

            if (_internalDictionary.ContainsKey(_rootName))
                return _internalDictionary[_rootName].ToString();

            var sb = new StringBuilder();
            foreach (var e in _internalDictionary)
                sb.AppendLine(string.Format("{0}: {1}", e.Key, e.Value));
            return sb.ToString();
        }

        public ValidateErrorsList GetOrCreate(string name)
        {
            var list = this[name];
            if (list == null)
            {
                list = new ValidateErrorsList();
                _internalDictionary.Add(name, list);
            }
            return list;
        }

        #region .  IEnumerable  .

        public IEnumerator<KeyValuePair<string, ValidateErrorsList>> GetEnumerator()
        {
            return _internalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalDictionary.GetEnumerator();
        }

        #endregion
    }
}
