// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexablePropertyModel.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Sample.Catalog.Search.Engine.Models
{
    public class IndexablePropertyModel
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the index field name.
        /// </summary>
        /// <value>
        /// The index field name.
        /// </value>
        public string IndexFieldName { get; set; }
    }
}
