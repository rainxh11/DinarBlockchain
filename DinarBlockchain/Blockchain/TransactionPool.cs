using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinar.Blockchain
{
    public class TransactionPool
    {
        private List<Transaction> _transactions;

        private object _lock;

        public TransactionPool()
        {
            _lock = new object();
            _transactions = new List<Transaction>();
        }

        public void AddTransaction(Transaction transaction)
        {
            lock (_lock)
            {
                _transactions.Add(transaction);
            }
        }

        public List<Transaction> TakeAll()
        {
            lock (_lock)
            {
                var all = _transactions.ToList();
                _transactions.Clear();
                return all;
            }
        }
    }
}
