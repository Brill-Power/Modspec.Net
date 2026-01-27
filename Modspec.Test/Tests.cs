using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SomeBms;
using SomeEms;
using Modspec.Client;
using Modspec.Model;
using NUnit.Framework;

namespace Modspec.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestLoadSchema()
    {
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Modspec.Test.someems.json");
        Assert.That(stream, Is.Not.Null);
        Schema? schema = Schema.GetSchema(stream);
        Assert.That(schema, Is.Not.Null);
        Assert.That(schema.Groups.Count, Is.EqualTo(5));
        Assert.That(schema.Groups[1].Points.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task TestGeneratedArray()
    {
        MockModbusClient mockClient = new MockModbusClient();
        BinaryPrimitives.WriteUInt16BigEndian(mockClient.InputRegisters.Slice(1000 * 2, 2).Span, 3125);
        BinaryPrimitives.WriteUInt16BigEndian(mockClient.InputRegisters.Slice(1001 * 2, 2).Span, 3000);
        SomeEmsClient hvClient = new SomeEmsClient(mockClient, 100, 480);
        await hvClient.ReadCellVoltagesAsync();
        Assert.That(hvClient.CellVoltages[0], Is.EqualTo(3.125));
        Assert.That(hvClient.CellVoltages[1], Is.EqualTo(3.000));
    }

    [Test]
    public void TestGeneratedArrayBoundsChecking()
    {
        MockModbusClient mockClient = new MockModbusClient();
        Assert.Throws<ArgumentException>(() => new SomeEmsClient(mockClient, 100, 481));
    }

    [Test]
    public void TestGeneratedRepeatingGroups()
    {
        MockModbusClient mockClient = new MockModbusClient();
        SomeBmsClient bmsClient = new SomeBmsClient(mockClient, 2, 2, 480, 100);
        Assert.That(bmsClient.BatteryStrings.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task TestInputRegisters()
    {
        MockModbusClient mockClient = new MockModbusClient();
        SomeBmsClient bmsClient = new SomeBmsClient(mockClient, 2, 2, 480, 100);
        mockClient.DiscreteInputs.Span[1] = 0b10000000;
        await bmsClient.ReadWarningsErrorsEmergenciesAsync();
        Assert.That(bmsClient.StringErrors1, Is.Not.Zero);
        Assert.That(bmsClient.StringErrors1, Is.EqualTo(StringErrors1.StringTerminalDischargeOverCurrentError));
    }

    [Test]
    public void TestErrorLevels()
    {
        StringErrors1 errors1 = 0;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.None));
        errors1 = StringErrors1.StringTerminalResistanceWarning;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.Warning));
        errors1 = StringErrors1.StringTerminalUnderVoltageWarning;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.Warning));
        errors1 = StringErrors1.StringTerminalResistanceError;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.Error)); ;
        errors1 = StringErrors1.StringTerminalDischargeOverCurrentEmergency;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.Emergency));
        errors1 = StringErrors1.StringTerminalDischargeOverCurrentWarning | StringErrors1.StringTerminalOverVoltageError;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.Error));
        errors1 = StringErrors1.StringTerminalUnderVoltageEmergency | StringErrors1.StringTerminalResistanceWarning;
        Assert.That(errors1.GetLevel(), Is.EqualTo(Level.Emergency));
    }

    [Test]
    public async Task TestRangeValidation()
    {
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Modspec.Test.somebms.json");
        Assert.That(stream, Is.Not.Null);
        Schema? schema = Schema.GetSchema(stream);
        Assert.That(schema, Is.Not.Null);
        MockModbusClient mockClient = new MockModbusClient();
        ModspecClient client = new ModspecClient(mockClient, true, schema);
        await client.ReadAllAsync();
        var group = client.Groups.Where(g => g.Group.Table == Table.HoldingRegisters).FirstOrDefault();
        Assert.That(group, Is.Not.Null);
        var value = group.Values[0];
        Assert.That(value.Point.Name, Is.EqualTo("StringNumber"));
        Assert.DoesNotThrow(() => { value.Value = 10; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { value.Value = 25; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { value.Value = 0; });
    }

    [Test]
    public void TestStringEnum()
    {
        Schema schema = new Schema
        {
            Name = "TestStringEnum",
            Groups = [
                new Group
                {
                    Name = "Test",
                    BaseRegister = 0,
                    Table = Table.HoldingRegisters,
                    Points = [
                        new Point
                        {
                            Name = "Language",
                            Type = PointType.Enum16,
                            Symbols = [
                                new Symbol
                                {
                                    Name = "English",
                                    Value = 0,
                                },
                                new Symbol
                                {
                                    Name = "French",
                                    Value = 1,
                                },
                                new Symbol
                                {
                                    Name = "Japanese",
                                    Value = 2,
                                },
                                new Symbol
                                {
                                    Name = "Chinese",
                                    Value = 3,
                                }
                            ]
                        }
                    ]
                },
            ]
        };
        MockModbusClient mockClient = new MockModbusClient();
        ModspecClient client = new ModspecClient(mockClient, true, schema);
        client.ReadAll();
        IModelValue value = client.Groups[0].Values[0];
        Assert.That(value.Point.Name, Is.EqualTo("Language"));
        Assert.That(value.Value, Is.EqualTo("English"));
        BinaryPrimitives.WriteUInt16BigEndian(mockClient.HoldingRegisters.Span, 1);
        client.ReadAll();
        Assert.That(value.Value, Is.EqualTo("French"));
        BinaryPrimitives.WriteUInt16BigEndian(mockClient.HoldingRegisters.Span, 2);
        client.ReadAll();
        Assert.That(value.Value, Is.EqualTo("Japanese"));
        BinaryPrimitives.WriteUInt16BigEndian(mockClient.HoldingRegisters.Span, 3);
        client.ReadAll();
        Assert.That(value.Value, Is.EqualTo("Chinese"));
    }

    [Test]
    public void TestClient()
    {
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Modspec.Test.somebms.json");
        Assert.That(stream, Is.Not.Null);
        Schema? schema = Schema.GetSchema(stream);
        Assert.That(schema, Is.Not.Null);
        MockModbusClient mockClient = new MockModbusClient();
        Assert.DoesNotThrow(() => new ModspecClient(mockClient, true, schema));
    }

    [Test]
    public async Task TestDiscreteInputs()
    {
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Modspec.Test.somebms.json");
        Assert.That(stream, Is.Not.Null);
        Schema? schema = Schema.GetSchema(stream);
        Assert.That(schema, Is.Not.Null);
        MockModbusClient mockClient = new MockModbusClient();
        mockClient.DiscreteInputs.Span[1] = 0b10000000;
        ModspecClient client = new ModspecClient(mockClient, true, schema);
        await client.ReadAllAsync();
        Assert.That(client.Groups.Count, Is.AtLeast(1));
        Assert.That(client.Groups[0].Values.Count, Is.AtLeast(1));
        Assert.That(client.Groups[0].Values[0].Value, Is.EqualTo([nameof(StringErrors1.StringTerminalDischargeOverCurrentError)]));
    }

    private class MockModbusClient : IModbusClient, IReadWriteModbusClient
    {
        private readonly Memory<byte> _coils = new byte[8192];
        private readonly Memory<byte> _discreteInputs = new byte[8192];
        private readonly Memory<byte> _holdingRegisters = new byte[131072];
        private readonly Memory<byte> _inputRegisters = new byte[131072];

        public Memory<byte> Coils => _coils;
        public Memory<byte> DiscreteInputs => _discreteInputs;
        public Memory<byte> HoldingRegisters => _holdingRegisters;
        public Memory<byte> InputRegisters => _inputRegisters;

        private ValueTask ReadAsync(int startingRegister, int count, in Memory<byte> source, ref Memory<byte> destination)
        {
            Span<byte> span = destination.Span;
            Read(startingRegister, count, source, ref span);
            return ValueTask.CompletedTask;
        }

        private void Read(int startingRegister, int count, in Memory<byte> source, ref Span<byte> destination)
        {
            source.Span.Slice(startingRegister, count).CopyTo(destination);
        }

        public ValueTask ReadCoilsAsync(int startingRegister, Memory<byte> destination)
        {
            return ReadAsync(startingRegister, destination.Length, in _coils, ref destination);
        }

        public ValueTask ReadDiscreteInputsAsync(int startingRegister, Memory<byte> destination)
        {
            return ReadAsync(startingRegister, destination.Length, in _discreteInputs, ref destination);
        }

        public ValueTask ReadHoldingRegistersAsync(int startingRegister, Memory<byte> destination)
        {
            return ReadAsync(startingRegister * 2, destination.Length, in _holdingRegisters, ref destination);
        }

        public ValueTask ReadInputRegistersAsync(int startingRegister, Memory<byte> destination)
        {
            return ReadAsync(startingRegister * 2, destination.Length, in _inputRegisters, ref destination);
        }

        public void WriteSingleCoil(int register, bool value)
        {
            Span<byte> slice = _coils.Span.Slice(register / 8, 1);
            if (value)
            {
                slice[0] |= (byte)(1 << (register % 8));
            }
            else
            {
                slice[0] &= (byte)~(1 << (register % 8));
            }
        }

        public void WriteRegisters(int startingRegister, Memory<byte> value)
        {
            value.Span.CopyTo(_holdingRegisters.Span.Slice(startingRegister * 2));
        }

        public void ReadHoldingRegisters(int startingRegister, Span<byte> destination)
        {
            Read(startingRegister * 2, destination.Length, in _holdingRegisters, ref destination);
        }

        public void ReadInputRegisters(int startingRegister, Span<byte> destination)
        {
            Read(startingRegister * 2, destination.Length, in _inputRegisters, ref destination);
        }

        public void ReadCoils(int startingRegister, Span<byte> destination)
        {
            Read(startingRegister, destination.Length, in _coils, ref destination);
        }

        public void ReadDiscreteInputs(int startingRegister, Span<byte> destination)
        {
            Read(startingRegister, destination.Length, in _discreteInputs, ref destination);
        }

        public void Dispose()
        {
        }
    }
}
