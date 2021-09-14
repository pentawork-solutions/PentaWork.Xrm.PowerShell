using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PentaWork.Xrm.PowerShell.XrmProxies
{
    public static class UniqueNameExtensions
    {
        private static UniqueNameDictionary _entityNames = new UniqueNameDictionary();

        public static string GetUniqueName(this EntityMetadata entityMetadata)
        {
            return _entityNames.GetUniqueName(entityMetadata);
        }
    }
}
