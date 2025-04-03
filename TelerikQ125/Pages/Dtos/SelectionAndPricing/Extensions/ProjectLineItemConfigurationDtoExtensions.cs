


using System.Collections.Generic;
using System.Linq;

namespace telerik_Q1_25.Pages.Dtos.SelectionAndPricing.Extensions
{
    public static class ProjectLineItemConfigurationDtoExtensions
    {
        /// <summary>
        /// Sort the list of ProjectLineItemConfigurationDtos by their StepID property, either in ASC (default) or DESC order.
        /// </summary>
        /// <param name="projectLineItemConfigurationDtos">List of ProjectLineItemConfigurationDtos to sort.</param>
        /// <param name="sortDescending">Sort the list in descending order (versus the default ascending order).</param>
        public static IList<ProjectLineItemConfigurationDto> SortLineItemConfigurationDtos(this IList<ProjectLineItemConfigurationDto> projectLineItemConfigurationDtos, bool sortDescending = false)
        {
            return sortDescending ?
                projectLineItemConfigurationDtos.OrderByDescending(c => c.StepId).ToList() :
                projectLineItemConfigurationDtos.OrderBy(c => c.StepId).ToList();
        }
    }
}
