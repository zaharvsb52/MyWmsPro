#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="Serializator.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>16.07.2012 14:57:09</Date>
/// <Summary>Сериализатор-десериализатор объектов и xml файлов</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Collections.Generic;
//using System.Xml.Serialization;
using System.IO;
using wmsMLC.General.Resources;

namespace wmsMLC.General.Types
{
    /// <summary>
    /// Класс сериализация и десериализации xml файла
    /// </summary>
    /// <typeparam name="T">тип объекта приведения XML файла</typeparam>
    public class Serializator<T> : BaseObject
    {
        ///// <summary>
        ///// Событие записи сообщения в лог
        ///// </summary>
        //public event EventErrorHandler OnEventErrorHandler;

        /// <summary>
        /// Полное имя файла (XML документа) для сериализатора
        /// </summary>
        readonly string _fileName;

        /// <summary>
        /// Коллекция исключений
        /// </summary>
        readonly List<Exception> _listException = new List<Exception>();

        readonly List<string> _errorAttributes = new List<string>();

        /// <summary>
        /// Строка, со всем содержимым файла
        /// </summary>
        string _allTextFile = string.Empty;

        /// <summary>
        /// Конструктор класса сериализатора
        /// </summary>
        /// <param name="fileName">текстовое представление полного имя XML файла</param>
        /// <param name="linkById">привязка по уникальному номеру</param>
        public Serializator(string fileName, string linkById)
        {
            _fileName = fileName;
            LinkById = linkById;
        }

        public Serializator(string fileName, string hostName, string userName, string linkById)
        {
            _fileName = fileName;
            LinkById = linkById;
            HostName = hostName;
            UserName = userName;
        }

        /// <summary>
        /// Выполняется десериализация в указанный Object с помощью объекта XmlReader.
        /// </summary>
        /// <returns>возвращает объект указанного типа</returns>
        private T Deserialize()
        {
            try
            {
                var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(T));
                serialiser.UnknownNode += serializer_UnknownNode;
                serialiser.UnknownAttribute += serializer_UnknownAttribute;
                serialiser.UnknownElement += serialiser_UnknownElement;
                serialiser.UnreferencedObject += serialiser_UnreferencedObject;

                using (TextReader reader = new StreamReader(_fileName))
                {
                    var myObject = (T)serialiser.Deserialize(reader);
                    reader.Close();
                    return myObject;
                }

            }
            catch (Exception ex)
            {
                ProcessException(ex, businessProcess: "GENERAL_SERIALIZATOR_DESERIALIZE");
                _listException.Add(ex);
                return default(T);
            }
        }

        /// <summary>
        /// Чтение всех данных из файла в строку
        /// </summary>
        private void ReadAllText()
        {
            try
            {
                using (TextReader reader = new StreamReader(_fileName))
                {
                    _allTextFile = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ProcessException(ex, businessProcess: "GENERAL_SERIALIZATOR_READALLTEXT");
                _listException.Add(ex);
                _allTextFile = string.Empty;
            }
        }

        /// <summary>
        /// Возвращает коллекцию исключений
        /// </summary>
        /// <returns>Коллекция исключений</returns>
        public List<Exception> GetError()
        {
            return _listException;
        }

        /// <summary>
        /// Десериализация документа XML 
        /// </summary>
        public T Get()
        {
            if (File.Exists(_fileName))
                return Deserialize();

            ProcessException(new FileNotFoundException("Configuration file not found", _fileName), "EUnknown", new string[] { string.Format("File: {0} not exists", _fileName) }, "GENERAL_SERIALIZATOR_SET");
            _listException.Add(new FileNotFoundException(ExceptionResources.NotFileInDirectory, _fileName));
            return default(T);
        }

        /// <summary>
        /// Возввращает все содержимое файла
        /// </summary>
        /// <returns>текстовая строка  даннымис</returns>
        public string GetAllTextFile()
        {
            ReadAllText();
            return _allTextFile;
        }

        /// <summary>
        /// Сериализует указанный Object и записывает документ XML в файл с помощью заданного Stream.
        /// </summary>
        public bool Set(T myObject)
        {
            if (myObject != null)
            {
                try
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                    using (Stream writer = new FileStream(_fileName, FileMode.Create))
                    {
                        serializer.Serialize(writer, myObject);
                        writer.Close();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    ProcessException(ex, businessProcess: "GENERAL_SERIALIZATOR_SET");
                    _listException.Add(ex);
                }
            }
            return false;
        }

        private void serializer_UnknownNode(object sender, System.Xml.Serialization.XmlNodeEventArgs e)
        {
            var str = string.Format("Unknown Node={0}\t{1}", e.Name, e.Text);
#if DEBUG
            Log.Warn(str);
#endif
            _errorAttributes.Add(str);
        }

        private void serializer_UnknownAttribute(object sender, System.Xml.Serialization.XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            var str = string.Format("Unknown attribute={0} value={1}", attr.Name, attr.Value);
#if DEBUG
            Log.Warn(str);
#endif
            _errorAttributes.Add(str);
        }

        void serialiser_UnreferencedObject(object sender, System.Xml.Serialization.UnreferencedObjectEventArgs e)
        {
            var str = string.Format("Unreferenced Object={0} value={1}", e.UnreferencedId, e.UnreferencedObject);
#if DEBUG
            Log.Warn(str);
#endif
            _errorAttributes.Add(str);
        }

        void serialiser_UnknownElement(object sender, System.Xml.Serialization.XmlElementEventArgs e)
        {
            var str = string.Format("UnknownElement={0} expected={1} line={2} position={3} value={4}", e.Element, e.ExpectedElements, e.LineNumber, e.LinePosition, e.ObjectBeingDeserialized);
#if DEBUG
            Log.Warn(str);
#endif
            _errorAttributes.Add(str);
        }

        public List<string> GetErrorAttributes()
        {
            return _errorAttributes;
        }
    }
}