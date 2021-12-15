using System;
using System.Collections.Generic;
using Dinar;
using LiteDB;

namespace DinarConsole
{
    public class DinarConsole
    {
        public static void Main(string[] args)
        {
            

            List<Dinar.Blockchain.Block> blockchain = new List<Dinar.Blockchain.Block>();

            Dinar.Blockchain.TransactionPool transactionPool = new Dinar.Blockchain.TransactionPool();
            Dinar.Miner.BlockMiner blockMiner = new Dinar.Miner.BlockMiner(blockchain, transactionPool);


            Dinar.EmbedServer server = new EmbedServer(blockMiner);

            server.Start();


            Console.ReadKey();

        }
    }
}
