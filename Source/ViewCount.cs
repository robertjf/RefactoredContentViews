using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco;

namespace Refactored.UmbracoViewCounter
{
    /// <summary>
    /// View Count library methods for controlling and retrieving View Count data.
    /// </summary>
    [XsltExtension("ViewCount")]
    public class ViewCount
    {
        #region Counter Manipulation
        /// <summary>
        /// Increments the View count for the specified Node Id.  If History is enabled, then a History record is also created
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.  <em>Optional</em></param>
        public static void Increment(int nodeId, string category = "")
        {
            Trace.Write(umbraco.GlobalSettings.DbDSN, "ViewCount.IncrementViewCount");

            DateTime lastViewed = DateTime.Now;
            if (CounterRecordsExist(nodeId, category))
            {
                int viewCount = SqlHelper.ExecuteScalar<int>("select count from refViewCount where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));

                Trace.Write(string.Format("Updating View Count to {0}", viewCount + 1), "ViewCount.IncrementViewCount");
                SqlHelper.ExecuteNonQuery("update refViewCount set count = @count, lastViewed = @lastViewed where nodeId = @nodeId and category = @category",
                        SqlHelper.CreateParameter("@nodeId", nodeId),
                        SqlHelper.CreateParameter("@category", category),
                        SqlHelper.CreateParameter("@count", viewCount + 1),
                        SqlHelper.CreateParameter("@lastViewed", lastViewed));
            }
            else
            {

                SqlHelper.ExecuteNonQuery(@"insert into refViewCount (nodeId, category, count, lastViewed) 
values (@nodeId, @category, @count, @lastViewed)",
                        SqlHelper.CreateParameter("@nodeId", nodeId),
                        SqlHelper.CreateParameter("@category", category),
                        SqlHelper.CreateParameter("@count", 1),
                        SqlHelper.CreateParameter("@lastViewed", lastViewed));
            }

            InsertHistoryRecord(nodeId, category, lastViewed, false);
        }

        /// <summary>
        /// Resets the View Count for the specified node and category.  If History is turned on, then a History record will be created indicating the reset.
        /// </summary>
        /// <remarks><para>History may be turned on in the configuration via a View Counter DataType connected to the Node.</para></remarks>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.  <em>Optional</em> Defaults to an empty string</param>
        public static void Reset(int nodeId, string category = "")
        {
            DateTime lastViewed = DateTime.Now;
            if (CounterRecordsExist(nodeId, category))
            {
                SqlHelper.ExecuteNonQuery("update refViewCount set count = 0, lastViewed = @lastViewed where nodeId = @nodeId and category = @category",
                        SqlHelper.CreateParameter("@nodeId", nodeId),
                        SqlHelper.CreateParameter("@category", category),
                        SqlHelper.CreateParameter("@lastViewed", lastViewed));

                InsertHistoryRecord(nodeId, category, lastViewed, true);
            }
        }

        private static void InsertHistoryRecord(int nodeId, string category, DateTime lastViewed, bool reset)
        {
            if (HistoryEnabled(nodeId, category))
                SqlHelper.ExecuteNonQuery(@"insert into refViewCountHistory (counterId, updated, reset) values (@counterId, @viewed, @reset)",
                                    SqlHelper.CreateParameter("@counterId", EnumerateCounterIds(nodeId, category).First()),
                                    SqlHelper.CreateParameter("@viewed", lastViewed),
                                    SqlHelper.CreateParameter("@reset", reset));
        }

        /// <summary>
        /// Deletes the View Count information for the specified node and category.
        /// </summary>
        /// <remarks><para>If Category is null (the default), then all view data related to the Node Id will be deleted regardless of category.</para></remarks>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.  <em>Optional</em>  Defaults to null</param>
        /// <param name="deleteHistory">delete history records as well <em>Optional</em> Defaults to true</param>
        public static void Delete(int nodeId, string category = null, bool deleteHistory = true)
        {
            if (deleteHistory)
                DeleteHistory(nodeId, category);

            if (category == null)
            {
                SqlHelper.ExecuteNonQuery("delete from refViewCount where nodeId = @nodeId",
                    SqlHelper.CreateParameter("@nodeId", nodeId));
                SqlHelper.ExecuteNonQuery("delete from refViewCountConfig where nodeId = @nodeId",
                    SqlHelper.CreateParameter("@nodeId", nodeId));
                SqlHelper.ExecuteNonQuery("delete from refViewCountConfig where nodeId = @nodeId",
                    SqlHelper.CreateParameter("@nodeId", nodeId));
            }
            else
            {
                SqlHelper.ExecuteNonQuery("delete from refViewCount where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));
                SqlHelper.ExecuteNonQuery("delete from refViewCountConfig where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));
            }
        }

        /// <summary>
        /// Deletes the View Count history for the specified node and category.
        /// </summary>
        /// <remarks><para>If Category is null (the default), then all view data history related to the Node Id will be deleted regardless of category.  Does not delete current view count.</para></remarks>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.  <em>Optional</em>  Defaults to null</param>
        public static void DeleteHistory(int nodeId, string category = null)
        {
            foreach(int i in EnumerateCounterIds(nodeId, category))
                SqlHelper.ExecuteNonQuery("delete from refViewCountHistory where counterId = @counterId",
                        SqlHelper.CreateParameter("@counterId", i));
        }

        internal static IEnumerable<int> EnumerateCounterIds(int nodeId, string category = null)
        {
            if (category == null)
            {
                var reader = SqlHelper.ExecuteReader("select id from refViewCount where nodeId = @nodeId",
                                            SqlHelper.CreateParameter("@nodeId", nodeId));
                while (reader.Read())
                {
                    yield return reader.GetInt("id");
                }
                reader.Close();
            }
            else
            {
                yield return SqlHelper.ExecuteScalar<int>("select id from refViewCount where nodeId = @nodeId and category = @category",
                                            SqlHelper.CreateParameter("@nodeId", nodeId),
                                            SqlHelper.CreateParameter("@category", category));
            }
        }

        internal static bool CounterRecordsExist(int nodeId, string category = "")
        {
            int exists = SqlHelper.ExecuteScalar<int>("select count(*) from refViewCount where nodeId = @nodeId and category = @category",
                            SqlHelper.CreateParameter("@nodeId", nodeId),
                            SqlHelper.CreateParameter("@category", category));
            return exists > 0;
        }
        #endregion
        #region Configuration
        internal static void UpdateConfig(int nodeId, string category, bool hideCounter, bool enableHistory)
        {
            SqlHelper.ExecuteNonQuery(@"delete from refViewCountConfig where nodeId = @nodeId and category = @category",
                SqlHelper.CreateParameter("@nodeId", nodeId),
                SqlHelper.CreateParameter("@category", string.IsNullOrWhiteSpace(category) ? string.Empty : category.Trim()));

            SqlHelper.ExecuteNonQuery(@"insert into refViewCountConfig (nodeId, category, hideCounter, enableHistory) 
values (@nodeId, @category, @hideCounter, @enableHistory)",
                SqlHelper.CreateParameter("@nodeId", nodeId),
                SqlHelper.CreateParameter("@category", string.IsNullOrWhiteSpace(category) ? string.Empty : category.Trim()),
                SqlHelper.CreateParameter("@hideCounter", hideCounter),
                SqlHelper.CreateParameter("@enableHistory", enableHistory));
        }

        /// <summary>
        /// Returns whether the Specified Counter should be hidden from Web Views
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category. <em>Optional</em> Defaults to empty string</param>
        /// <returns>bool</returns>
        public static bool CounterHidden(int nodeId, string category = "")
        {

            return SqlHelper.ExecuteScalar<bool>("select hideCounter from refViewCountConfig where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));
        }

        /// <summary>
        /// Returns whether or not History is turned on for the specified Counter
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category. <em>Optional</em> Defaults to empty string</param>
        /// <returns>bool</returns>
        public static bool HistoryEnabled(int nodeId, string category = "")
        {
            return SqlHelper.ExecuteScalar<bool>("select enableHistory from refViewCountConfig where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));
        }
        #endregion
        #region XML Library Methods
        /// <summary>
        /// Gets the View Count Data for the specified Node Id for consumption by XSLT
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <returns>XPathNodeIterator containing View Count and configuration</returns>
        public static XPathNodeIterator GetViews(int nodeId)
        {
            return GetViews(nodeId, string.Empty, false);
        }


        /// <summary>
        /// Gets the View Count Data for the specified Node Id for consumption by XSLT
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.  </param>
        /// <returns>XPathNodeIterator containing View Count and configuration</returns>
        public static XPathNodeIterator GetViews(int nodeId, string category)
        {
            return GetViews(nodeId, category, false);
        }

        /// <summary>
        /// Gets the View Count Data for the specified Node Id for consumption by XSLT
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.</param>
        /// <param name="increment">View Count information is updated if this is true after retrieving the current count.</param>
        /// <returns>XPathNodeIterator containing View Count and configuration</returns>
        public static XPathNodeIterator GetViews(int nodeId, string category, bool increment)
        {
            XmlDocument xd = new XmlDocument();
            XmlNode pv = xd.CreateElement("pageViews");
            xd.AppendChild(pv);

            IRecordsReader rr = SqlHelper.ExecuteReader("select * from refViewCount where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));

            XmlNode v = xd.CreateElement("viewCount");

            if (rr.Read())
            {
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "nodeId", nodeId.ToString()));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "category", category));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "count", rr.IsNull("count") ? "0" : rr.GetInt("count").ToString()));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "lastViewed", rr.IsNull("lastViewed") ? "" : rr.GetDateTime("lastViewed").ToString("s")));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "hideCounter", CounterHidden(nodeId, category) ? "1" : "0"));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "enableHistory", HistoryEnabled(nodeId, category) ? "1" : "0"));

            }
            else
            {
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "nodeId", nodeId.ToString()));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "category", category));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "count", "0"));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "lastViewed", string.Empty));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "hideCounter", CounterHidden(nodeId, category) ? "1" : "0"));
                v.Attributes.Append(umbraco.xmlHelper.addAttribute(xd, "enableHistory", HistoryEnabled(nodeId, category) ? "1" : "0"));

            }
            rr.Close();

            pv.AppendChild(v);

            if (increment)
                Increment(nodeId, category);

            return xd.CreateNavigator().Select(".");
        }
        #endregion

        /// <summary>
        /// Retrieves the default View Count value only, without incrementing the View Count
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <returns>int containing the last Count</returns>
        public static int GetViewCount(int nodeId)
        {
            return GetViewCount(nodeId, string.Empty);
        }

        /// <summary>
        /// Retrieves the specified View Count value only, without incrementing the View Count
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.</param>
        /// <returns>int containing the last Count</returns>
        public static int GetViewCount(int nodeId, string category)
        {
            return GetViewCount(nodeId, category, false);
        }

        /// <summary>
        /// Retrieves the View Count value only, while optionally incrementing the View Count
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.</param>
        /// <param name="increment">View Count information is updated first if this is true.</param>
        /// <returns>int containing the last Count</returns>
        public static int GetViewCount(int nodeId, string category, bool increment)
        {
            int viewCount = SqlHelper.ExecuteScalar<int>("select count from refViewCount where nodeId = @nodeId and category = @category",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category));

            if (increment)
                Increment(nodeId, category);

            return viewCount;
        }

        /// <summary>
        /// Retrieves the View Count value only, while optionally incrementing the View Count
        /// </summary>
        /// <param name="date">Date </param>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.</param>
        /// <returns>int containing the last Count</returns>
        public static int GetViewCountSince(DateTime date, int nodeId, string category)
        {
            return GetViewCountSince(date, nodeId, category, false);
        }

        /// <summary>
        /// Retrieves the View Count value only, while optionally incrementing the View Count
        /// </summary>
        /// <param name="date">Date </param>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.</param>
        /// <param name="increment">View Count information is updated first if this is true.</param>
        /// <returns>int containing the last Count</returns>
        public static int GetViewCountSince(DateTime date, int nodeId, string category, bool increment)
        {
            int viewCount = SqlHelper.ExecuteScalar<int>(@"SELECT count(refViewCountHistory.counterId)
FROM refViewCount inner join refViewCountHistory on refViewCount.id = refViewCountHistory.counterId
where refViewCount.nodeId = @nodeId 
and refViewCount.category = @category
and refViewCountHistory.updated >= @date",
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@category", category),
                    SqlHelper.CreateParameter("@date", date));

            if (increment)
                Increment(nodeId, category);

            return viewCount;
        }
        #region View Count History
        /// <summary>
        /// Retrieves all history records for the specified content node and Default category
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <returns>IEnumerable of ViewHistoryData containing view date and reset details</returns>
        public static IEnumerable<ViewHistoryData> GetHistory(int nodeId)
        {
            return GetHistory(nodeId, string.Empty);
        }

        /// <summary>
        /// Retrieves all history records for the specified content node and specified category
        /// </summary>
        /// <param name="nodeId">Content Node Id</param>
        /// <param name="category">Category.</param>
        /// <returns>IEnumerable of ViewHistoryData containing view date and reset details</returns>
        public static IEnumerable<ViewHistoryData> GetHistory(int nodeId, string category)
        {
            var reader = SqlHelper.ExecuteReader("select * from refViewCount where counterId = @counterId",
                    SqlHelper.CreateParameter("@counterId", EnumerateCounterIds(nodeId, category)));

            while (reader.Read())
            {
                yield return new ViewHistoryData { Updated = reader.GetDateTime("updated"), Reset = reader.GetBoolean("reset") };
            }

            reader.Close();
        }
        #endregion

        internal static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }
    }
}