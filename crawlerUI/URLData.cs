using System;
using System.Collections.Generic;
using System.Linq;
using crawlerUtilities;

namespace crawlerUI
{
    public abstract class URLAbstractData // 2019103014
    {
        private string _url;
        public string url { get { return _url; } set { _url = value.Split('#').First(); } }
        public string rootURL { get; set; }
        public Uri uriURL { get; set; }
        public int errorCounter { get; set; }
        public DateTime urlDisabledTime { get; set; }
        public CrawlerState state { get; set; }
        public DateTime urlRegisteredTime { get; set; }

        public URLAbstractData(string url, string rootURL)
        {
            this.url = url; // 2019103006
            this.rootURL = rootURL; // 2019103006
            uriURL = new Uri(url);
            errorCounter = 0;
            state = CrawlerState.ToBeCrawled;
            urlRegisteredTime = DateTime.Now;
        }

        public virtual void setState(CrawlerState state) // 2019103013
        {
            this.state = state; // 2019103006
        }
    }

    public class URLParentData : URLAbstractData // 2019103013
    {
        public string title { get; set; }
        public DateTime urlCrawledTime { get; set; }
        public List<URLChildData> childrenURLs { get; set; }

        // Constructor
        public URLParentData(URLChildData child) : base(child.url, child.rootURL) // 2019103012
        {
            errorCounter = child.errorCounter;
            childrenURLs = new List<URLChildData>();
        }

        public override void setState(CrawlerState state) // 2019103013
        {
            this.state = state; // 2019103012
            if (state == CrawlerState.Crawled)
            {
                urlCrawledTime = DateTime.Now;
            }
        }
    }

    public class URLChildData : URLAbstractData // 2019103013
    {
        public CrawledType type { get; set; }

        public URLChildData(string url, CrawledType type, string rootURL) : base(url, rootURL) // 2019103012
        {
            this.type = type; // 2019103006
        }
    }

    public sealed class URLRootData // 2019103016
    {
        public string rootURL;
        public int maxThreadNo;
        public bool externalActivated;

        public URLRootData(string rootURL, int maxThreadNo, bool externalActivated)
        {
            this.rootURL = rootURL; // 2019103006
            this.maxThreadNo = maxThreadNo; // 2019103006
            this.externalActivated = externalActivated; // 2019103006
        }
    }
}
