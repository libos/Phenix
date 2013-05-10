using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Phenix.Core.Database
{
    interface IDatabaseHelper
    {
        object execute(string sql);
        DataSet getDataSet(string sql);
        void saveProto(string tablename, string primaryKey, params string[] ValueList);
        void sendProtoFile();
        void readyReceive();
        void receiveRDBFile();


    }
}
