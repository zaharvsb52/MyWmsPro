#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsNode.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>08.11.2012 20:15:36</Date>
/// <Summary>Узел древовидной структуры с данными
/// Унифицированный контейнер для объектов системы
/// </Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;

namespace wmsMLC.General.Telegrams
{
    /// <summary>
    /// Узел древовидной структуры с данными
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class Node
    {
        /// <summary>
        /// Имя узла
        /// </summary>
        [XmlElement("Name")]
        [ProtoMember(1)]
        public string Name;

        /// <summary>
        /// Значение
        /// </summary>
        [XmlElement("Value")]
        [ProtoMember(2)]
        public string Value;

        /// <summary>
        /// Список дочерних узлов
        /// </summary>
        [XmlArray("Tree")]
        [XmlArrayItem("Node")]
        [ProtoMember(3)]
        public List<Node> Nodes;

        /// <summary>
        /// Конструктор
        /// </summary>
        public Node()
        {
            Nodes = new List<Node>();
        }
    }
}
