# Modspec.Net

A schema-based client code generator for any Modbus device.

## Introduction

Inspired by work done on building a [C# client for SunSpec](https://github.com/Brill-Power/sunspec.net), Modspec
aims to do the same thing for any Modbus device.

## Features

* static and dynamic clients to suit your use case
* reading of values and, with the dynamic client, writing of values (holding registers only)
* per-value scaling and offsets
* arrays
* repeating groups (see `somebms.json` in the test project for an example)
* discrete inputs mapped as bitfields (`[Flags]`-decorated enums)

## Getting Started

### Code-Generated Static Client

1. Create a schema. The two JSON files included in the test project give some idea of the expected format. e.g.

    ```json
    {
        "name": "MyDevice",
        "groups": [
            {
                "name": "WarningsErrorsEmergencies",
                "baseRegister": 1,
                "table": "DiscreteInputs",
                "points": [
                    {
                        "name": "StringErrors1",
                        "type": "Bitfield16",
                        "symbols": [
                            {
                                "name": "StringOverVoltageWarning",
                                "level": "Warning",
                                "value": 0
                            },
                            {
                                "name": "StringOverVoltageError",
                                "level": "Error",
                                "value": 1
                            },
                            {
                                "name": "StringOverVoltageEmergency",
                                "level": "Emergency",
                                "value": 2
                            },
                        ]
                    }
                ]
            },
            {
                "name": "SystemSettings",
                "baseRegister": 50,
                "table": "HoldingRegisters",
                "points": [
                    {
                        "name": "StringCount",
                        "type": "UInt16",
                        "minValue": 1,
                        "maxValue": 20
                    },
                    {
                        "name": "SystemPowerUpDownControlCommand",
                        "type": "Enum16",
                        "symbols": [
                            {
                                "name": "NoOperation",
                                "value": 0
                            },
                            {
                                "name": "PowerOn",
                                "value": 1
                            },
                            {
                                "name": "PowerOff",
                                "value": 2
                            }
                        ]
                    }
                ]
            },
            {
                "name": "SystemInfo",
                "baseRegister": 1,
                "table": "InputRegisters",
                "points": [
                    {
                        "name": "SystemCircuitBreakerStatus",
                        "type": "UInt16"
                    },
                    {
                        "name": "SystemTotalVoltage",
                        "type": "UInt16",
                        "scaleFactor": 0.1
                    },
                    {
                        "name": "SystemCurrent",
                        "type": "UInt16",
                        "scaleFactor": 0.1,
                        "offset": -1600
                    }
                ]
            }
        ]
    }
    ```

2. Include the generator in your csproj:

    ```xml
    <ItemGroup>
        <ProjectReference Include="../Modspec.Model/Modspec.Model.csproj" OutputItemType="Analyzer"
                          ReferenceOutputAssembly="false" />
    </ItemGroup>
    ```

3. Embed the schema in your csproj as an `AdditionalFile`:

    ```xml
    <ItemGroup>
        <AdditionalFiles Include="myschema.json" />
    </ItemGroup>
    ```

4. Use the client in your code:

    ```csharp
    using MyDevice;

    MyDeviceClient client = new MyDeviceClient(...);
    await client.ReadSystemInfoAsync();
    double totalVoltage = client.SystemTotalVoltage;
    await client.ReadWarningsErrorsEmergenciesAsync();
    if (client.StringErrors1 == StringErrors1.StringOverVoltageWarning)
    {
        // do something
    }
    ```

### Dynamic Client

1. Create a schema (as above).
2. Load the schema into your code as you wish (e.g. using `<EmbeddedResource />`).
3. Instantiate a dynamic client and use:

    ```csharp
    using System;
    using Modspec.Client;
    using Modspec.Model;

    ModspecClient client = new ModspecClient(..., true, schema);
    await client.ReadAllAsync();
    Group? sysInfoGroup = client.Groups.Where(g => g.Group.Name == "SystemInfo").FirstOrDefault();
    Console.WriteLine($"Total Voltage: {sysInfoGroup.Values[1].Value}");
    Group? systemSettingsGroup = client.Groups.Where(g => g.Group.Name == "SystemSettings").FirstOrDefault();
    systemSettingsGroup.Values[0].Value = 10; // set value
    ```

## Contributions

Contributions are most welcome. Notable missing features include write support for coils.
