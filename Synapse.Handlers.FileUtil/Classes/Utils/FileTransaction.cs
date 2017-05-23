using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Alphaleonis.Win32.Filesystem;

namespace Synapse.Handlers.FileUtil
{
    public class FileTransaction
    {
        public bool IsStarted { get; set; } = false;
        public TransactionScope Scope { get; set; } = null;
        public  KernelTransaction Kernal = null;

        public void Start()
        {
            IsStarted = true;
            Scope = new TransactionScope(TransactionScopeOption.RequiresNew);
            Kernal = new KernelTransaction(Transaction.Current);
        }

        public void Stop()
        {
            IsStarted = false;
            Scope.Complete();

            Kernal.Dispose();
            Kernal = null;

            Scope.Dispose();
            Scope = null;
        }



    }
}
