using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using crawlerUtilities; // 2019103004 TODO: URLCrawler.GetState()
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.Specialized;

namespace crawlerUI
{
    public class CrawlerBackend
    {
        // Variables
        private TimeSpan DISABLE_DURATION = new TimeSpan(0, 2, 0); // 2019103040
        private List<Tuple<string, Task, string>> tl_activelyRunningTasks; // 2019103022
        private Dictionary<string, URLChildData> dToBeCrawledURLs;
        private Dictionary<string, URLChildData> dCrawlingURLs;
        private Dictionary<string, URLParentData> dCrawledURLs;
        private List<URLRootData> lRootURLs;
        private HashSet<URLChildData> disabledURLs;
        private HashSet<string> hsAllURLs;
        private DispatcherTimer timerLoadBalancer; // 2019103041
        private DispatcherTimer timerCheckDisabledURLs; // 2019103041
        private DispatcherTimer timerGarbageCollector; // 2019103041
        private Stopwatch stopwatch; // 2019103040

        // Constructor
        public CrawlerBackend()
        {
            dToBeCrawledURLs = new Dictionary<string, URLChildData>();
            dCrawlingURLs = new Dictionary<string, URLChildData>();
            dCrawledURLs = new Dictionary<string, URLParentData>();
            lRootURLs = new List<URLRootData>();
            disabledURLs = new HashSet<URLChildData>();
            hsAllURLs = new HashSet<string>();
            tl_activelyRunningTasks = new List<Tuple<string, Task, string>>();
            timerLoadBalancer = new DispatcherTimer(); 
            timerCheckDisabledURLs = new DispatcherTimer();
            timerGarbageCollector = new DispatcherTimer();
            stopwatch = new Stopwatch();

            loadFromDatabase();
            setupTimers();
        }

        private void setupTimers()
        {
            // Timer to check ToBeCrawled List to start new crawling tasks
            timerLoadBalancer.Tick += timerLoadBalancer_Tick;
            timerLoadBalancer.Interval = new TimeSpan(0, 0, 0, 0, 250);  // 2019103040

            // Timer to check disabledURLs List if their 24 hour ban is finished
            timerCheckDisabledURLs.Tick += timerCheckDisabled_Tick;
            timerCheckDisabledURLs.Interval = new TimeSpan(0, 0, 0, 10, 0);  // 2019103040

            // Timer to run every 15min to call GarbageCollector (automatic GC already called by system)
            timerGarbageCollector.Tick += timerGarbageCollector_Tick;
            timerGarbageCollector.Interval = new TimeSpan(0, 0, 15, 0, 0);  // 2019103040

            stopwatch.Start(); // run time and average number of crawled page measurement 
        }

        public void startCrawling()
        {
            timerLoadBalancer.Start();
            timerCheckDisabledURLs.Start();
            timerGarbageCollector.Start();
        }

        public void pauseCrawling()
        {
            timerLoadBalancer.Stop();
            timerCheckDisabledURLs.Stop();
            timerGarbageCollector.Stop();
        }

        public void stopCrawling()
        {
            timerLoadBalancer.Stop();
            timerCheckDisabledURLs.Stop();
            timerGarbageCollector.Stop();

            resetDatabase();

            // Waiting all tasks to finish
            // 2019103026
            foreach (Task runningTask in tl_activelyRunningTasks.Select(tuple => tuple.Item2))
                runningTask.Wait();

            dToBeCrawledURLs.Clear();
            dCrawlingURLs.Clear();
            dCrawledURLs.Clear();
            disabledURLs.Clear();
            hsAllURLs.Clear();
            lRootURLs.Clear();
            tl_activelyRunningTasks.Clear();
            GC.Collect();

            stopwatch.Restart();
        }

