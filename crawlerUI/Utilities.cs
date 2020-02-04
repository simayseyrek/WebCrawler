
namespace crawlerUtilities // 2019103004
{
    public enum CrawlerState // 2019103011
    {
        ToBeCrawled = 0,
        Crawling = 1,
        Crawled = 2,
        Failed = 3,
        Disabled = 4,
        Root = 5,
    }

    public enum CrawledType // 2019103011
    {
        internalURL = 0,
        externalURL = 1,
    }
}
