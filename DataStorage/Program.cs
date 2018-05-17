﻿using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace DataStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            RunProgram().GetAwaiter().GetResult();
            Console.ReadKey();
        }

        private static async Task RunProgram()
        {
            try
            {
                var conStr = ConfigurationManager.ConnectionStrings["mysql"].ConnectionString;

                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                // and start it off
                await scheduler.Start();

                // define the job and tie it to our HelloJob class
                IJobDetail job = JobBuilder.Create<RequestJob>()
                    .WithIdentity("job1", "group1").UsingJobData("conStr", conStr)
                    .Build();

                // Trigger the job to run now, and then repeat every 10 seconds
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger1", "group1")
                    .StartNow()                  
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())                    
                    .Build();

                // Tell quartz to schedule the job using our trigger
                await scheduler.ScheduleJob(job, trigger);


                // some sleep to show what's happening
                //await Task.Delay(TimeSpan.FromSeconds(60));

                // and last shut down the scheduler when you are ready to close your program
                //await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
    }
}