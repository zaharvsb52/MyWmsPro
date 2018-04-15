using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using wmsMLC.General.BL;
using wmsMLC.General.Services.Telegrams;

namespace wmsMLC.General.Services.Service.Telegrams
{
    public class RepoAnswerTelegramWrapper : BaseTelegrammWrapper
    {
        public RepoAnswerTelegramWrapper()
        {
            TransportTelegram.Type = TelegramType.Answer;
            TransportTelegram.Content = new Node { Name = "ANS", Value = "RepoAnswer" };
        }

        public RepoAnswerTelegramWrapper(Telegram queryTelegram) : this()
        {
            TransportTelegram.ToId = queryTelegram.FromId;
            TransportTelegram.TransactionNumber = queryTelegram.TransactionNumber;
        }

        public override Telegram UnWrap()
        {
            return TransportTelegram;
        }

        public void AddExceptionNode(Exception ex)
        {
            var errorNode = new Node { Name = Node.ErrorNodeName };
            errorNode.Nodes.Add(CreateExceptionNode(ex));
            TransportTelegram.Content.Nodes.Add(errorNode);
        }

        public void AddStatisticNode(string statisticValue)
        {
            var staticticsnode = new Node { Name = Node.SectionNameStatictics };
            staticticsnode.Nodes.Add(new Node
            {
                Name = Node.LastQueryExecutionTimeNodeName,
                Value = statisticValue
            });
            TransportTelegram.Content.Nodes.Add(staticticsnode);
        }

        public void ProcessResults(string requestedEntity, object result, object[] args, MethodInfo methodInfo, ParameterInfo[] methodParams)
        {
            // обрабатываем результаты
            var parametersNode = new Node { Name = Node.ParametersNodeName };

            // результат вызова метода
            var methodReturnType = methodInfo.ReturnType;
            if (methodReturnType != typeof(void))
            {
                var resultNode = new Node { Name = Node.ResultNodeName };

                if (IsDictionaryType(methodReturnType))
                {
                    if (result != null)
                        resultNode.Value = GetValueForTelegram(result);
                }
                else if (IsCollectionType(methodReturnType))
                {
                    foreach (var item in (IEnumerable)result)
                    {
                        resultNode.Nodes.Add(new Node { Name = requestedEntity, Value = GetValueForTelegram(item) });
                    }
                }
                else
                {
                    if (result != null)
                        resultNode.Value = GetValueForTelegram(result);
                }
                //if (resultNode.Nodes.Count > 0)
                parametersNode.Nodes.Add(resultNode);
            }

            // проверяем out param-ы
            for (var i = 0; i < methodParams.Length; i++)
            {
                if (!methodParams[i].IsOut && !methodParams[i].ParameterType.IsByRef)
                    continue;

                var pNode = new Node { Name = methodParams[i].Name };
                var argval = args[i];
                if (argval != null && IsCollectionType(argval.GetType()))
                {
                    foreach (var item in (IEnumerable) argval)
                    {
                        pNode.Nodes.Add(new Node {Name = requestedEntity, Value = GetValueForTelegram(item)});
                    }
                }
                else
                {
                    var value = argval.To<string>();
                    if (args[i] is IXmlSerializable)
                        value = XmlDocumentConverter.ConvertFrom(args[i]).InnerXml;
                    pNode.Value = value;
                }
                parametersNode.Nodes.Add(pNode);
            }

            if (parametersNode.Nodes.Count > 0)
                TransportTelegram.Content.Nodes.Add(parametersNode);
        }

        private static bool IsCollectionType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type)
                && type != typeof(string)
                && type != typeof(XmlDocument);
        }

        private static bool IsDictionaryType(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type)
                && type != typeof(string)
                && type != typeof(XmlDocument);
        }

        private static string GetValueForTelegram(object item)
        {
            if (item is IXmlSerializable)
                return XmlDocumentConverter.ConvertFrom(item).InnerXml;

            if (item is XmlDocument)
                return ((XmlDocument)item).InnerXml;

            //if (item.GetType().IsAssignableFrom(typeof(double)))
            //    return ((double)item).ToString("R");

            //return item.ToString();

            return SerializationHelper.GetCorrectStringValue(item);
        }

        private static Node CreateExceptionNode(Exception ex)
        {
            var exNode = new Node { Name = Node.ExceptionNodeName, Value = ex.GetType().AssemblyQualifiedName };
            var exMessage = new Node { Name = Node.ExceptionMessageNodeName, Value = ex.Message };
            var exStackTrace = new Node { Name = Node.ExceptionStackTraceNodeName, Value = ex.StackTrace };
            exNode.Nodes.AddRange(new[] { exMessage, exStackTrace });

            if (ex.InnerException != null)
            {
                var exInner = new Node { Name = Node.ExceptionInnerNodeName };
                exInner.Nodes.Add(CreateExceptionNode(ex.InnerException));
                exNode.Nodes.Add(exInner);
            }
            return exNode;
        }
    }
}