using BitcoinTransaction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinMonitor
{
    public class Startup
    {
        public static event EventHandler<RawTransaction> NewTransaction = (_, __) => { };
        public static decimal Value;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseStaticFiles();
            app.UseMvc();
            NewTransaction += Startup_NewTransaction;
            Task.Factory.StartNew(() => SubscribeForBitcoinTransactions("tcp://192.168.100.254:5557"));
        }

        private void Startup_NewTransaction(object sender, RawTransaction e)
        {
            Value += e.Outputs.Select(o => (decimal)o.Value / 100000000).Sum();
        }

        private void SubscribeForBitcoinTransactions(string address)
        {
            using (var reciever = new SubscriberSocket())
            {
                reciever.Connect(address);
                reciever.Subscribe("rawtx");

                while (true)
                {
                    var message = reciever.ReceiveMultipartBytes();
                    if (message.Count <= 1) continue; // first index is reserved for topic name
                    var data = new List<byte>();
                    for (int i = 1; i < message.Count; i++)
                    {
                        data.AddRange(message[i]);
                    }

                    var hex = BitConverter.ToString(data.ToArray()).Replace("-", "");
                    //https://github.com/lAnubisl/BitcoinTransaction
                    var rawTransaction = new RawTransaction(data.ToArray());
                    NewTransaction(this, rawTransaction);
                }
            }
        }
    }
}