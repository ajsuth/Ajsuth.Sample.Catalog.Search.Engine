// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InitializeExtendedSellableItemIndexingViewBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Ajsuth.Sample.Catalog.Search.Engine.Extensions;
using Ajsuth.Sample.Catalog.Search.Engine.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Search;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;

namespace Ajsuth.Sample.Catalog.Search.Engine.Pipelines.Blocks
{
    public class InitializeExtendedSellableItemIndexingViewBlock : SyncPipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override EntityView Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            // 1. Validation
            Condition.Requires(arg, nameof(arg)).IsNotNull();
            Condition.Requires(context, nameof(context)).IsNotNull();

            var argument = context.CommerceContext.GetObjects<SearchIndexMinionArgument>().FirstOrDefault();
            if (string.IsNullOrEmpty(argument?.Policy?.Name))
            {
                return arg;
            }

            // 2. Prepare Entities
            var entityItems = argument.Entities?.OfType<SellableItem>().ToList();
            if (entityItems == null || !entityItems.Any())
            {
                return arg;
            }

            // 3. Prepare custom properties
            var scopeIndexablePropertiesPolicy = IndexablePropertiesPolicy.GetPolicyByScope(context.CommerceContext, context.CommerceContext.Environment, argument.Policy.Name);
            if (scopeIndexablePropertiesPolicy?.ComposerProperties == null || !scopeIndexablePropertiesPolicy.ComposerProperties.Any())
            {
                return arg;
            }

            var searchViewNames = context.GetPolicy<KnownSearchViewsPolicy>();
            var childViews = arg.ChildViews.OfType<EntityView>().ToList();

            // 4. Iterate over each entity
            foreach (var si in argument.Entities.OfType<SellableItem>())
            {
                // 5. Get existing document entity view
                var documentView = childViews.First(v => v.EntityId.EqualsOrdinalIgnoreCase(si.Id)
                    && v.Name.EqualsOrdinalIgnoreCase(searchViewNames.Document));

                // 6. Add custom fields
                AddComposerFields(si, documentView, scopeIndexablePropertiesPolicy);
            }

            return arg;
        }

        protected void AddComposerFields(SellableItem si, EntityView documentView, IndexablePropertiesPolicy scopeIndexablePropertiesPolicy)
        {
            // 1. Iterate over each composer template configuration
            foreach (var composerView in scopeIndexablePropertiesPolicy.ComposerProperties)
            {
                // 2. Get property value
                var composerEntityView = si.GetComposerViewFromName(composerView.Key);

                if (composerEntityView == null)
                {
                    continue;
                }

                // 3. Iterate over each custom property to index
                foreach (var property in composerView.Value)
                {
                    var value = composerEntityView.GetPropertyValue(property.PropertyName);

                    if (value == null)
                    {
                        continue;
                    }

                    // 4. Add property to index document
                    documentView.Properties.Add(new ViewProperty
                    {
                        Name = property.IndexFieldName,
                        RawValue = value
                    });
                }
            }
        }
    }
}
