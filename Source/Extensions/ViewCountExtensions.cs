using System.Linq;
using System.Xml.Linq;
using umbraco.cms.businesslogic;
using umbraco.MacroEngines;
using umbraco.interfaces;
using System;

namespace Refactored.UmbracoViewCounter.Extensions
{
    /// <summary>
    /// Provides Extension methods for Content instances
    /// </summary>
    public static class ViewCountExtensions
    {
        /// <summary>
        /// Gets the View Counter
        /// </summary>
        /// <param name="node">Content Node</param>
        /// <param name="category">Category. <em>Optional</em></param>
        /// <param name="increment">Specifies whether to increment the count or not.  <em>Optional</em></param>
        /// <returns></returns>
        public static int CurrentViewCount(this Content node, string category = "", bool increment = false)
        {
            if (node == null)
                return 0;

            return ViewCount.GetViewCount(node.Id, category, increment);
        }

        /// <summary>
        /// Gets the View Counter
        /// </summary>
        /// <param name="node">Content Node</param>
        /// <param name="category">Category. <em>Optional</em></param>
        /// <param name="increment">Specifies whether to increment the count or not.  <em>Optional</em></param>
        /// <returns></returns>
        public static int ViewCountSince(this Content node, DateTime date, string category = "", bool increment = false)
        {
            if (node == null)
                return 0;

            if (ViewCount.HistoryEnabled(node.Id, category))
                return ViewCount.GetViewCountSince(date, node.Id, category, increment);
            else
                return ViewCount.GetViewCount(node.Id, category, increment);
        }

        /// <summary>
        /// Gets whether to hide the View Counter or not.
        /// </summary>
        /// <param name="node">Content Node</param>
        /// <param name="category">Category. <em>Optional</em></param>
        /// <returns></returns>
        public static bool CounterHidden(this Content node, string category = "")
        {
            if (node == null)
                return false;

            return ViewCount.CounterHidden(node.Id, category);
        }

        /// <summary>
        /// Silently Increments the View Counter
        /// </summary>
        /// <param name="node">Content Node</param>
        /// <param name="category">Category. <em>Optional</em></param>
        public static void IncrementCounter(this Content node, string category = "")
        {
            if (node == null)
                return;

            ViewCount.Increment(node.Id, category);
        }

        //public static int CurrentViewCount(this DynamicNode node, string category = "", bool increment = false)
        //{
        //    DynamicNode n = node as DynamicNode;
        //    if (node == null)
        //        return 0;

        //    return ViewCount.GetViewCount(n.Id, category, increment);
        //}

    }
}