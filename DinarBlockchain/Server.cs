using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using Dinar.Blockchain;
using Dinar.Miner;
using EmbedIO.WebApi;
using EmbedIO.Routing;
using Newtonsoft.Json;
using EmbedIO.Actions;

namespace Dinar
{
    public class EmbedServer
    {
        private readonly BlockMiner blockMiner;
        private readonly TransactionPool transactionPool;

        private WebServer server;
        private string url;
        public EmbedServer(BlockMiner blockMiner)
        {
            url = $"http://localhost:9500/";

            server = CreateWebServer(url);
            this.transactionPool = blockMiner.TransactionPool;
            this.blockMiner = blockMiner;
        }
        public void Stop()
        {
            server.Dispose();
        }
        public void Start()
        {
            server.RunAsync();
            blockMiner.Start(1);
        }

        private WebServer CreateWebServer(string url)
        {
            var server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m.WithController(() => new Controller(blockMiner, transactionPool)))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));

            return server;
        }

        public sealed class Controller : WebApiController
        {
            private readonly BlockMiner blockMiner;
            private readonly TransactionPool transactionPool;

            public Controller(BlockMiner blockMiner, TransactionPool transactionPool)
            {
                this.blockMiner = blockMiner;
                this.transactionPool = transactionPool;
            }

            [Route(HttpVerbs.Get, "/blocks")]
            public string GetAllBlocks() => JsonConvert.SerializeObject(blockMiner.Blockchain, Formatting.Indented);

            [Route(HttpVerbs.Get, "/blocks/index/{index?}")]
            public string GetAllBlocks(int index)
            {
                Block block = null;
                if (index < blockMiner.Blockchain.Count)
                    block = blockMiner.Blockchain[index];
                return JsonConvert.SerializeObject(block, Formatting.Indented);
            }

            [Route(HttpVerbs.Get, "/blocks/latest")]
            public string GetLatestBlocks()
            {
                var block = blockMiner.Blockchain.LastOrDefault();
                return JsonConvert.SerializeObject(block, Formatting.Indented);
            }

            [Route(HttpVerbs.Post, "/add")]
            public async Task AddTransaction()
            {
                var request = await HttpContext.GetRequestBodyAsStringAsync();
                var data = JsonConvert.DeserializeObject<Transaction>(request);
                if (data != null && data != null)
                    transactionPool.AddTransaction(data);
            }
        }
    }
}
