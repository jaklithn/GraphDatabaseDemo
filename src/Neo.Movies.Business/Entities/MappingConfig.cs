using Utility.Extensions;

namespace Neo.Movies.Business.Entities
{
    /// <summary>
    /// Specification on how the incoming relation is to be interpreted and mapped to existing nodes.
    /// </summary>
    public class MappingConfig
    {
        private string _fromProperty;
        private string _toProperty;

        /// <summary>
        /// The name of the relation in Neo4j
        /// </summary>
        public string RelationName { get; set; }

        /// <summary>
        /// Label of the existing From node
        /// </summary>
        public string FromNode { get; set; }

        /// <summary>
        /// Name of the lookup property on the From node
        /// </summary>
        public string FromProperty
        {
            get => _fromProperty;
            set => _fromProperty = value.ToCamelCase();
        }

        /// <summary>
        /// Label of the existing To node
        /// </summary>
        public string ToNode { get; set; }

        /// <summary>
        /// Name of the lookup property on the To node
        /// </summary>
        public string ToProperty
        {
            get => _toProperty;
            set => _toProperty = value.ToCamelCase();
        }
    }
}
