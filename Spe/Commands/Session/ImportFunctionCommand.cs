﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Sitecore.Data.Items;
using Spe.Commands.Interactive;
using Spe.Core.Diagnostics;
using Spe.Core.Extensions;
using Spe.Core.Modules;
using Spe.Core.Utility;
using Spe.Core.Validation;

namespace Spe.Commands.Session
{
    [Cmdlet(VerbsData.Import, "Function", SupportsShouldProcess = true)]
    [OutputType(typeof (object))]
    public class ImportFunctionCommand : BaseShellCommand
    {
        private static string[] functions;
        private static string[] libraries;
        private static string[] modules;


        [Parameter(Mandatory = true, Position = 0)]
        [AutocompleteSet(nameof(Functions))]
        public string Name { get; set; }

        [Parameter]
        [AutocompleteSet(nameof(Libraries))]
        public string Library { get; set; }

        [Parameter]
        [AutocompleteSet(nameof(Modules))]
        public string Module { get; set; }

        public static string[] Functions
        {
            get
            {
                if (functions == null)
                {
                    UpdateCache();
                }
                return functions;
            }
        }

        public static string[] Libraries
        {
            get
            {
                if (functions == null)
                {
                    UpdateCache();
                }
                return libraries;
            }
        }

        public static string[] Modules
        {
            get
            {
                if (functions == null)
                {
                    UpdateCache();
                }
                return modules;
            }
        }


        static ImportFunctionCommand()
        {
            ModuleManager.OnInvalidate += InvalidateCache;
            functions = null;
        }

        // Methods
        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(Name))
            {
                WriteError(new ErrorRecord(new AmbiguousMatchException(
                    "Function name was not provided."),
                    "sitecore_name_missing", ErrorCategory.NotSpecified, null));
                return;
            }

            var functionItems = new List<Item>();
            var roots = ModuleManager.GetFeatureRoots(IntegrationPoints.FunctionsFeature);

            if (!string.IsNullOrEmpty(Module))
            {
                roots =
                    roots.Where(
                        p =>
                            string.Equals(ModuleManager.GetItemModule(p).Name, Module,
                                StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            var name = Name.ToLower();
            foreach (var root in roots)
            {
                var path = PathUtilities.PreparePathForQuery(root.Paths.Path);
                var query = string.IsNullOrEmpty(Library)
                    ? $"{path}//*[@@TemplateId=\"{Templates.Script.Id}\" and @@Key=\"{name}\"]"
                    : $"{path}/#{Library}#//*[@@TemplateId=\"{Templates.Script.Id}\" and @@Key=\"{name}\"]";
                var scriptItems = root.Database.SelectItems(query);
                if (scriptItems?.Length > 0)
                {
                    functionItems.AddRange(scriptItems);
                }
            }

            if (functionItems.Count > 1)
            {
                WriteError(new ErrorRecord(new AmbiguousMatchException(
                        $"Ambiguous function name '{Name}' detected, please narrow your search by specifying sub-library and/or module name."),
                    "sitecore_ambiguous_name", ErrorCategory.InvalidData, null));
                return;
            }

            if (functionItems.Count == 0)
            {
                WriteError(new ErrorRecord(new AmbiguousMatchException(
                        $"Function item with name '{Name}' could not be found in the specified module or library or it does not exist."),
                    "sitecore_function_not_found", ErrorCategory.ObjectNotFound, null));
                return;
            }

            if (!IsPowerShellScriptItem(functionItems[0]))
            {
                return;
            }

            var script = functionItems[0][Templates.Script.Fields.ScriptBody];

            if (ShouldProcess(functionItems[0].GetProviderPath(), "Import functions"))
            {
                var sendToPipeline = InvokeCommand.InvokeScript(script, false,
                    PipelineResultTypes.Output | PipelineResultTypes.Error, null);

                if (sendToPipeline != null && sendToPipeline.Any())
                {
                    WriteObject(sendToPipeline);
                }
            }

        }

        private static void UpdateCache()
        {
            var localFunctions = new List<string>();
            var roots = ModuleManager.GetFeatureRoots(IntegrationPoints.FunctionsFeature);

            modules =
                (from module in ModuleManager.Modules where module.Enabled select module.Name).ToList()
                    .ConvertAll(WrapNameWithSpacesInQuotes)
                    .ToArray();

            libraries = (from root in roots
                from Item library in root.GetChildren()
                where library.IsPowerShellLibrary()
                select library.Name).ToList().ConvertAll(WrapNameWithSpacesInQuotes).ToArray();

            foreach (var root in roots)
            {
                var path = PathUtilities.PreparePathForQuery(root.Paths.Path);
                var query = $"{path}//*[@@TemplateId=\"{Templates.Script.Id}\"]";
                try
                {
                    var results = root.Database.SelectItems(query);
                    localFunctions.AddRange(
                        results.ToList().ConvertAll(p => WrapNameWithSpacesInQuotes(p.Name)));
                }
                catch (Exception ex)
                {
                    PowerShellLog.Error("Error while querying for items", ex);
                }
            }
            functions = localFunctions.ToArray();
        }

        public static void InvalidateCache(object sender, EventArgs e)
        {
            functions = null;
        }
    }
}