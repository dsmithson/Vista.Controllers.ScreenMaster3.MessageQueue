﻿using Spyder.Controllers.ScreenMaster3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Vista.Controllers.ScreenMaster3.MessageQueue.Messages;
using System.IO;
using System.Text.Json.Serialization;

namespace Vista.Controllers.ScreenMaster3.MessageQueue
{
    /// <summary>
    /// Provides interafce to translate between ScreenMaster3 UDP messages and RabbitMQ
    /// </summary>
    public class RabbitMqServer
    {
        private readonly KeyboardInterface keyboard;
        private readonly ConnectionFactory rabbitMqFactory;
        private readonly JsonSerializerOptions jsonOptions;

        private IConnection rabbitConnection;
        private IModel rabbitChannel;
        private AsyncEventingBasicConsumer consumer;

        public bool IsRunning { get; private set; }

        public RabbitMqServer(string rabbitMqHost = "localhost", string rabbitMqUser = "devtest", string rabbitMqPassword = "devtest", string rabbitMqVHost = "/")
        {
            //Configure our keyboard
            keyboard = new KeyboardInterface();
            keyboard.KeyAction += Keyboard_KeyAction;
            keyboard.RotaryValueChanged += Keyboard_RotaryValueChanged;
            keyboard.TBarValueChanged += Keyboard_TBarValueChanged;
            keyboard.JoystickValueChanged += Keyboard_JoystickValueChanged;

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
                    new JsonStringEnumConverter()
                }
            };
        }

        public async Task<bool> Startup()
        {
            await ShutdownAsync().ConfigureAwait(false);
            IsRunning = true;

            //Startup keyboard
            if (!await keyboard.StartupAsync().ConfigureAwait(false))
            {
                await ShutdownAsync().ConfigureAwait(false);
                return false;
            }

            //Startup RabbitMQ connection
            rabbitConnection = rabbitMqFactory.CreateConnection();
            rabbitChannel = rabbitConnection.CreateModel();

            //Send messages on 
            rabbitChannel.ExchangeDeclare(RoutingAddressMap.Exchange, RoutingAddressMap.ExchangeType);

            var lampCommandQueue = rabbitChannel.QueueDeclare();
            rabbitChannel.QueueBind(lampCommandQueue.QueueName, RoutingAddressMap.Exchange, RoutingAddressMap.LampCommandRoutingKey);

            var quickKeyCommandQueue = rabbitChannel.QueueDeclare();
            rabbitChannel.QueueBind(quickKeyCommandQueue.QueueName, RoutingAddressMap.Exchange, RoutingAddressMap.QuickKeyCommandRoutingKey);

            consumer = new AsyncEventingBasicConsumer(rabbitChannel);
            consumer.Received += Consumer_Received;
            rabbitChannel.BasicConsume(lampCommandQueue.QueueName, false, consumer);
            rabbitChannel.BasicConsume(quickKeyCommandQueue.QueueName, false, consumer);

            return true;
        }

        public async Task ShutdownAsync()
        {
            IsRunning = false;

            //Shutdown keybaord
            await keyboard.ShutdownAsync().ConfigureAwait(false);

            //Shutdown rabbit
            if (consumer != null)
            {
                consumer.Received -= Consumer_Received;
                consumer = null;
            }

            if (rabbitChannel != null)
            {
                rabbitChannel.Close();
                rabbitChannel.Dispose();
                rabbitChannel = null;
            }

            if (rabbitConnection != null)
            {
                rabbitConnection.Close();
                rabbitConnection.Dispose();
                rabbitConnection = null;
            }
        }

        #region Action Handlers

        private void Keyboard_TBarValueChanged(object sender, KeyboardTBarEventArgs e)
        {
            var msg = new TBarActionMessage()
            {
                TBarPosition = e.TBarPosition
            };
            BasicPublish(RoutingAddressMap.TBarActionRoutingKey, msg);
        }

        private void Keyboard_RotaryValueChanged(object sender, KeyboardRotaryEventArgs e)
        {
            var msg = new RotaryActionMessage()
            {
                RotaryIndex = e.RotaryIndex,
                RotaryOffset = e.RotaryOffset
            };
            BasicPublish(RoutingAddressMap.RotaryActionRoutingKey, msg);
        }

        private void Keyboard_KeyAction(object sender, KeyboardKeyEventArgs e)
        {
            var msg = new KeyActionMessage()
            {
                KeyIndex = e.KeyIndex,
                IsPressed = e.IsPressed
            };
            BasicPublish(RoutingAddressMap.KeyActionRoutingKey, msg);
        }

        private void Keyboard_JoystickValueChanged(object sender, KeyboardJoystickEventArgs e)
        {
            var msg = new JoystickActionMessage()
            {
                X = e.X,
                Y = e.Y,
                Z = e.Z
            };
            BasicPublish(RoutingAddressMap.JoystickActionRoutingKey, msg);
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

        #endregion

        /// <summary>
        /// Command handler
        /// </summary>
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                if (e.RoutingKey == RoutingAddressMap.LampCommandRoutingKey)
                {
                    var msg = Deserialize<LampCommandMessage>(e.Body);
                    if (msg.Command == LampCommandType.ClearAll)
                    {
                        keyboard.ClearAllPushButtonsAndLedButtons();
                        await keyboard.UpdateLampsAsync();
                        await keyboard.UpdateAllDisplaysAsync();
                    }
                    else
                    {
                        bool on = msg.Command == LampCommandType.SetOn;
                        foreach (int keyIndex in msg.KeyIndexes)
                        {
                            keyboard.SetPushButtonLamp(keyIndex, on);
                        }
                        await keyboard.UpdateLampsAsync().ConfigureAwait(false);
                    }
                }
                else if (e.RoutingKey == RoutingAddressMap.QuickKeyCommandRoutingKey)
                {
                    var msg = Deserialize<QuickKeyMessage>(e.Body);
                    foreach (var button in msg.Buttons)
                    {
                        keyboard.SetLcdButton(button.Index, button.Color.Convert(), button.Text);
                    }

                    //Update buttons
                    List<int> rowsToUpdate = msg.Buttons.Select(b => b.Index / 8).Distinct().ToList();
                    if (msg.Buttons.Count == 1)
                    {
                        await keyboard.UpdateOneDisplayAsync(msg.Buttons[0].Index).ConfigureAwait(false);
                    }
                    else if (rowsToUpdate.Count == 1)
                    {
                        await keyboard.UpdateDisplayRowAsync(rowsToUpdate[0]).ConfigureAwait(false);
                    }
                    else
                    {
                        await keyboard.UpdateAllDisplaysAsync().ConfigureAwait(false);
                    }

                    //Need to update lamps to update LCD button colors
                    await keyboard.UpdateLampsAsync().ConfigureAwait(false);
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
        }

        private T Deserialize<T>(ReadOnlyMemory<byte> message)
        {
            string msg = Encoding.UTF8.GetString(message.Span);
            var obj = JsonSerializer.Deserialize<T>(msg, jsonOptions);
            return obj;
        }
    }
}
