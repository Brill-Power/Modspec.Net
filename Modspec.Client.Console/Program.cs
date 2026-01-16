using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrillPower.FluentModbus;
using Modspec.Client.FluentModbus;
using Modspec.Model;

namespace Modspec.Client.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        using FileStream file = new FileStream(args[0], FileMode.Open, FileAccess.Read);
        Schema schema = Schema.GetSchema(file);
        if (args.Length > 1)
        {
            ModbusTcpClient tcpClient = new ModbusTcpClient();
            tcpClient.Connect(args[1]);
            FluentModbusClient fmc = new FluentModbusClient(tcpClient, 1);
            ModspecClient client = new ModspecClient(fmc, true, schema);
            await client.ReadAllAsync();
            DumpGroups(client.Groups);
            foreach (BoundRepeatingGroup repeatingGroup in client.RepeatingGroups)
            {
                foreach (var entry in repeatingGroup.Entries)
                {
                    await entry.ReadAllAsync();
                    System.Console.WriteLine($"{repeatingGroup.RepeatingGroup.Name}[{entry.Index}]");
                    DumpGroups(entry.Groups, "  ");
                }
            }
        }
    }

    private static void DumpGroups(IReadOnlyCollection<BoundGroup> groups, string prefix = "")
    {
        foreach (BoundGroup group in groups)
        {
            System.Console.WriteLine($"{prefix}{group.Group.Name}");
            foreach (IModelValue value in group.Values)
            {
                System.Console.WriteLine($"{prefix}- {value.Point.Name} ({value.ModiconId}): {Render(value.Value)}");
            }
        }
    }

    private static string Render(object? value)
    {
        if (value is null)
        {
            return String.Empty;
        }

        if (value is IList array)
        {
            return String.Join(",", array.OfType<object>().Select(o => Render(o)));
        }

        return value.ToString()!;
    }
}