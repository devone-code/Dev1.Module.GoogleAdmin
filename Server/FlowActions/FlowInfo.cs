using Dev1.Flow.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin
{
    public class FlowInfo : IFlowInfo
    {
        public FlowDefinition FlowDefinition { get; set; }
        public FlowInfo()
        {

            FlowDefinition = new FlowDefinition
            {
                FriendlyModuleName = "GoogleAdmin",

                CustomFlowEvents = new List<string> (),

            };
        }
    }
}
