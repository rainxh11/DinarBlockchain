using Dinar.Blockchain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blake3;
using Newtonsoft.Json;
using System.Threading;
using Konscious.Security.Cryptography;
using SimpleBase;

namespace Dinar.Miner
{
    public class BlockMiner
    {
        private static char MagicCharacter = 'A';
        public TransactionPool TransactionPool { get; private set; }
        public List<Block> Blockchain { get; private set; }
        private CancellationTokenSource cancellationToken;

        public BlockMiner(List<Block> blockchain, TransactionPool transactionPool)
        {
            this.Blockchain = blockchain;
            this.TransactionPool = transactionPool;
        }

        public static string CalculateHash(string data)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(data));

            argon2.Salt = Encoding.UTF8.GetBytes(GetSalt(data));
            argon2.DegreeOfParallelism = Environment.ProcessorCount - 1;
            argon2.Iterations = 4;
            argon2.MemorySize = 256 * 256;

            return Base58.Bitcoin.Encode(argon2.GetBytes(64));
        }
        
        public static string GetSalt(string data)
        {
            return Blake3.Hasher.Hash(Encoding.UTF8.GetBytes(data)).ToString();
        }
        private string FindMerkleRootHash(IList<Transaction> transactionList)
        {
            var transactionStrList = transactionList.Select(tran => CalculateHash(CalculateHash(tran.From + tran.To + tran.Amount))).ToList();
            return BuildMerkleRootHash(transactionStrList);
        }

        private string BuildMerkleRootHash(IList<string> merkelLeaves)
        {
            if (merkelLeaves == null || !merkelLeaves.Any())
                return string.Empty;

            if (merkelLeaves.Count() == 1)
                return merkelLeaves.First();

            if (merkelLeaves.Count() % 2 > 0)
                merkelLeaves.Add(merkelLeaves.Last());

            var merkleBranches = new List<string>();

            for (int i = 0; i < merkelLeaves.Count(); i += 2)
            {
                var leafPair = string.Concat(merkelLeaves[i], merkelLeaves[i + 1]);
                merkleBranches.Add(CalculateHash(CalculateHash(leafPair)));
            }
            return BuildMerkleRootHash(merkleBranches);
        }
        private void GenerateBlock(int difficulty)
        {
            var lastBlock = Blockchain.LastOrDefault();
            var block = new Block()
            {
                TimeStamp = DateTime.Now,
                Nounce = 0,
                Transactions = TransactionPool.TakeAll(),
                Index = (lastBlock?.Index + 1 ?? 0),
                PreviousHash = lastBlock?.Hash ?? string.Empty
            };
            DateTime miningStart = DateTime.Now;

            MineBlock(block, difficulty);

            DateTime miningEnd = DateTime.Now;

            Console.WriteLine($"Block {block.Index:N0} Mined. Took: {(miningEnd - miningStart).TotalSeconds:N3} seconds.");
            Console.WriteLine(JsonConvert.SerializeObject(block, Formatting.Indented));

            Blockchain.Add(block);
        }

        private void MineBlock(Block block, int difficulty = 4)
        {
            var merkleRootHash = FindMerkleRootHash(block.Transactions);
            long nounce = -1;
            var hash = string.Empty;
            do
            {
                nounce++;
                var rowData = block.Index + block.PreviousHash + block.TimeStamp.ToString() + nounce + merkleRootHash;
                hash = CalculateHash(CalculateHash(rowData));
            }
            while (!hash.StartsWith(MagicCharacter.ToString().PadLeft(difficulty, MagicCharacter)));
            block.Hash = hash;
            block.Nounce = nounce;
        }
        public void Start(int difficulty)
        {
            cancellationToken = new CancellationTokenSource();
            Task.Run(() => DoGenerateBlock(difficulty), cancellationToken.Token);
            Console.WriteLine("Mining has started");
        }
        public void Stop()
        {
            cancellationToken.Cancel();
            Console.WriteLine("Mining has stopped");
        }

        private void DoGenerateBlock(int difficulty)
        {
            while (true)
            {
                var startTime = DateTime.Now;
                this.GenerateBlock(difficulty);
                var endTime = DateTime.Now;
                var remainTime = TimeSpan.FromSeconds(5) - (endTime - startTime);
                Thread.Sleep(remainTime.Seconds < 0 ? 0 : remainTime.Seconds);
            }
        }
    }
}
