using Network_Chat_task_TCP_server.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using Network_Chat_task_TCP_server.Models;
using Newtonsoft.Json;
using Network_Chat_task_TCP_server.Views;
using Network_Chat_task_TCP_server.Views.UserControls;
using System.Windows.Interop;

namespace Network_Chat_task_TCP_server.ViewModels
{

    public class MainViewModel : BaseViewModel
    {
        static TcpListener _listener = null;
        static BinaryWriter bw = null;
        static BinaryReader br = null;
        public RelayCommand ConnectServerCommand { get; set; }
        public RelayCommand SelectedUserChangedCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }


        private ObservableCollection<User> _allUsers = new ObservableCollection<User>();

        public ObservableCollection<User> AllUsers
        {
            get { return _allUsers; }
            set { _allUsers = value; OnPropertyChanged(); }
        }

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; OnPropertyChanged(); }
        }

        private User selectedUser;

        public User SelectedUser
        {
            get { return selectedUser; }
            set { selectedUser = value; OnPropertyChanged(); }
        }

        private bool _serverUpIsEnabled = true;

        public bool ServerUpIsEnabled
        {
            get { return _serverUpIsEnabled; }
            set { _serverUpIsEnabled = value; OnPropertyChanged(); }
        }


        static List<TcpClient> users = new List<TcpClient>();


        public MainViewModel()
        {
            var ipAdressRemote = IPAddress.Parse("10.2.27.3");
            var port = 27001;

            SelectedUserChangedCommand = new RelayCommand((_) =>
            {

                App.MessageWrapPanel.Children.Clear();
                ChatUC chatUC = new ChatUC();
                ChatUcViewModel chatUcViewModel = new ChatUcViewModel();
                chatUcViewModel.UserName = SelectedUser.Name;
                chatUC.DataContext = chatUcViewModel;
                App.MessageWrapPanel.Children.Add(chatUC);

                chatUcViewModel.SendCommand = new RelayCommand((__) =>
                {
             
                    Task.Run(() =>
                    {
             
             
                         for (int i = 0; i < users.Count; i++)
                        {
                            if (SelectedUser.EndPoint == users[i].Client.RemoteEndPoint.ToString())
                            {
                                var client = users[i];
                                
                                var stream = client.GetStream();
                                bw = new BinaryWriter(stream);

                                
                                if (chatUcViewModel.UserMessage != String.Empty)
                                {
                                    bw.Write(chatUcViewModel.UserMessage);
                                    
                                    App.Current.Dispatcher.Invoke((System.Action)delegate
                                    {
                                        EachMessageUcViewModel eachMessageUcViewModel = new EachMessageUcViewModel();
                                        EachMessageUC eachMessageUC = new EachMessageUC();
                                        eachMessageUcViewModel.Message = chatUcViewModel.UserMessage;
                                        eachMessageUC.DataContext = eachMessageUcViewModel;
                                        
                                        App.UserMessageWrapPanel.Children.Add(eachMessageUC);
                                        
                                    });
                                    chatUcViewModel.UserMessage = String.Empty;
                                }
                             
                            }
                            //}yoxladim
                            ////var client=_listener.
                            //Task.Run(() =>
                            //{
                            //    //App.Current.Dispatcher.Invoke((System.Action)delegate
                            //    //{
                            //    //var reader = Task.Run(() =>
                            //    //{
                            //    //    var stream = client.GetStream();
                            //    //    br = new BinaryReader(stream);
                            //    //    while (true)
                            //    //    {

                            //    //        var msg = br.ReadString();

                            //    //        var user = JsonConvert.DeserializeObject<User>(msg);

                            //    //        //App.Current.Dispatcher.Invoke((System.Action)delegate
                            //    //        //{
                            //    //        //    AllUsers.Add(user);
                            //    //        //    //MessageBox.Show($"{user.Name} adli kisi sayfaya eklendi");
                            //    //        //    //MessageBox.Show($"{AllUsers.Count}");
                            //    //        //    //MessageBox.Show($"{client.Client.LocalEndPoint}");
                            //    //        //});
                            //    //        //try
                            //    //        //{
                            //    //        //    AllUsers.Add(user);
                            //    //        //}
                            //    //        //catch (Exception)
                            //    //        //{

                            //    //        //}
                            //    //    }
                            //    //});

                            //    //}
                            //    //});

                            //    //SendMessageCommand = new RelayCommand((__) =>
                            //    //{
                            //    //var writer = Task.Run(() =>
                            //    //{
                            //    //    var stream = client.GetStream();
                            //    //    bw = new BinaryWriter(stream);

                            //    //    while (true)
                            //    //    {
                            //    //        if (serverName != String.Empty)
                            //    //        {
                            //    //            bw.Write(chatUcViewModel.UserMessage);
                            //    //            //MessageBox.Show($"Send message : {serverName}");
                            //    //            serverName = String.Empty;
                            //    //        }
                            //    //    }
                            //    //});
                            //    //});
                            //});
                        }
                    });
                });
            });


            ConnectServerCommand = new RelayCommand((_) =>
            {
                var endPoint = new IPEndPoint(ipAdressRemote, port);
                ServerUpIsEnabled = false;

                _listener = new TcpListener(endPoint);

                _listener.Start();
                MessageBox.Show($"Server Start");
                User user = null;
                Task.Run(() =>
                {
                    while (true)
                    {
                        var client = _listener.AcceptTcpClient();
                        Task.Run(() =>
                        {
                           
                            var reader = Task.Run(() =>
                            {
                                var stream = client.GetStream();
                                br = new BinaryReader(stream);
                                while (true)
                                {
                                    try
                                    {
                                        var msg = br.ReadString();

                                        user = JsonConvert.DeserializeObject<User>(msg);

                                        App.Current.Dispatcher.Invoke((System.Action)delegate
                                        {
                                            AllUsers.Add(user);
                                            users.Add(client);
                                            
                                        });
                                      
                                    }
                                    catch (Exception)
                                    {
                                        App.Current.Dispatcher.Invoke((System.Action)delegate
                                        {
                                            AllUsers.Remove(user);
                                        });
                                    }
                                }
                            });

                        
                        });
                    }
                });
            });
        }
    }
}
