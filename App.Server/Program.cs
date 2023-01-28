using App.Business.Concrete;
using App.DataAccess.Concrete.EfEntityFramework;
using App.Entities.Concrete;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace App.Server
{
    public class Program
    {
        static TcpListener listener = null;
        static BinaryWriter bw = null;
        static BinaryReader br = null;
        public static List<TcpClient> Clients { get; set; }
        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
        static void Main(string[] args)
        {

            Clients = new List<TcpClient>();
            var ip = IPAddress.Parse("10.2.11.19");
            var port = 27001;

            var ep = new IPEndPoint(ip, port);
            listener = new TcpListener(ep);
            listener.Start();
            string nspace = "App.Business.Concrete";
            var productService = new ProductService(new EfProductDal());


            Console.WriteLine($"Listening on {listener.LocalEndpoint}");
            while (true)
            {
                var client = listener.AcceptTcpClient();
                Clients.Add(client);
                Console.WriteLine($"{client.Client.RemoteEndPoint}");
                Task.Run(() =>
                {
                    var reader = Task.Run(() =>
                    {
                        foreach (var item in Clients)
                        {
                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    try
                                    {
                                        var stream = item.GetStream();
                                        br = new BinaryReader(stream);
                                        bw = new BinaryWriter(stream);
                                        var msg = br.ReadString();
                                        Console.WriteLine($"CLIENT : {client.Client.RemoteEndPoint} : {msg}");
                                        //Products/1

                                        if (msg != String.Empty)
                                        {
                                            var className = msg.Split('\\')[0];
                                            var methodName = msg.Split('\\')[1];
                                            var myType = Assembly.GetAssembly(typeof(ProductService)).GetTypes()
                                            .FirstOrDefault(a => a.FullName.Contains(className));

                                            var methods = myType.GetMethods();
                                            MethodInfo myMethod = myType.GetMethods()
                                            .FirstOrDefault(m=>m.Name.Contains(methodName));

                                            object myInstance = Activator.CreateInstance(myType);
                                            var products = myMethod.Invoke(myInstance, null);
                                            //var products = productService.GetAll();
                                            var jsonString = JsonConvert.SerializeObject(products);
                                            bw.Write(jsonString);
                                        }
                                        stream.Flush();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"{item.Client.RemoteEndPoint}  disconnected");
                                        Clients.Remove(item);
                                    }
                                }
                            }).Wait(50);
                        }

                        //var stream = client.GetStream();
                        //br = new BinaryReader(stream);
                        //bw = new BinaryWriter(stream);
                        //while (true)
                        //{
                        //    try
                        //    {

                        //        //Products/1
                        //        var msg = br.ReadString();
                        //        Console.WriteLine($"CLIENT : {client.Client.RemoteEndPoint} : {msg}");
                        //        if (msg == @"Product\Getlist")
                        //        {
                        //            var products = productService.GetAll();
                        //            var jsonString = JsonConvert.SerializeObject(products);
                        //            bw.Write(jsonString);

                        //        }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        Console.WriteLine(ex.Message);
                        //    }
                        //}
                    });

                    var writer = Task.Run(() =>
                    {
                        //var stream = client.GetStream();
                        //bw = new BinaryWriter(stream);
                        //while (true)
                        //{
                        //    var msg = Console.ReadLine();
                        //    bw.Write(msg);
                        //}

                        while (true)
                        {
                            var msg = Console.ReadLine();
                            foreach (var item in Clients)
                            {
                                var stream = item.GetStream();
                                bw = new BinaryWriter(stream);
                                bw.Write(msg);
                            }
                            foreach (var item in Clients)
                            {
                                if (item.Connected)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                }
                                Console.WriteLine($"item : {item.Client.RemoteEndPoint}");
                                Console.ResetColor();
                            }
                        }
                    });
                });
            }
        }
    }
}



