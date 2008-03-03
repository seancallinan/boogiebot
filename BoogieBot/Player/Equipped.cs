using System;
using System.Collections.Generic;
using System.Text;

namespace BoogieBot.Common
{
    public class Equipped
    {
        public Equipped(Object po)
        {
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Equipped Class:\n");

            return sb.ToString();
        }
    }
}
