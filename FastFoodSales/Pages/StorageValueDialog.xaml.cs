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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DAQ.Pages
{
    /// <summary>
    /// StorageValueDialog.xaml 的交互逻辑
    /// </summary>
    public partial class StorageValueDialog : UserControl
    {
        public StorageValueDialog()
        {
            InitializeComponent();
        }
        



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = sender as ComboBox;
            if (s == null)
                return;
            var selectvalue = (DAQ.Service.TYPE)(((ComboBox)sender)?.SelectedValue??Service.TYPE.SHORT);
            if (selectvalue == Service.TYPE.STRING)
            {
                Len.Visibility = Visibility.Visible;
                BitIndex.Visibility = Visibility.Collapsed;
            }
            else if (selectvalue == Service.TYPE.BOOL)
            {
                BitIndex.Visibility = Visibility.Visible;
                Len.Visibility = Visibility.Collapsed;
            }
            else
            {
                Len.Visibility = Visibility.Collapsed;
                BitIndex.Visibility = Visibility.Collapsed;
            }
        }
    }
}
