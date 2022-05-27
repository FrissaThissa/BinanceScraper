using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceScraper
{
    public class Datapoint
    {
        public long OpenTime { get; private set; }
        public double Open { get; private set; }
        public double High { get; private set; }
        public double Low { get; private set; }
        public double Close { get; private set; }
        public double Volume { get; private set; }
        public long CloseTime { get; private set; }
        public int NumberOfTrades { get; private set; }

        public Datapoint(long opentime, double open, double high, double low, double close, double volume, long closetime, int numberoftrades)
        {
            OpenTime = opentime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            CloseTime = closetime;
            NumberOfTrades = numberoftrades;
        }
    }
}
