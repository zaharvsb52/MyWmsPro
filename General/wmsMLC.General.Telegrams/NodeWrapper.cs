using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;
using wmsMLC.General.Types;

namespace wmsMLC.General.Telegrams
{
    /// <summary> Обертка над универсальным объектом </summary>
    [Serializable]
    [ProtoContract]
    public class NodeWrapper
    {
        #region .  Properties  .

        /// <summary> Имя узла </summary>
        [XmlElement("Name")]
        [ProtoMember(1)]
        public string Name
        {
            get { return _node.Name; }
            set { _node.Name = value; }
        }

        /// <summary> Значение </summary>
        [XmlElement("Value")]
        [ProtoMember(2)]
        public string Value
        {
            get { return _node.Value; }
            set { _node.Value = value; }
        }

        /// <summary> Список дочерних узлов </summary>
        [XmlArray("Tree")]
        [XmlArrayItem("Node")]
        [ProtoMember(3)]
        public List<NodeWrapper> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        #endregion

        private Node _node;
        private List<NodeWrapper> _nodes;

        public NodeWrapper(Node node)
        {
            _node = node;
            Nodes = new List<NodeWrapper>();
        }
    }
}