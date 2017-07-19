using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp3
{
    class DictionaryHelper<T, O>
    {
        protected string Type;
        protected DataTable outputTable;

        public DictionaryHelper(string myType, DataTable myTable)
        {
            this.Type = myType;
            outputTable = myTable;
        }
    }
}
