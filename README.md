sde2string v1.2
===============

This Windows console application will return the Connection Properties of a passed in .sde file. By default this will be in 'Connection String' syntax; ``[Key1]=Value;[Key2]=Value;``. The ``-l`` / ``--list`` switch will list out the properties on separate lines for easier reading, and ``-b`` / ``--bracketless`` will drop the brackets; ``Key1=Value;Key2=Value;``. The full options are detailed below and are also available with the ``--help`` switch (or parameterless invokation).

Usage:
---
  - ``sde2string``, ``sde2string.exe``, ``sde2string -h``, ``sde2string.exe --help``
    - Prints the usage
  - ``sde2string Sample.sde``, ``sde2string -i Sample.sde``, ``sde2string.exe --input Sample.sde``
    - Print the Connection Properties of `Sample.sde` in Connection String format (``[Key]=Value;``)
  - Options:
    - ``-v``, ``--verbose``
    - Write Additional Information
    - ``-l``, ``--list``
    - Print the Connection Properties as a List (one per line)
    - ``-b``, ``--bracketless``
    - Don't bracket the keys (Key=Value;)
    - ``-p``, ``--pause``
    - Pause for a key press before exiting

Latest Changes:
---
  - Added ``--pause`` option
  - Added ``--bracketless`` option
  - Embedded Resources into standalone .exe with Costura.Fody
  - Added ``--list`` option
  - Using ArcInfo licence
  - Initial Release Candidate (2014.7.15)

Release History:
---
  - v1.2 2014.7.16 Adds the --pause option to leave the console window open
  - v1.1 2014.7.16 This embeds the necessary resources for a standalone .exe and adds the --bracketless option
  - v1.0 2014.7.15 This release is a functional parser to read the connection properties from a .sde file

Acknowledgements:
---
Thanks to Michael Juvrud for providing sample .NET ArcObjects code snippets.

Main Contributors:
- Eric Menze ([@Ehryk42](https://twitter.com/Ehryk42))
- Michael Juvrud ([@mikejuvrud](https://twitter.com/mikejuvrud))

Resources:
---
  - [ArcObjects SDK .NET Reference - 10.0](http://help.arcgis.com/en/sdk/10.0/arcobjects_net/componenthelp/index.html#/Overview/001m00000039000000/)
  - [Creating .sde files in ArcCatalog](http://resources.arcgis.com/en/help/main/10.1/index.html#//0017000000pt000000)

Build Requirements:
---
  - Visual Studio (Built with Visual Studio 2013)
  - nuget
    - Packages:
    - [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/) - Command Line Parsing
    - [Fody](https://www.nuget.org/packages/Fody/) - .NET Assembly Weaving
    - [Costura.Fody](https://www.nuget.org/packages/Costura.Fody/) - Making the .exe a standalone executable
  - ESRI ArcGIS Installed (Built with ArcGIS 10.0.3)
    - Libraries:
    - ESRI.ArcGIS.DataSourcesGDB
    - ESRI.ArcGIS.Framework
    - ESRI.ArcGIS.Geodatabase
    - ESRI.ArcGIS.System
    - ESRI.ArcGIS.Version

Contacts:
---
Eric Menze
  - [Email Me](mailto:rhaistlin+gh@gmail.com)
  - [www.ericmenze.com](http://ericmenze.com)
  - [Github](https://github.com/Ehryk)
  - [Twitter](https://twitter.com/Ehryk42)
  - [Source Code](https://github.com/Ehryk/sde2string)
