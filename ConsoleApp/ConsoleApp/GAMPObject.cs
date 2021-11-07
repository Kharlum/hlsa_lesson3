using System;
using System.Collections.Generic;

namespace ConsoleApp
{
    public class GAMPObject
    {
        public GAMPObject(string value)
        {
            el = value;
        }

        public string v { get; } = "1";

        public string tid { get; } = "UA-212171238-1";

        public Guid cid { get; } = Guid.NewGuid();

        public string t { get; } = "event";

        public string ec { get; } = "currency_exchange";

        public string ea { get; } = "uah/usd";

        public string el { get; }

        public Dictionary<string, string> AsDictionary()
        {
            return new Dictionary<string, string>
            {
                { nameof(v), v },
                { nameof(tid), tid },
                { nameof(cid), cid.ToString() },
                { nameof(t), t },
                { nameof(ec), ec },
                { nameof(ea), ea },
                { nameof(el), el },
            };
        }
    }
}
