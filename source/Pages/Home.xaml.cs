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
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using S3stat.SecureSetup.Content;

namespace S3stat.SecureSetup.Pages
{
	/// <summary>
	/// Interaction logic for Home.xaml
	/// </summary>
	public partial class Home : UserControl
	{
		public Home()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			//ErrorDetail.ShowMessage("hey", "what", new Exception("Amazon.S3.AmazonS3Exception: Access Denied"));


			var uri = new Uri("/Pages/S3statLogin.xaml", UriKind.Relative);
			var frame = NavigationHelper.FindFrame(null, this);
			frame.Source = uri;
		}
	}
}
