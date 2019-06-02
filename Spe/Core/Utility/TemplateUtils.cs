﻿using System;
using System.Data;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Spe.Core.Utility
{
    public static class TemplateUtils
    {
        public static Item GetFromPath(string itemPath, string currentDrive)
        {
            if (itemPath.StartsWith(@".\"))
            {
                itemPath = itemPath.Substring(2);
            }

            itemPath = itemPath.Replace('\\', '/').Trim('/');

            var templateItem = ID.IsID(itemPath)
                ? Factory.GetDatabase(currentDrive).GetItem(ID.Parse(itemPath))
                : PathUtilities.GetItem(currentDrive, "/" + itemPath);

            if (templateItem == null)
            {
                // for when the template name is starting with /sitecore/
                if (itemPath.StartsWith("sitecore/", StringComparison.OrdinalIgnoreCase))
                {
                    itemPath = itemPath.Substring(9);
                }
                //for when the /templates at the start was missing
                if (!itemPath.StartsWith("templates/", StringComparison.OrdinalIgnoreCase))
                {
                    itemPath = "templates/" + itemPath;
                }

                templateItem = PathUtilities.GetItem(currentDrive, "/" + itemPath);

                if (templateItem == null)
                {
                    throw new ObjectNotFoundException(
                        string.Format("Template '{0}' does not exist or wrong path provided.",
                            itemPath));
                }
            }

            return templateItem;
        }

        public static TemplateItemType GetType(Item item)
        {
            switch (item.TemplateName)
            {
                case "Template": return TemplateItemType.Template;
                case "Branch": return TemplateItemType.Branch;
            }

            return TemplateItemType.None;
        }
    }

    public enum TemplateItemType
    {
        None,
        Template,
        Branch
    }
}