        private void timerLoadBalancer_Tick(object sender, EventArgs e)
        {
            // 6
            tl_activelyRunningTasks = tl_activelyRunningTasks.Where(tuple => tuple.Item2.Status != TaskStatus.RanToCompletion 
                                                                          && tuple.Item2.Status != TaskStatus.Faulted).ToList();

            HashSet<URLChildData> toBeCrawledNowList = loadBalancer();

            foreach (URLChildData tobeCrawledNow in toBeCrawledNowList)
            {
                startSingleCrawler(tobeCrawledNow);
            }
        }

        // checks disabled lists and their time and acts accordingly
        private void timerCheckDisabled_Tick(object sender, EventArgs e)
        {
            HashSet<URLChildData> enableList = new HashSet<URLChildData>();

            foreach (URLChildData disabledURL in disabledURLs)
            {
                DateTime expireDate = new DateTime(disabledURL.urlDisabledTime.Ticks + DISABLE_DURATION.Ticks);
                if (DateTime.Compare(DateTime.Now, expireDate) > 0)
                {
                    enableList.Add(disabledURL);
                }
            }

            foreach (URLChildData disabledURL in enableList)
            {
                lock (disabledURLs)
                    disabledURLs.Remove(disabledURL);
                lock (dToBeCrawledURLs)
                    dToBeCrawledURLs.Add(disabledURL.url, disabledURL);
            }
        }

        private void timerGarbageCollector_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }

        // her websitesine gore esit agirlikli tobecrawled listesi donmek
        private HashSet<URLChildData> loadBalancer()
        {
            HashSet<URLChildData> toBeCrawledNowList = new HashSet<URLChildData>();

            lock (dToBeCrawledURLs) // 2019103027
            {
                foreach (URLRootData rootData in lRootURLs)
                {
                    // Loop over items which have the same rootURL with rootData
                    //2019103026
                    int toBeCrawledNow_counter = 0;
                    var dToBeCrawledURLs_root = dToBeCrawledURLs.Values.Where(child => child.rootURL == rootData.rootURL).ToList();
                    var tl_activelyRunningTasks_root = GetActiveRunTupleList(rootData.rootURL);

                    foreach (URLChildData tobeCrawled in dToBeCrawledURLs_root)
                    {
                        if (tl_activelyRunningTasks_root.Count + toBeCrawledNow_counter >= rootData.maxThreadNo)
                            break;

                        toBeCrawledNow_counter++;
                        toBeCrawledNowList.Add(tobeCrawled);
                    }
                }
            }

            return toBeCrawledNowList;
        }

        public void newRootURL(string url, int max_thread_number, bool externalURLactivated)
        {
            if (URLexist(url))
                return; // skip the root url since it already exists

            lRootURLs.Add(new URLRootData(url, max_thread_number, externalURLactivated));

            URLChildData newRootData = new URLChildData(url, CrawledType.internalURL, url);
            lock (dToBeCrawledURLs)  // 2019103027
                dToBeCrawledURLs.Add(newRootData.url, newRootData);
        }

