using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;

namespace Dinar.Blockchain
{
    public class Transaction
    {
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
        public Transaction(string from, string to, decimal amount)
        {
            this.From = from;
            this.To = to;
            this.Amount = amount;
        }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
    }
}

    

