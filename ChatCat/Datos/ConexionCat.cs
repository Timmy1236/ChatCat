using ChatCat.Clases;

// Esta clase manejará la conexión al servidor de CatCat, "ServerCat"
namespace ChatCat.Datos
{
    public class ConexionCat
    {
        private SocketIOClient.SocketIO client;
        public string Username { get; set; }

        public event Action<bool> OnConnected;
        public event Action<ChatMessage> OnMessageReceived;
        public event Action<User[]> OnUserListUpdated;

        public event Action<Result> LoginResult;
        public event Action<Result> RegisterResult;

        // Conetarnos al servidor
        public async Task ConnectAsync(string url)
        {
            client = new SocketIOClient.SocketIO(url);

            client.OnConnected += (sender, e) =>
            {
                OnConnected?.Invoke(true);
            };

            client.On("loginResult", response =>
            {
                var result = response.GetValue<Result>();
                LoginResult?.Invoke(result);
            });

            client.On("registerResult", response =>
            {
                var result = response.GetValue<Result>();
                RegisterResult?.Invoke(result);
            });

            client.On("chatMessage", response =>
            {
                var msg = response.GetValue<ChatMessage>();

                string from = msg.from;
                string text = msg.text;

                if (from == "System")
                {
                    msg.pfp = "pack://application:,,,/Assets/Images/ServerCatIcon.png";
                }
                else
                {
                    msg.pfp = "pack://application:,,,/Assets/Images/ChatCatIcon.png";
                }

                    OnMessageReceived?.Invoke(msg);
            });

            client.On("userList", response =>
            {
                var users = response.GetValue<User[]>();
                OnUserListUpdated?.Invoke(users);
            });

            await client.ConnectAsync();
        }

        public async Task RequestUserLogin(string username, string password)
        {
            await client.EmitAsync("login", username, password);
        }

        public async Task SendMessageAsync(string message)
        {
            await client.EmitAsync("chatMessage", message);
        }

        public async Task RequestUserListAsync()
        {
            await client.EmitAsync("getUserList");
        }

        public async Task RequestUserCreation(string username, string password)
        {
            await client.EmitAsync("register", username, password);
        }

        public async Task DisconnectAsync()
        {
            if (client != null)
            {
                await client.DisconnectAsync();
            }
        }
    }
}
