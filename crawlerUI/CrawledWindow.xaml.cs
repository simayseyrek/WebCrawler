using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace crawlerUI
{
    /// <summary>
    /// Interaction logic for CrawledWindow.xaml
    /// </summary>
    public partial class CrawledWindow : Window
    {
        public CrawledWindow(URLParentData parent)
        {
            InitializeComponent();

            tb_title.Text = parent.title;
            tb_url.Text = parent.url;
            tb_rootURL.Text = parent.rootURL;
            tb_crawledTime.Text = parent.urlCrawledTime.ToString();

            TimeSpan ts = new TimeSpan(parent.urlCrawledTime.Ticks - parent.urlRegisteredTime.Ticks);
            tb_duration.Text = ts.TotalSeconds + " s";

            foreach (URLChildData child in parent.childrenURLs)
                lb_children.Items.Add(child.url);
        }
    }
}
