using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfEntraLoginSample
{
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _config;
        private readonly IPublicClientApplication _pca;

        public MainWindow()
        {
            InitializeComponent();

            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                .Build();

            var clientId = _config["Entra:ClientId"];
            var tenantId = _config["Entra:TenantId"];

            _pca = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                .WithDefaultRedirectUri()
                .Build();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await SignInAsync();
        }

        private async Task SignInAsync()
        {
            try
            {
                StatusTextBlock.Text = "ログイン中...";
                UserTextBlock.Text = "";

                string[] scopes = { "User.Read" };

                var accounts = await _pca.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();

                AuthenticationResult result;

                try
                {
                    result = await _pca
                        .AcquireTokenSilent(scopes, firstAccount)
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                    result = await _pca
                        .AcquireTokenInteractive(scopes)
                        .ExecuteAsync();
                }

                StatusTextBlock.Text = "ログイン成功";
                UserTextBlock.Text = $"ユーザー: {result.Account?.Username}";
            }
            catch (System.Exception ex)
            {
                StatusTextBlock.Text = "ログイン失敗";
                MessageBox.Show(ex.Message, "エラー");
            }
        }
    }
}