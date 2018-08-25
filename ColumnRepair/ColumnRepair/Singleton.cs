using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnRepair
{
    public class Singleton
    {
        public static Singleton Instance
        {
            get;
            set;
        }
        private RevitData revitData;
        private WPFData wpfData;
        private OtherData otherData;
        public RevitData RevitData
        {
            get
            {
                if (revitData == null) revitData = new RevitData();
                return revitData;
            }
        }
        public WPFData WPFData
        {
            get
            {
                if (wpfData == null) wpfData = new WPFData();
                return wpfData;
            }
        }
        public OtherData OtherData
        {
            get
            {
                if (otherData == null) otherData = new OtherData();
                return otherData;
            }
        }
    }
}
