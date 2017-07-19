using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for Chrome_History_Window.xaml
    /// </summary>
    public partial class Chrome_History_Window : Window
    {
        public Chrome_History_Window()
        {
            InitializeComponent();
        }

        private void gv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (gv.SelectedItem == null)
                {
                    return;
                }
                DataRowView dr = gv.SelectedItem as DataRowView;
                DataRow myRow = dr.Row;
                using (Process p = new Process())
                {
                    Process.Start("firefox.exe", myRow[1].ToString());
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
