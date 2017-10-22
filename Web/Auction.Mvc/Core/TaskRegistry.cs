using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Mvc.Core
{
    public class TaskRegistry : Registry
    {
        public TaskRegistry()
        {
            //Schedules a ScheduledJob : IJob class to run at ApplicationStart and once every 1 minute afterwards 
            Schedule<ScheduledJob>().ToRunNow().AndEvery(30).Seconds();
        }
    }
}