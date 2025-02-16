using SchoopFunctionApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SchoopFunctionApp.Extensions
{
    public static class HtmlExtensions
    {
        public static string ToUnderstandActiveYears(this string activeYears)
        {
            if (string.IsNullOrEmpty(activeYears))
                return "";

            if (activeYears.Contains("|0|"))
                return "All";
            if (activeYears == "0")
            {
                return "N/A";
            }
            var availableActiveYears = activeYears.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var understandActiveYears = new List<string>();
            foreach (var item in availableActiveYears)
            {
                var itemValue = 0;
                if (int.TryParse(item, out itemValue))
                {
                    var activeYearValue = string.Format("Y{0}", itemValue - 2);
                    switch (itemValue)
                    {
                        case 1:
                            activeYearValue = "Nursery";
                            break;
                        case 2:
                            activeYearValue = "Reception";
                            break;
                        default:
                            break;
                    }
                    understandActiveYears.Add(activeYearValue);
                }
            }

            if (understandActiveYears.Count > 0)
                return string.Join(", ", understandActiveYears);
            return "";
        }

        public static string ToActiveGroupNames(this string activeGroups, int languageID)
        {
            if (string.IsNullOrEmpty(activeGroups))
                return "";

            if (activeGroups.Contains("|0|"))
                return "|0|";
            if (activeGroups == "0")
            {
                return "0";
            }
            var availableActiveGroups = activeGroups.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var dataService = new DataServices();
            var groupIds = new List<int>();
            foreach (var item in availableActiveGroups)
            {
                var itemValue = 0;
                if (int.TryParse(item, out itemValue))
                {
                    groupIds.Add(itemValue);
                }
            }

            var groupNames = dataService.GetGroupNamesByGroups(groupIds, languageID);

            if (groupNames.Count > 0)
                return "|" + string.Join("|", groupNames) + "|";
            return "";
        }
    }
}
