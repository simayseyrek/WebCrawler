using crawlerUtilities;
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
using System.Windows.Threading;

namespace crawlerUI
{
    /// <summary>
    /// Interaction logic for RootWindow.xaml
    /// </summary>
    public partial class RootWindow : Window
    {
        private DispatcherTimer timerUpdateGUI;
        private CrawlerBackend crawlerBE;
        private URLRootData root;

        public RootWindow(URLRootData root, CrawlerBackend crawlerBE)
        {
            InitializeComponent();

            this.root = root;
            this.crawlerBE = crawlerBE;

            tb_rootURL.Text = root.rootURL;
            tb_maxThreadNo.Text = root.maxThreadNo.ToString();
            tb_extActive.Text = root.externalActivated.ToString();

            timerUpdateGUI = new DispatcherTimer(); // 2019103041
            timerUpdateGUI.Tick += timerUpdateGUI_Tick;
            timerUpdateGUI.Interval = new TimeSpan(0, 0, 0, 0, 20);  // 2019103040
            timerUpdateGUI.Start();
        }

        private void timerUpdateGUI_Tick(object sender, EventArgs e)
        {
            var activelyRunningTupleList = crawlerBE.GetActiveRunTupleList(root.rootURL);

            l_activelyRunningTasks.Content = "Actively Running Tasks (" + activelyRunningTupleList.Count + ")";

            lv_activelyRunningTasks.Items.Clear();

            foreach (Tuple<string, Task, string> tuple in activelyRunningTupleList)
                lv_activelyRunningTasks.Items.Add(tuple);
        }
    }
}
