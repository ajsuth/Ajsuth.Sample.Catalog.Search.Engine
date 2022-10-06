// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Sample.Catalog.Search.Engine
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Search;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

                .ConfigurePipeline<IIncrementalIndexMinionPipeline>(pipeline => pipeline
                    .Add<Pipelines.Blocks.InitializeExtendedSellableItemIndexingViewBlock>().After<InitializeSellableItemIndexingViewBlock>()
                )

                .ConfigurePipeline<IFullIndexMinionPipeline>(pipeline => pipeline
                    .Add<Pipelines.Blocks.InitializeExtendedSellableItemIndexingViewBlock>().After<InitializeSellableItemIndexingViewBlock>()
                )

                .ConfigurePipeline<ISearchPipeline>(pipeline => pipeline
                    .Replace<Sitecore.Commerce.Plugin.Catalog.ProcessDocumentSearchResultBlock, Pipelines.Blocks.ProcessDocumentSearchResultBlock>()
                )
            );

            services.RegisterAllCommands(assembly);
        }
    }
}