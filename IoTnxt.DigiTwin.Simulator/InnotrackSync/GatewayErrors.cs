using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTnxt.DigiTwin.Simulator.InnotrackSync
{
   public static class GatewayErrors
    {
        private static List<string> lst;
        public static List<string> ErrorList
        {
            get
            {
                lst = new List<string>();
                lst.Add("Unable to write data to the transport connection: An existing connection was forcibly closed by the remote host.");

                return lst;
            }
        }
        public static bool CheckError(Exception ex)
        {
            if (ErrorList.Where(x => x.Contains(ex.Message)).Any())
                return true;
            else
                return false;
        }
       
    }
}
