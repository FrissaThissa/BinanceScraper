using BinanceScraper;

Binance binance = new();
while (true)
{
    List<string> symbols = Database.RetrieveSymbols();
    DateTimeOffset epoch = DateTime.Now.AddMonths(-3);
    long minstarttime = epoch.ToUnixTimeMilliseconds();

    foreach (string symbol in symbols)
    {
        Console.WriteLine("Creating datatable for " + symbol);
        Database.CreateDataTable(symbol);
        long starttime = 0;

        Console.WriteLine("Retrieving data for " + symbol);
        while (true)
        {
            List<Datapoint>? datapoints = await binance.RetrieveBatch(symbol, 1000, starttime);
            if (datapoints == null)
                break;
            if (datapoints.Count == 0)
                break;
            if (starttime != 0 && datapoints[0].OpenTime != starttime)
                break;
            if (Database.PushDatapoints(symbol, datapoints) == 0)
                break;
            if (starttime != 0 && starttime < minstarttime)
                break;
            starttime = datapoints[0].OpenTime - 60000000;
        }
    }

    //Sleep 12 hours
    Database.CloseConnection();
    Thread.Sleep(43200000);
}
