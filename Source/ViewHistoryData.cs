using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Refactored.UmbracoViewCounter
{
    /// <summary>
    /// Represents a History record for View Count data.
    /// </summary>
    public class ViewHistoryData
    {
        /// <summary>
        /// Date the History Record was created.
        /// </summary>
        public DateTime Updated { get; set; }

        /// <summary>
        /// Whether or not the View Count this record is associated with was Reset.
        /// </summary>
        public bool Reset { get; set; }
    }
}