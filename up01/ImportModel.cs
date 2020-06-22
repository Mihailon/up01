using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace up01
{
    public class ImportModel
    {
        public string Name { get; set; }
        public DateTime date_start { get; set; }

        public int duration { get; set; }

        public int delay { get; set; }

        public DateTime date_end
        {
            get
            {
                DateTime newstart = this.date_start;
                newstart = newstart.AddDays(duration);
                newstart = newstart.AddDays(delay);
                return newstart;
            } 
            set { }
        }
            

        public string responsible { get; set; }

    }
}
