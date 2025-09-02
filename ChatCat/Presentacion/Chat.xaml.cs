using ChatCat.Clases;
using ChatCat.Datos;
using ChatCat.Presentacion;
using System.Collections.ObjectModel;
using System.Media;
using System.Windows;
using System.Windows.Interop;

namespace ChatCat
{
    public partial class Chat : Window
    {
        private ConexionCat conexionCat;

        public ObservableCollection<ChatMessage> chatMessages { get; set; } = new();
        public ObservableCollection<User> usersList { get; set; } = new();

        public Chat(ConexionCat _conexionCat)
        {
            InitializeComponent();
            conexionCat = _conexionCat;

            listMessages.ItemsSource = chatMessages;
            listUsers.ItemsSource = usersList;

            conexionCat.OnMessageReceived += async msg =>
            {
                Dispatcher.Invoke(() =>
                {
                    chatMessages.Add(msg);

                    listMessages.ScrollIntoView(chatMessages.Last());
                });
            };

            conexionCat.OnUserListUpdated += users =>
            {
                Dispatcher.Invoke(() =>
                {
                    usersList.Clear();
                    foreach (var user in users)
                    {
                        usersList.Add(user);
                    }
                });
            };
        }

        private async void Chat_Loaded(object sender, RoutedEventArgs e)
        {
            await conexionCat.RequestUserListAsync();

            base.OnSourceInitialized(e);

            // Activamos el dark mode del titlebar
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            IntPtr handle = source.Handle;
            DwmApi.UseImmersiveDarkMode(handle, true);
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                SystemSounds.Hand.Play();
                return;
            }

            var message = MessageInput.Text;

            await conexionCat.SendMessageAsync(message);
            MessageInput.Clear();
        }

        private void ButtonDesconectar(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("¿Estas seguro de que te quieres desconectar del chat?", "Cerrando Sesión", MessageBoxButton.YesNo, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    conexionCat.DisconnectAsync();

                    var loginWindow = new Login();
                    loginWindow.Show();
                    
                    this.Close();
                    
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }

        private void MessageInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendMessage_Click(null, null);
            }
        }
    }
}
