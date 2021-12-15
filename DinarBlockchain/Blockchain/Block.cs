using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinar.Blockchain
{
    public class Block
    {
        public long Index { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public long Nounce { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
