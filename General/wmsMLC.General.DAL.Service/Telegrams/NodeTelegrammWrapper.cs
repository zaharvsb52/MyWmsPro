using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using wmsMLC.General.BL;
using wmsMLC.General.Resources;
using wmsMLC.General.Services;
using wmsMLC.General.Services.Telegrams;

namespace wmsMLC.General.DAL.Service.Telegrams
{
    public class NodeTelegrammWrapper : BaseTelegrammWrapper
    {
        #region .  Properties  .

        public string EntityName { get; set; }
        public string ActionName { get; set; }
        public TransmitterParam[] Parameters { get; set; }

        #endregion

        #region .  Constructors  .

        public NodeTelegrammWrapper(string entityName, string actionName, TransmitterParam[] parameters)
        {
            EntityName = entityName;
            ActionName = actionName;
            Parameters = parameters;
        }

        #endregion

        #region .  Methods  .

        public override Telegram UnWrap()
        {
            // TODO: переносить все поля!
            var telegram = base.UnWrap();

            var node = new Node {Name = EntityName, Value = ActionName};
            if (Parameters != null) // параметров может и не быть в системной телеграмме
                foreach (var p in Parameters)
                    node.Nodes.Add(ValueToNode(p.Name, p.Value));

            telegram.Content.Nodes.Add(node);

            return telegram;
        }

        public override void FillBy(Telegram telegram)
        {
            CheckOnException(telegram);

            var paramsNode = GetNodeByName(telegram.Content.Nodes, Node.ParametersNodeName);

            if (paramsNode == null && Parameters.Any(i => i.IsOut))
                throw new DeveloperException("Waiting result parameters but not recieved.");

            if (paramsNode != null && !Parameters.Any(i => i.IsOut))
                throw new DeveloperException("Recieved result paramters but not wait them.");

            // бежим по ожидаемым результатам
            foreach (var param in Parameters.Where(i => i.IsOut))
            {
                //var paramNode = paramsNode.Nodes.FirstOrDefault(i => param.Name.EqIgnoreCase(i.Name));
                var paramNode = GetNodeByName(paramsNode.Nodes, param.Name);
                if (paramNode == null)
                    throw new DeveloperException("Waiting node {0} but not recieved.", param.Name);

                param.Value = GetValueFromNode(paramNode, param.Type);
            }

            UpdateLastQueryExecutionTime(telegram);
        }

        protected Node ValueToNode(string valueName, object value)
        {
            var paramNode = new Node {Name = valueName};

            if (value is IXmlSerializable)
            {
                var xmlDoc = XmlDocumentConverter.ConvertFrom(value);
                paramNode.Value = xmlDoc.InnerXml;
                xmlDoc.RemoveAll();
            }
            else if (value is XmlDocument)
            {
                var xmlDoc = (XmlDocument) value;
                paramNode.Value = xmlDoc.InnerXml;
                xmlDoc.RemoveAll();
            }
            else if (value is IEnumerable && value.GetType() != typeof (string))
            {
                var items = (IEnumerable) value;
                foreach (var item in items)
                    paramNode.Nodes.Add(ValueToNode(null, item));
            }
            else
            //TODO: притянуть культуру и договоренности к разным типам
                paramNode.Value = value == null ? null : SerializationHelper.GetCorrectStringValue(value);

            return paramNode;
        }

        private object GetValueFromNode(Node node, Type valueType)
        {
            if (valueType.IsValueType || (typeof (string) == valueType))
            {
                // хитро определяем, что значения нет
                if (node.Nodes == null)
                    return valueType.IsByRef ? null : Activator.CreateInstance(valueType);

                return SerializationHelper.ConvertToTrueType(node.Value, valueType);
            }

            if (typeof (IEnumerable).IsAssignableFrom(valueType) && valueType != typeof (XmlDocument) && !typeof(IDictionary).IsAssignableFrom(valueType))
            {
                var itemType = valueType.IsGenericType ? valueType.GetGenericArguments()[0] : typeof (string);
                var listType = valueType.IsInterface ? typeof (List<>).MakeGenericType(itemType) : valueType;
                var list = (IList) Activator.CreateInstance(listType);
                foreach (var n in node.Nodes)
                {
                    list.Add(GetValueFromNode(n, itemType));
                }

                return list;
            }

            // если сложный объект, то разбираем его сложно
            if (typeof (IXmlSerializable).IsAssignableFrom(valueType) && (node.Value != null || node.Nodes != null))
            {
                if (node.Value != null)
                {
                    using (var sr = new StringReader(node.Value))
                    using (var xmlReader = XmlReader.Create(sr))
                        return XmlDocumentConverter.ConvertTo(valueType, xmlReader);
                }
            }

            if (valueType == typeof (XmlDocument))
            {
                if (string.IsNullOrEmpty(node.Value))
                    return null;
                var doc = new XmlDocument();
                doc.LoadXml(node.Value);
                return doc;
            }
            return node.Value;
        }

        private void CheckOnException(Telegram telegram)
        {
            // проверяем наличие ошибки
            var errNode = GetNodeByName(telegram.Content.Nodes, Node.ErrorNodeName);
            if (errNode == null)
                return;

            var ex = GetExceptionFromNode(errNode.Nodes);
            if (ex != null)
                throw ex;
        }

        private Exception GetExceptionFromNode(List<Node> nodes)
        {
            // вытаскиваем ноду ошибки
            var exceptionNode = GetNodeByName(nodes, Node.ExceptionNodeName);
            if (exceptionNode == null)
                throw new DeveloperException("Error node exists but exception node not.");

            // вытаскиваем тип
            var exceptionType = exceptionNode.Value;
            if (string.IsNullOrEmpty(exceptionType))
                throw new DeveloperException("Exception type information is empty.");

            // вытаскиваем сообщение
            var exceptionMessage = GetNodeByName(exceptionNode.Nodes, Node.ExceptionMessageNodeName);
            var message = exceptionMessage.Value;

            Exception innerException = null;
            var inner = GetNodeByName(exceptionNode.Nodes, Node.ExceptionInnerNodeName);
            if (inner != null)
                innerException = GetExceptionFromNode(inner.Nodes);

            var exType = System.Type.GetType(exceptionType, false);

            // если другой детализации нет - генерим Developer
            if (exceptionNode.Nodes == null || exceptionNode.Nodes.Count == 0 || exType == null || !typeof(BaseException).IsAssignableFrom(exType))
                return new DeveloperException(message, innerException);

            // создаем ошибку
            try
            {
                return (Exception)Activator.CreateInstance(exType, message, innerException);
            }
            catch (Exception ex)
            {
                throw new DeveloperException(string.Format("Can't create instance '{0}' of custom exception.", exceptionType), ex);
            }
        }

        private Node GetNodeByName(IEnumerable<Node> nodes, string nodename)
        {
            if (nodes == null)
                return null;

            return nodes.FirstOrDefault(i => nodename.EqIgnoreCase(i.Name));
        }

        private void UpdateLastQueryExecutionTime(Telegram telegram)
        {
            LastQueryExecutionTime = 0;
            var staticticsnode = GetNodeByName(telegram.Content.Nodes, Node.SectionNameStatictics);
            if (staticticsnode == null)
                return;

            var lastQueryExecutionTimeNode = GetNodeByName(staticticsnode.Nodes, Node.LastQueryExecutionTimeNodeName);
            if (lastQueryExecutionTimeNode == null)
                return;

            double lastQueryExecutionTime;
            if (double.TryParse(lastQueryExecutionTimeNode.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out lastQueryExecutionTime))
                LastQueryExecutionTime = lastQueryExecutionTime;
        }

        #endregion
    }
}
