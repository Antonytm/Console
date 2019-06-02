﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.Publish;
using Spe.Core.Extensions;
using Spe.Core.Utility;
using Spe.Core.Validation;

namespace Spe.Commands.Data
{
    [Cmdlet(VerbsData.Publish, "Item", SupportsShouldProcess = true)]
    [OutputType(new Type[] { }, ParameterSetName = new[] { "Item from Pipeline", "Item from Path", "Item from ID" })]
    public class PublishItemCommand : BaseItemCommand
    {
        [Parameter]
        public SwitchParameter Recurse { get; set; }

        [Parameter]
        [Alias("Targets")]
        [AutocompleteSet(nameof(Databases))]
        public string[] Target { get; set; }

        [Parameter]
        public PublishMode PublishMode { get; set; } = PublishMode.Smart;

        [Parameter]
        public SwitchParameter PublishRelatedItems { get; set; }

        [Parameter]
        public SwitchParameter RepublishAll { get; set; }

        [Parameter]
        public SwitchParameter CompareRevisions { get; set; }

        [Parameter]
        public DateTime FromDate { get; set; }

        [Parameter]
        public SwitchParameter AsJob { get; set; }

        protected override void ProcessItem(Item item)
        {
            if (item.Database.Name.IsNot("master"))
            {
                WriteError(typeof(PSInvalidOperationException), $"Item '{item.Name}' cannot be published. Only items from the 'master' database can be published!",
                    ErrorIds.InvalidOperation, ErrorCategory.InvalidOperation, null);
                return;
            }

            var source = Factory.GetDatabase("master");

            if (Target != null)
            {
                var targets = Target.Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                foreach (var target in targets.Select(Factory.GetDatabase))
                {
                    PublishToTarget(item, source, target);
                }
            }
            else
            {
                foreach (var publishingTarget in PublishManager.GetPublishingTargets(source))
                {
                    var target = Factory.GetDatabase(publishingTarget[Sitecore.FieldIDs.PublishingTargetDatabase]);
                    PublishToTarget(item, source, target);
                }
            }
        }

        private void PublishToTarget(Item item, Database source, Database target)
        {
            if (PublishMode == PublishMode.Unknown)
            {
                PublishMode = PublishMode.Smart;
            }

            var language = item.Language;

            if (ShouldProcess(item.GetProviderPath(),
                string.Format("{3}ublishing language '{0}', version '{1}' to target '{2}'.", language, item.Version,
                    target.Name, Recurse.IsPresent ? "Recursively p" : "P")))
            {
                WriteVerbose($"Publishing item '{item.Name}' in language '{language}', version '{item.Version}' to target '{target.Name}'.  (Recurse={Recurse.IsPresent}).");

                var options = new PublishOptions(source, target, PublishMode, language, DateTime.Now)
                {
                    Deep = Recurse,
                    RootItem = (PublishMode == PublishMode.Incremental) ? null : item,
                    RepublishAll = RepublishAll,
                    CompareRevisions = CompareRevisions || PublishMode == PublishMode.Smart
                };

                if (PublishMode == PublishMode.Incremental)
                {
                    WriteVerbose("Incremental publish causes ALL Database Items that are in the publishing queue to be published.");
                }

                if (!CompareRevisions && IsParameterSpecified(nameof(CompareRevisions)) && (PublishMode == PublishMode.Smart))
                {
                    WriteWarning($"The -{nameof(CompareRevisions)} parameter is set to $false but required to be $true when -{nameof(PublishMode)} is set to {PublishMode.Smart}, forcing {CompareRevisions} to $true.");
                }

                if (IsParameterSpecified(nameof(FromDate)))
                {
                    options.FromDate = FromDate;
                }

                if (PublishRelatedItems)
                {
                    options.PublishRelatedItems = PublishRelatedItems;
                    // Below blog explains why we're forcing Single Item 
                    // http://www.sitecore.net/learn/blogs/technical-blogs/reinnovations/posts/2014/03/related-item-publishing.aspx
                    if (PublishRelatedItems && IsParameterSpecified(nameof(PublishMode)) && (PublishMode != PublishMode.SingleItem))
                    {
                        WriteWarning($"The -{nameof(PublishRelatedItems)} parameter is used which requires -{nameof(PublishMode)} to be set to set to {PublishMode.SingleItem}, forcing {PublishMode.SingleItem} PublishMode.");
                    }
                    options.Mode = PublishMode.SingleItem;
                }

                if (AsJob)
                {
                    var publisher = new Publisher(options);
                    var job = publisher.PublishAsync();

                    if (job == null) return;
                    WriteObject(job);
                }
                else
                {
                    var publishContext = PublishManager.CreatePublishContext(options);
                    publishContext.Languages = new[] { language };
                    var stats = PublishPipeline.Run(publishContext)?.Statistics;
                    if (stats != null)
                    {
                        WriteVerbose($"Items Created={stats.Created}, Deleted={stats.Deleted}, Skipped={stats.Skipped}, Updated={stats.Updated}.");
                    }

                    WriteVerbose("Publish Finished.");
                }
            }
        }

        protected override List<Item> LatestVersionInFilteredLanguages(Item item)
        {
            var languagePatterns = new List<WildcardPattern>();
            IEnumerable<string> fullyQualifiedLanguages = new List<string>();
            if (Language != null && Language.Any())
            {
                languagePatterns =
                    Language.Where(lang => lang.Contains('*') || lang.Contains('?')).Select(
                            language =>
                                new WildcardPattern(language,
                                    WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant))
                        .ToList();
                fullyQualifiedLanguages = Language.Where(lang => !lang.Contains('*') && !lang.Contains('?'));
            }


            var publishedLangs = new List<string>();
            var result = new List<Item>();

            foreach (var langName in fullyQualifiedLanguages)
            {
                var language = LanguageManager.GetLanguage(langName);
                var langItem = item.Database.GetItem(item.ID, language);
                if (!publishedLangs.Contains(langItem.Language.Name))
                {
                    publishedLangs.Add(langItem.Language.Name);
                    result.Add(langItem);
                }
            }

            // if there are any wildcards - filter item in all languages
            if (languagePatterns.Any())
            {
                foreach (
                    var langItem in
                    item.Versions.GetVersions(true).Reverse())
                {

                    // publish latest version of each language
                    if (LanguageWildcardPatterns.Any(wildcard => !publishedLangs.Contains(langItem.Language.Name) &&
                                                                 wildcard.IsMatch(langItem.Language.Name)))
                    {
                        publishedLangs.Add(langItem.Language.Name);
                        result.Add(langItem);
                    }
                }
            }
            return result;
        }

    }
}