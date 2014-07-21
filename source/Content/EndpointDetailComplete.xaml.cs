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
using FirstFloor.ModernUI.Windows.Navigation;

namespace S3stat.SecureSetup.Content
{
	/// <summary>
	/// Interaction logic for EndpointDetailComplete.xaml
	/// </summary>
	public partial class EndpointDetailComplete : UserControl
	{
		public EndpointDetailComplete()
		{
			InitializeComponent();
		}

		private void btnContinue_Click(object sender, RoutedEventArgs e)
		{
			var uri = new Uri("/Pages/Finished.xaml", UriKind.Relative);
			var frame = NavigationHelper.FindFrame(null, this);
			frame.Source = uri;

		}
	}
}
