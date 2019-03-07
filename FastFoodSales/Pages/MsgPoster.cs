using System;
using Stylet;

namespace DAQ.Pages
{
    public static class MsgPoster
    {
        public static void PostInfo(this IEventAggregator events, string msg)
        {
            events.Publish(new MsgItem()
            {
                Level = "D",
                Time = DateTime.Now,
                Value = msg
            });
        }
        public static void PostError(this IEventAggregator events, string msg)
        {
            events.Publish(new MsgItem()
            {
                Level = "E",
                Time = DateTime.Now,
                Value = msg
            });
        }

        public static void PostWarn(this IEventAggregator events, string msg)
        {
            events.Publish(new MsgItem()
            {
                Level = "W",
                Time = DateTime.Now,
                Value = msg
            });
        }
    }
}