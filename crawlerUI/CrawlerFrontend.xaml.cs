using crawlerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace crawlerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CrawlerFrontend : Window
    {
        private CrawlerBackend crawlerBE;
        private DispatcherTimer timerUpdateURLTab;
        private DispatcherTimer timerUpdateStatisticsTab;
        private CrawlerState state;

       public CrawlerFrontend()
        {
            InitializeComponent();

            ThreadPool.SetMaxThreads(100000, 100000);
            ThreadPool.SetMinThreads(100000, 100000);
            ServicePointManager.DefaultConnectionLimit = 1000;

            crawlerBE = new CrawlerBackend();

            state = CrawlerState.Crawled;
            timerUpdateURLTab = new DispatcherTimer(); // 2019103041
            timerUpdateURLTab.Tick += timerUpdateURLTab_Tick;
            timerUpdateURLTab.Interval = new TimeSpan(0, 0, 0, 0, 250);  // 2019103040

            timerUpdateStatisticsTab = new DispatcherTimer(); // 2019103041
            timerUpdateStatisticsTab.Tick += timerUpdateStatisticsTab_Tick;
            timerUpdateStatisticsTab.Interval = new TimeSpan(0, 0, 0, 0, 250);  // 2019103040
        }

        private void btnAddNewURL_Click(object sender, RoutedEventArgs e)
        {
            bool externalActivated = false;
            string rootURL = txtRootURL.Text;

            if (Uri.IsWellFormedUriString(rootURL, UriKind.Absolute))
            {
                if(Int32.TryParse(tb_maxThreadNumber.Text, out int thread_number))
                {
                    if (cb_activateExternal.IsChecked == true)
                        externalActivated = true;
                    else
                        externalActivated = false;
                    crawlerBE.newRootURL(rootURL, thread_number, externalActivated);
                }
                else
                    MessageBox.Show("Max thread number should be integer. Please try again.");
            }
            else
            {
                MessageBox.Show("URL is not valid. Please try again.");
            }
        }

        private void btnStartCrawling_Click(object sender, RoutedEventArgs e)
        {
            crawlerBE.startCrawling();
        }

        private void btnStopCrawling_Click(object sender, RoutedEventArgs e)
        {
            crawlerBE.stopCrawling();
        }

        private void btnPauseCrawling_Click(object sender, RoutedEventArgs e)
        {
            crawlerBE.pauseCrawling();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            crawlerBE.saveToDatabase();
        }

        private void btnCloseApplication_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // check which tab is selected
            if (ti_main.IsSelected)
            {
                timerUpdateURLTab.Stop();
                timerUpdateStatisticsTab.Stop();
            }
			else if (ti_url.IsSelected)
            {
                timerUpdateURLTab.Start();
                timerUpdateStatisticsTab.Stop();
            }
            else if (ti_stats.IsSelected)
            {
                timerUpdateURLTab.Stop();
                timerUpdateStatisticsTab.Start();
            }
        }

        private void timerUpdateStatisticsTab_Tick(object sender, EventArgs e)
        {
            tb_date.Text = DateTime.Now.ToString();

            List<int> urlSizes = crawlerBE.GetURLsizes();
            tb_success.Text = urlSizes[0].ToString();
            tb_disable.Text = urlSizes[3].ToString();

            TimeSpan runTime = crawlerBE.GetRuntime();
            tb_runTime.Text = runTime.ToString() + " (H:M:S:ms)";
            tb_avgUrls.Text = String.Format("{0:0.00}", (urlSizes[0] / runTime.TotalMinutes)).ToString() + " (page per minute)";
        }

        private void timerUpdateURLTab_Tick(object sender, EventArgs e)
        {
            List<string> urls = crawlerBE.GetURLs(state);

            if(state == CrawlerState.Crawling || state == CrawlerState.ToBeCrawled || state == CrawlerState.Disabled)
            {
                List<string> itemsToBeDeleted = new List<string>();
                foreach (string url in lb_URL.Items) // remove currently visible but not in backend items
                {
                    if (urls.IndexOf(url) == -1)
                        itemsToBeDeleted.Add(url);
                }
                foreach(string url in itemsToBeDeleted)
                    lb_URL.Items.Remove(url);
            }
            foreach (string url in urls) // add items from backend which are not visible
            {
                if (lb_URL.Items.IndexOf(url) == -1)
                    lb_URL.Items.Add(url);
            }

            List<int> urlSizes = crawlerBE.GetURLsizes();
            rb_crawled.Content = "Crawled (" + urlSizes[0] + ")";
            rb_crawling.Content = "Crawling (" + urlSizes[1] + ")";
            rb_toBeCrawled.Content = "To Be Crawled (" + urlSizes[2] + ")";
            rb_disabled.Content = "Disabled (" + urlSizes[3] + ")";
            rb_roots.Content = "Roots (" + urlSizes[4] + ")";
        }

        private void rb_Checked(object sender, RoutedEventArgs e)
        {
            lb_URL.Items.Clear();
            if (rb_crawled.IsChecked == true)
            {
                state = CrawlerState.Crawled;
            }
            else if (rb_crawling.IsChecked == true)
            {
                state = CrawlerState.Crawling;
            }
            else if (rb_toBeCrawled.IsChecked == true)
            {
                state = CrawlerState.ToBeCrawled;
            }
            else if (rb_disabled.IsChecked == true)
            {
                state = CrawlerState.Disabled;
            }
            else if (rb_roots.IsChecked == true)
            {
                state = CrawlerState.Root;
            }
        }

        private void lb_URL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_URL.SelectedItem == null)
                return;

            string clickedURL = lb_URL.SelectedItem.ToString();

            if (rb_crawled.IsChecked == true)
            {
                CrawledWindow window = new CrawledWindow(crawlerBE.GetParent(clickedURL));
                window.Show();
            }
            else if (rb_roots.IsChecked == true)
            {
                RootWindow window = new RootWindow(crawlerBE.GetRoot(clickedURL), crawlerBE);
                window.Show();
            }

        }
    }
}