        private void startSingleCrawler(URLChildData childToBecomeParent)
        {
            // transfer url from toBeCrawled to crawling
            lock (dToBeCrawledURLs)  // 2019103027
                dToBeCrawledURLs.Remove(childToBecomeParent.url);
            lock (dCrawlingURLs)  // 2019103027
                dCrawlingURLs.Add(childToBecomeParent.url, childToBecomeParent);

            URLParentData parent = new URLParentData(childToBecomeParent);
            URLCrawler crawler = new URLCrawler(parent);
            
            // create a new task for crawler operation
            var urlCrawlerTask = Task.Factory.StartNew(() =>
            {
                crawler.start();
            }).ContinueWith(taskInfo =>
            {
                if (parent.state == CrawlerState.Crawled)
                {
                    // transfer url from crawling hs to crawled hs
                    lock (dCrawlingURLs)  // 2019103027
                        dCrawlingURLs.Remove(childToBecomeParent.url);
                    lock (dCrawledURLs) // 2019103027
                        dCrawledURLs.Add(parent.url, parent);

                    // loops over each child URL, checks if exists and adds it to tobecrawled hs
                    foreach (URLChildData child in parent.childrenURLs)
                    {
                        // 2019103026
                        bool externalURLactivated = lRootURLs.Where(rootData => rootData.rootURL == child.rootURL)
                                                             .Select(rootData => rootData.externalActivated)
                                                             .First();

                        // discards external URLs if externalURL checkbox is not activated
                        if (child.type == CrawledType.externalURL && !externalURLactivated)
                            continue;

                        if (!URLexist(child.url))
                        {
                            lock (dToBeCrawledURLs)
                                dToBeCrawledURLs.Add(child.url, child);
                        }   
                    }
                }
                else
                {
                    childToBecomeParent.errorCounter = parent.errorCounter;

                    // handle CrawlerState.Failed
                    if (childToBecomeParent.errorCounter >= 3)
                    {
                        childToBecomeParent.urlDisabledTime = DateTime.Now;
                        childToBecomeParent.errorCounter = 0;
                        lock (disabledURLs)
                            disabledURLs.Add(childToBecomeParent);
                    }
                    else
                    {
                        // transfer url from toBeCrawled to crawling
                        lock (dToBeCrawledURLs)
                            dToBeCrawledURLs.Add(childToBecomeParent.url, childToBecomeParent);
                    }
                    // remove url from crawling list
                    lock (dCrawlingURLs)
                        dCrawlingURLs.Remove(childToBecomeParent.url);
                }
            });
            Tuple<string, Task, string> newTask = new Tuple<string,Task,string>(childToBecomeParent.rootURL, urlCrawlerTask, childToBecomeParent.url);
            tl_activelyRunningTasks.Add(newTask);
        }

        // A function that checks if the given url exists in tobecrawled+crawling+crawled lists
        private bool URLexist(string url)
        {
            lock(hsAllURLs)
            {
                if (hsAllURLs.Contains(url))
                    return true;
                else
                {
                    hsAllURLs.Add(url);
                    return false;
                }
            }
        }

        public List<string> GetURLs(CrawlerState state)
        {
            switch(state)
            {
                case CrawlerState.Crawled:
                    return new List<string>(dCrawledURLs.Keys);
                case CrawlerState.Crawling:
                    return new List<string>(dCrawlingURLs.Keys);
                case CrawlerState.ToBeCrawled:
                    return new List<string>(dToBeCrawledURLs.Keys);
                case CrawlerState.Disabled:
                    return disabledURLs.Select(child => child.url).ToList(); // 2019103026
                case CrawlerState.Root:
                    return lRootURLs.Select(root => root.rootURL).ToList(); // 2019103026
                default:
                    return new List<string>();
            }
        }

        public List<int> GetURLsizes()
        {
            return new List<int> {dCrawledURLs.Count, dCrawlingURLs.Count,
                    dToBeCrawledURLs.Count, disabledURLs.Count, lRootURLs.Count};
        }

        // Function that takes url and returns the parent object
        public URLParentData GetParent(string url)
        {
            URLParentData parent;
            dCrawledURLs.TryGetValue(url, out parent);
            return parent;
        }

        // Function that takes url and returns the root object
        public URLRootData GetRoot(string rootURL)
        {
            URLRootData root;
            root = lRootURLs.Where(rootData => rootData.rootURL == rootURL).First(); // 2019103026
            return root;
        }

        public List<Tuple<string, Task, string>> GetActiveRunTupleList(string rootURL)
        {
            return tl_activelyRunningTasks.Where(tuple => tuple.Item1 == rootURL).ToList();
        }

        public TimeSpan GetRuntime()
        {
            return stopwatch.Elapsed;
        }

        public void saveToDatabase()
        {
            // MyDatabase.cs
            Console.WriteLine("Exit clicked. Save everything before close..");
        }

        public void loadFromDatabase()
        {
            // MyDatabase.cs
        }

        public void resetDatabase()
        {
            // MyDatabase.cs
        }
    }
}
