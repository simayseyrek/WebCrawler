using System;
using System.Collections.Specialized;
using HtmlAgilityPack;
using crawlerUtilities; // 2019103004
using System.Collections.Generic;

namespace crawlerUI
{
    class URLCrawler
    {
        // Variables
        private HtmlDocument htmlDoc;
        private HtmlWeb htmlWeb;
        private URLParentData parentURLData;

        // Take input url as string
        // 2019103002
        public URLCrawler(URLParentData parentURLData)
        {
            this.parentURLData = parentURLData;
            htmlWeb = new HtmlWeb();
            htmlDoc = new HtmlDocument();
        }

        public void start()
        {
            parentURLData.setState(CrawlerState.Crawling);
            try
            {
                LoadHTML();
                ParseHTML();
                parentURLData.setState(CrawlerState.Crawled);

            }
            catch
            {
                parentURLData.errorCounter++;
                parentURLData.setState(CrawlerState.Failed);
            }
        }

        // Function loads HTML document from the url
        private void LoadHTML()
        {
            htmlDoc = htmlWeb.Load(parentURLData.uriURL);

            // Get title
            var urlTitle = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText.ToString().Trim();
            urlTitle = System.Net.WebUtility.HtmlDecode(urlTitle);
            parentURLData.title = urlTitle;
        }

        // Function that parses HTML document for internal and external links
        private void ParseHTML()
        {
            // extracting all links
            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                // Obtain the URL
                HtmlAttribute att = link.Attributes["href"];

                // Adds host part (for the internal links)
                var crawled_url = new Uri(parentURLData.uriURL, att.Value);
                // TODO: check item type (png, zip, vs vs.)

                // Decide if the url is internal or external for the baseURL
                CrawledType type = CrawledType.externalURL;
                if (Uri.Compare(parentURLData.uriURL, crawled_url, UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.CurrentCulture) == 0)
                {
                    type = CrawledType.internalURL;
                }

                // add the child url to the parent's list
                URLChildData child = new URLChildData(crawled_url.AbsoluteUri, type, parentURLData.rootURL);
                parentURLData.childrenURLs.Add(child);
            }
        }
    }
}
