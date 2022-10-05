// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexablePropertiesPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Ajsuth.Sample.Catalog.Search.Engine.Models;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ajsuth.Sample.Catalog.Search.Engine.Policies
{
    /// <summary>
    /// Defines the indexable properties policy
    /// </summary>
    /// <seealso cref="T:Sitecore.Commerce.Core.Policy" />
    public class IndexablePropertiesPolicy : Policy
    {

        /// <summary>
        /// Gets or sets the search scope policy name.
        /// </summary>
        /// <value>
        /// The search scope policy name.
        /// </value>
        public string SearchScopeName { get; set; }

        /// <summary>
        /// Gets or sets the composer properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public ConcurrentDictionary<string, List<IndexablePropertyModel>> ComposerProperties { get; set; }

        /// <summary>
        /// Gets the policy by scope.
        /// </summary>
        /// <param name="commerceContext">The commerce context.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="searchScopePolicyName">The search scope policy name.</param>
        /// <returns>An <see cref="IndexablePolicy"/></returns>
        public static IndexablePropertiesPolicy GetPolicyByScope(CommerceContext commerceContext, CommerceEnvironment environment, string searchScopePolicyName)
        {
            var indexablePolicy = environment.GetPolicies<IndexablePropertiesPolicy>().FirstOrDefault(p => p.SearchScopeName.Equals(searchScopePolicyName, StringComparison.OrdinalIgnoreCase));
            if (indexablePolicy == null)
            {
                commerceContext.Logger.LogDebug($"Indexable properties policy using Search Scope Policy with name '{searchScopePolicyName}' does not exist");
            }

            return indexablePolicy;
        }
    }
}
