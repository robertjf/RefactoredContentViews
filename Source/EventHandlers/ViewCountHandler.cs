using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.nodeFactory;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;

namespace Refactored.UmbracoViewCounter.EventHandlers
{
    public class ViewCountHandler : ApplicationBase
    {
        public ViewCountHandler()
        {
            Document.AfterDelete += Document_AfterDelete;
            Media.AfterDelete += Media_AfterDelete;
        }

        void Media_AfterDelete(Media sender, DeleteEventArgs e)
        {
            ViewCount.Delete(sender.Id);
        }

        void Document_AfterDelete(Document sender, DeleteEventArgs e)
        {
            // We want to delete any page view records for the node.
            ViewCount.Delete(sender.Id);
        }
    }
}