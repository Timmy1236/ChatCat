using ChatCat.Datos;
using System.Windows;
using System.Windows.Interop;

namespace ChatCat.Presentacion
{
    public partial class Login : Window
    {
        private ConexionCat conexionCat;
        private bool isConnected = false;

        public Login()
        {
            InitializeComponent();
            conexionCat = new ConexionCat();

            conexionCat.OnConnected += (isConnected) =>
            {
                Dispatcher.Invoke(() =>
                {
                    this.isConnected = isConnected;

                    loginButton.IsEnabled = isConnected;
                    registerButton.IsEnabled = isConnected;
                });
            };
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario y una contraseña válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password.Trim();

            if (!isConnected)
            {
                MessageBox.Show("No puedes realizar esta acción ya que no te encuentras conectado al servidor.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Action<bool> loginResultHandler = null;

            loginResultHandler = (success) =>
            {
                Dispatcher.Invoke(() =>
                {
                    conexionCat.LoginResult -= loginResultHandler;
                    if (success)
                    {
                        var chatWindow = new Chat(conexionCat);
                        chatWindow.Show();

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Error de inicio de sesión", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            };

            conexionCat.LoginResult += loginResultHandler;

            await conexionCat.RequestUserLogin(username, password);
        }

        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Por favor, ingresa un nombre de usuario y una contraseña válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password.Trim();

            if (!isConnected)
            {
                MessageBox.Show("No puedes realizar esta acción ya que no te encuentras conectado al servidor.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Action<bool> registerResultHandler = null;

            registerResultHandler = (success) =>
            {
                Dispatcher.Invoke(() =>
                {
                    conexionCat.RegisterResult -= registerResultHandler;
                    if (success)
                    {
                        MessageBox.Show("Usuario registrado con éxito. Ahora puedes iniciar sesión.", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error al registrar el usuario. Intenta con otro nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            };

            conexionCat.RegisterResult += registerResultHandler;

            await conexionCat.RequestUserCreation(username, password);
        }

        private async void ConnectServerButton_Click(object sender, RoutedEventArgs e)
        {
            var server = txtServer.Text.Trim();

            if (string.IsNullOrWhiteSpace(txtServer.Text))
            {
                MessageBox.Show("Por favor, ingresa una dirección de servidor válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await conexionCat.ConnectAsync(server);

            ConnectServerButton.IsEnabled = false;
            txtServer.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            base.OnSourceInitialized(e);

            // Activamos el dark mode del titlebar
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            IntPtr handle = source.Handle;
            DwmApi.UseImmersiveDarkMode(handle, true);
        }
    }
}
