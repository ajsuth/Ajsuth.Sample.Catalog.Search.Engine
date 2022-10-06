// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessDocumentSearchResultBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Commerce.Plugin.Views;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Ajsuth.Sample.Catalog.Search.Engine.Pipelines.Blocks
{
    /// <summary>
    /// Defines a block that processes a search result document.
    /// </summary>
    /// <seealso>
    ///     <cref>
    ///         Sitecore.Framework.Pipelines.PipelineBlock{Sitecore.Commerce.EntityViews.EntityView,
    ///         Sitecore.Commerce.EntityViews.EntityView, Sitecore.Commerce.Core.CommercePipelineExecutionContext}
    ///     </cref>
    /// </seealso>
    public class ProcessDocumentSearchResultBlock : SyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// Runs the specified argument.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// an <see cref="EntityView" /></returns>
        public override EntityView Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            var documents = context.CommerceContext.GetObjects<List<Document>>().FirstOrDefault();

            if (documents == null || !documents.Any() || !arg.HasPolicy<SearchScopePolicy>())
            {
                return arg;
            }

            var scopePolicy = arg.GetPolicy<SearchScopePolicy>();
            var indexablePolicy = IndexablePolicy.GetPolicyByScope(context.CommerceContext, context.CommerceContext.Environment, scopePolicy.Name);
            var retrievableProperties = (from p in indexablePolicy.Properties let pName = p.Key.ToLowerInvariant() where p.Value.IsRetrievable select pName).ToList();

            var hasTags = scopePolicy.ResultDetailsTags.Any(t => t.Name.EqualsOrdinalIgnoreCase("CatalogTable"));
            if (!hasTags)
            {
                return arg;
            }

            var viewPolicy = arg.GetPolicy<SearchViewPolicy>();
            if (viewPolicy != null && !string.IsNullOrEmpty(viewPolicy.Hint))
            {
                return arg;
            }

            foreach (var document in documents)
            {
                int? entityVersion = null;
                var docId = document[CoreConstants.EntityId.ToLowerInvariant()].ToString();
                if (int.TryParse(document[CoreConstants.EntityVersion.ToLowerInvariant()].ToString(), out var version))
                {
                    entityVersion = version;
                }

                var child = arg.ChildViews.OfType<EntityView>().FirstOrDefault(c =>
                    c.EntityId.Equals(docId, StringComparison.OrdinalIgnoreCase) && c.EntityVersion == entityVersion);
                if (child == null)
                {
                    continue;
                }

                var variantId = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(CatalogConstants.VariantId));
                if (variantId == null && retrievableProperties.Contains(CatalogConstants.VariantId, StringComparer.OrdinalIgnoreCase))
                {
                    variantId = new ViewProperty
                    {
                        Name = CatalogConstants.VariantId.ToLowerInvariant(),
                        Value = string.Empty,
                        RawValue = string.Empty,
                        OriginalType = typeof(string).FullName,
                        IsReadOnly = true
                    };
                }

                var variantDisplayName = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(CatalogConstants.VariantDisplayName));
                if (variantDisplayName == null && retrievableProperties.Contains(CatalogConstants.VariantDisplayName, StringComparer.OrdinalIgnoreCase))
                {
                    variantDisplayName = new ViewProperty
                    {
                        Name = CatalogConstants.VariantDisplayName.ToLowerInvariant(),
                        Value = string.Empty,
                        RawValue = string.Empty,
                        OriginalType = typeof(string).FullName,
                        IsReadOnly = true
                    };
                }

                var name = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(CoreConstants.Name));
                var displayName = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(CoreConstants.DisplayName));
                var updatedDate = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(CoreConstants.DateUpdated));
                var createdDate = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase(CoreConstants.DateCreated));

                // 1. Copy the field view property 
                var yearmanufactured = child.Properties.FirstOrDefault(p => p.Name.EqualsOrdinalIgnoreCase("YearManufactured"));

                // 2. Clear the results view properties
                child.Properties.Clear();

                if (name != null)
                {
                    name.UiType = ViewsConstants.EntityLinkUiType;
                }

                // 3. Add desired indexed fields back into results view
                child.AddViewPropertyToEntityViewOrDefault(name, retrievableProperties, CoreConstants.Name, typeof(string).FullName);
                child.AddViewPropertyToEntityViewOrDefault(displayName, retrievableProperties, CoreConstants.DisplayName, typeof(string).FullName);
                child.AddViewPropertyToEntityViewOrDefault(variantId, retrievableProperties, CatalogConstants.VariantId, typeof(string).FullName);
                child.AddViewPropertyToEntityViewOrDefault(variantDisplayName, retrievableProperties, CatalogConstants.VariantDisplayName, typeof(string).FullName);
                child.AddViewPropertyToEntityViewOrDefault(createdDate, retrievableProperties, CoreConstants.DateCreated, typeof(DateTimeOffset).FullName);
                child.AddViewPropertyToEntityViewOrDefault(updatedDate, retrievableProperties, CoreConstants.DateUpdated, typeof(DateTimeOffset).FullName);
                child.AddViewPropertyToEntityViewOrDefault(yearmanufactured, retrievableProperties, "YearManufactured", typeof(string).FullName);
            }

            return arg;
        }
    }
}
