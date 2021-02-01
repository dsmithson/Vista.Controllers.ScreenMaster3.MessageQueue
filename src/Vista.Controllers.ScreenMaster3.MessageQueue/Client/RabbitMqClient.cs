using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vista.Controllers.ScreenMaster3.MessageQueue.Messages;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Client
{
    /// <summary>
    /// Client interface for consuming ScreenMaster events via RabbitMQ
    /// </summary>
    public class RabbitMqClient
    {
        private readonly ConnectionFactory rabbitMqFactory;

        private IConnection rabbitConnection;
        private IModel rabbitChannel;
        private AsyncEventingBasicConsumer consumer;
        private readonly JsonSerializerOptions jsonOptions;

        public event KeyActionEventHandler KeyAction;
        protected void OnKeyAction(KeyActionEventArgs e)
        {
            KeyAction?.Invoke(this, e);
        }

        public event JoyStickActionEventHandler JoystickAction;
        protected void OnJoystickAction(JoystickActionEventArgs e)
        {
            JoystickAction?.Invoke(this, e);
        }

        public event RotaryActionEventHandler RotaryAction;
        protected void OnRotaryAction(RotaryActionEventArgs e)
        {
            RotaryAction?.Invoke(this, e);
        }

        public event TBarActionEventHandler TBarAction;
        protected void OnTBarAction(TBarActionEventArgs e)
        {
            TBarAction?.Invoke(this, e);
        }

        public bool IsRunning { get; private set; }

        public RabbitMqClient(string rabbitMqHost = "localhost", string rabbitMqUser = "devtest", string rabbitMqPassword = "devtest", string rabbitMqVHost = "/")
        {
            //Create our RabbitMQ connection factory
            rabbitMqFactory = new ConnectionFactory()
            {
                HostName = rabbitMqHost,
                UserName = rabbitMqUser,
                Password = rabbitMqPassword,
                VirtualHost = rabbitMqVHost,
                DispatchConsumersAsync = true
            };

            jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false)
                }
            };
        }

        public async Task<bool> Startup()
        {
            await ShutdownAsync();
            IsRunning = true;

            //Startup RabbitMQ connection
            rabbitConnection = rabbitMqFactory.CreateConnection();
            rabbitChannel = rabbitConnection.CreateModel();

            rabbitChannel.ExchangeDeclare(RoutingAddressMap.Exchange, RoutingAddressMap.ExchangeType);

            //Bind to event queues
            var consoleEventQueue = rabbitChannel.QueueDeclare();
            rabbitChannel.QueueBind(consoleEventQueue.QueueName, RoutingAddressMap.Exchange, RoutingAddressMap.KeyActionRoutingKey);
            rabbitChannel.QueueBind(consoleEventQueue.QueueName, RoutingAddressMap.Exchange, RoutingAddressMap.JoystickActionRoutingKey);
            rabbitChannel.QueueBind(consoleEventQueue.QueueName, RoutingAddressMap.Exchange, RoutingAddressMap.RotaryActionRoutingKey);
            rabbitChannel.QueueBind(consoleEventQueue.QueueName, RoutingAddressMap.Exchange, RoutingAddressMap.TBarActionRoutingKey);

            consumer = new AsyncEventingBasicConsumer(rabbitChannel);
            consumer.Received += Consumer_Received;
            rabbitChannel.BasicConsume(consoleEventQueue.QueueName, false, consumer);

            return true;
        }

        public Task ShutdownAsync()
        {
            if(consumer != null)
            {
                consumer.Received -= Consumer_Received;
                consumer = null;
            }

            if(rabbitChannel != null)
            {
                rabbitChannel.Close();
                rabbitChannel.Dispose();
                rabbitChannel = null;
            }

            if(rabbitConnection != null)
            {
                rabbitConnection.Close();
                rabbitConnection.Dispose();
                rabbitConnection = null;
            }

            return Task.FromResult(true);
        }

        public void SetLamps(bool isOn, params int[] keyIndexes)
        {
            if (keyIndexes == null || keyIndexes.Length == 0)
                return;

            var msg = new LampCommandMessage()
            {
                Command = isOn ? LampCommandType.SetOn : LampCommandType.SetOff,
                KeyIndexes = new List<int>(keyIndexes)
            };
            BasicPublish(RoutingAddressMap.LampCommandRoutingKey, msg);
        }

        public void ClearAllLamps()
        {
            var msg = new LampCommandMessage()
            {
                Command = LampCommandType.ClearAll
            };
            BasicPublish(RoutingAddressMap.LampCommandRoutingKey, msg);
        }

        public void SetQuickKeys(params QuickKeyButton[] buttons)
        {
            if (buttons == null || buttons.Length == 0)
                return;

            var msg = new QuickKeyMessage()
            {
                Buttons = new List<QuickKeyButton>(buttons)
            };
            BasicPublish(RoutingAddressMap.QuickKeyCommandRoutingKey, msg);
        }

        public void BasicPublish<T>(string routingKey, T obj)
        {
            if (IsRunning)
            {
                string msgText = JsonSerializer.Serialize(obj, jsonOptions);
                byte[] msg = Encoding.UTF8.GetBytes(msgText);
                rabbitChannel.BasicPublish(RoutingAddressMap.Exchange, routingKey, null, msg);
            }
        }

        /// <summary>
        /// Command handler
        /// </summary>
        private Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                switch (e.RoutingKey)
                {
                    case RoutingAddressMap.KeyActionRoutingKey:
                        {
                            DeserializeAndExecute<KeyActionMessage>(e.Body, (msg) => OnKeyAction(new KeyActionEventArgs(msg.KeyIndex, msg.IsPressed)));
                            break;
                        }

                    case RoutingAddressMap.JoystickActionRoutingKey:
                        {
                            DeserializeAndExecute<JoystickActionMessage>(e.Body, (msg) => OnJoystickAction(new JoystickActionEventArgs(msg.X, msg.Y, msg.Z)));
                            break;
                        }

                    case RoutingAddressMap.TBarActionRoutingKey:
                        {
                            DeserializeAndExecute<TBarActionMessage>(e.Body, (msg) => OnTBarAction(new TBarActionEventArgs(msg.TBarPosition)));
                            break;
                        }

                    case RoutingAddressMap.RotaryActionRoutingKey:
                        {
                            DeserializeAndExecute<RotaryActionMessage>(e.Body, (msg) => OnRotaryAction(new RotaryActionEventArgs(msg.RotaryIndex, msg.RotaryOffset)));
                            break;
                        }

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name} occurred while processing message: {ex.Message}");
            }
            finally
            {
                //Ack our processed message
                rabbitChannel.BasicAck(e.DeliveryTag, false);
            }

            return Task.FromResult(true);
        }

        private void DeserializeAndExecute<T>(ReadOnlyMemory<byte> message, Action<T> action)
        {
            try
            {
                string msg = Encoding.UTF8.GetString(message.Span);
                var obj = JsonSerializer.Deserialize<T>(msg);
                action(obj);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name} occurred while handling message: {ex.Message}");
            }
        }
    }
}
