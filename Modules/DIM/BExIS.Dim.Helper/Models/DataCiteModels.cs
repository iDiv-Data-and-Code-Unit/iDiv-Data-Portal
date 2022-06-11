﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.Models
{
    public class DataCiteSettings
    {
        public List<DataCiteMapping> Mappings { get; set; }
    }

    public class DataCiteMapping
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool UseParty { get; set; }
        public Dictionary<string, string> PartyAttributes { get; set; }

        public DataCiteMapping(string name, string type, string value, bool useParty = false, Dictionary<string, string> partyAttributes = null)
        {
            Name = name;
            Type = type;
            Value = value;
            UseParty = useParty;
            PartyAttributes = partyAttributes;
        }
    }
}
